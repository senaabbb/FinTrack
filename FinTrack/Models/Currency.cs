namespace FinTrack.Models
{
    public class Currency
    {
        public int Id { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    }
}
