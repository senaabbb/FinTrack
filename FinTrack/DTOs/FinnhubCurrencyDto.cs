using System.Text.Json.Serialization;

namespace FinTrack.DTOs
{
    public class FinnhubCurrencyDto
    {
        [JsonPropertyName("base")]
        public string Base { get; set; } = string.Empty;
        // Baz para birimi (örn: "USD")

        [JsonPropertyName("quote")]
        public Dictionary<string, decimal> Quote { get; set; } = new();
        // Diğer para birimleri ve kurları
        // Örnek: { "TRY": 32.50, "EUR": 0.92, "GBP": 0.79 }
    }
}
