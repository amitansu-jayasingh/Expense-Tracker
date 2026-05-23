using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models
{
    public class Budget
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyLimit { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public string? UserId { get; set; }
    }
}