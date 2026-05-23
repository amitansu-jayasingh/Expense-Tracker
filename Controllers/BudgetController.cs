using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BudgetController(AppDbContext context,
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

            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId &&
                    b.Month == currentMonth &&
                    b.Year == currentYear);

            var totalSpent = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.Date.Month == currentMonth &&
                    e.Date.Year == currentYear)
                .SumAsync(e => e.Amount);

            ViewBag.TotalSpent = totalSpent;
            ViewBag.CurrentMonth = DateTime.Now.ToString("MMMM yyyy");

            return View(budget);
        }

        [HttpGet]
        public async Task<IActionResult> SetBudget()
        {
            var userId = _userManager.GetUserId(User)!;
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var existing = await _context.Budgets
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId &&
                    b.Month == currentMonth &&
                    b.Year == currentYear);

            return View(existing ?? new Budget
            {
                Month = currentMonth,
                Year = currentYear
            });
        }

        [HttpPost]
        public async Task<IActionResult> SetBudget(Budget budget)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User)!;
                budget.UserId = userId;

                var existing = await _context.Budgets
                    .FirstOrDefaultAsync(b =>
                        b.UserId == userId &&
                        b.Month == budget.Month &&
                        b.Year == budget.Year);

                if (existing != null)
                {
                    existing.MonthlyLimit = budget.MonthlyLimit;
                    _context.Budgets.Update(existing);
                }
                else
                {
                    _context.Budgets.Add(budget);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(budget);
        }
    }
}