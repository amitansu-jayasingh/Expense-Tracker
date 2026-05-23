using ExpenseTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(AppDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var totalSpent = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.Date.Month == currentMonth &&
                    e.Date.Year == currentYear)
                .SumAsync(e => e.Amount);

            var totalExpenses = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.Date.Month == currentMonth &&
                    e.Date.Year == currentYear)
                .CountAsync();

            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId &&
                    b.Month == currentMonth &&
                    b.Year == currentYear);

            var remaining = budget != null ?
                budget.MonthlyLimit - totalSpent : 0;

            var recentExpenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .Take(5)
                .ToListAsync();

            var categorySpending = await _context.Expenses
                .Include(e => e.Category)
                .Where(e =>
                    e.UserId == userId &&
                    e.Date.Month == currentMonth &&
                    e.Date.Year == currentYear)
                .GroupBy(e => e.Category!.Name)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(e => e.Amount)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            ViewBag.TotalSpent = totalSpent;
            ViewBag.TotalExpenses = totalExpenses;
            ViewBag.Remaining = remaining;
            ViewBag.Budget = budget;
            ViewBag.RecentExpenses = recentExpenses;
            ViewBag.CategorySpending = categorySpending;
            ViewBag.CurrentMonth = DateTime.Now.ToString("MMMM yyyy");

            return View();
        }
    }
}