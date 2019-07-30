namespace Service.Model
{
    public class Expense
    {
        public string CostCentre { get; set; }
        public decimal TotalIncludingTax { get; set; }
        public string PaymentMethod { get; set; }
        public decimal GstTax { get; set; }
        public decimal TotalExcludingGst { get; set; }
    }
}
