namespace FinTrack.DTOs
{
    public class StockDto
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Son fiyat bilgisi — varsa göster
        public decimal? LatestPrice { get; set; }
        public DateTime? LatestPriceFetchedAt { get; set; }
    }
}
