namespace FinTrack.Models
{
    public class StockPrice
    {
        public int Id { get; set; }
        public int StockId { get; set; }
        public decimal Price { get; set; }
        public decimal? OpenPrice { get; set; }
        public decimal? HighPrice { get; set; }
        public decimal? LowPrice { get; set; }
        public string? Currency { get; set; } = "USD";
        public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
        public Stock Stock { get; set; } = null!;
    }
}
