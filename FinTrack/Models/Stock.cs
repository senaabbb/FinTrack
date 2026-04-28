namespace FinTrack.Models
{
    public class Stock
    {
        public int Id { get; set; } 
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<StockPrice> Prices { get; set; } = new List<StockPrice>();
    }
}
