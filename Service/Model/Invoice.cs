using System;

namespace Service.Model
{
    public class Invoice
    {
        public string Vendor { get; set; }
        public string Description { get; set; }
        public DateTime? Date { get; set; }
        public string DateText { get; set; }
        public Expense ExpenseDetail { get; set; }
    }
}
