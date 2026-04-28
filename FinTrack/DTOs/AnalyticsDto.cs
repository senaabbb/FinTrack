namespace FinTrack.DTOs
{
    public class StockAnalyticsDto
    {
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public decimal LatestPrice { get; set; }
        public decimal OldestPrice { get; set; }
        public decimal GrowthAmount { get; set; }
        public decimal GrowthPercentage { get; set; }
    }
}
