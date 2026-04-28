namespace FinTrack.DTOs
{
    public class StockPriceDto
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public decimal? OpenPrice { get; set; }
        public decimal? HighPrice { get; set; }
        public decimal? LowPrice { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime FetchedAt { get; set; }
    }
}
