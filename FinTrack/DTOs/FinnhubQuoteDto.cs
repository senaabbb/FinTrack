using System.Text.Json.Serialization;

namespace FinTrack.DTOs
{
    public class FinnhubQuoteDto
    {
        [JsonPropertyName("c")]
        public decimal CurrentPrice { get; set; }
        // c = current price (anlık fiyat)

        [JsonPropertyName("o")]
        public decimal OpenPrice { get; set; }
        // o = open price (günün açılış fiyatı)

        [JsonPropertyName("h")]
        public decimal HighPrice { get; set; }
        // h = high price (günün en yüksek fiyatı)

        [JsonPropertyName("l")]
        public decimal LowPrice { get; set; }
        // l = low price (günün en düşük fiyatı)

        [JsonPropertyName("pc")]
        public decimal PreviousClosePrice { get; set; }
        // pc = previous close (bir önceki kapanış fiyatı)
    }
}
