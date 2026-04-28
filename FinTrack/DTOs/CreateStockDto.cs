namespace FinTrack.DTOs
{
    public class CreateStockDto
    {
        public string Symbol { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public string Sector { get; set; } = string.Empty;
    }
}
