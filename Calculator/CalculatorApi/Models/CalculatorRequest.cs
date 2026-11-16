namespace CalculatorApi.Models
{
    public class CalculatorRequest
    {
        public decimal DailyCost { get; set; }
        public int Days { get; set; }
        public bool IncludeInsurance { get; set; }
    }
}