using FinTrack.DTOs;
using FinTrack.Interfaces;
using System.Text.Json;

namespace FinTrack.ExternalClients
{
    public class FinnhubClient : IFinnhubClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<FinnhubClient> _logger;

        public FinnhubClient(HttpClient httpClient, IConfiguration configuration, ILogger<FinnhubClient> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Finnhub:ApiKey"] ?? throw new ArgumentNullException("Finnhub API key is missing");
            _logger = logger;

            // Base URL'yi HttpClient'a tanıt
            _httpClient.BaseAddress = new Uri(configuration["Finnhub:BaseUrl"]
                ?? "https://finnhub.io/api/v1");
        }

        public async Task<FinnhubQuoteDto?> GetStockQuoteAsync(string symbol)
        {
            try
            {
                // /quote?symbol=AAPL&token=API_KEY şeklinde istek at
                var url = $"/api/v1/quote?symbol={symbol.ToUpper()}&token={_apiKey}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Finnhub quote request failed for {Symbol}. Status: {Status}",
                        symbol, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();

                // JSON'u FinnhubQuoteDto nesnesine çevir
                var result = JsonSerializer.Deserialize<FinnhubQuoteDto>(json);

                // Finnhub bazen boş veri döner (geçersiz sembol için c=0)
                if (result == null || result.CurrentPrice == 0)
                {
                    _logger.LogWarning("No price data returned for symbol: {Symbol}", symbol);
                    return null;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quote for symbol: {Symbol}", symbol);
                return null;
            }
        }
        public async Task<FinnhubCurrencyDto?> GetForexRatesAsync(string baseCurrency)
        {
            try
            {
                // Finnhub forex endpoint'i premium olduğu için
                // ExchangeRate-API kullanıyoruz (ücretsiz, kayıt gerektirmez)
                var url = $"https://open.er-api.com/v6/latest/{baseCurrency.ToUpper()}";

                // Yeni bir HttpClient oluştur — farklı base URL kullanıyoruz
                using var client = new HttpClient();
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("ExchangeRate-API request failed for {Currency}. Status: {Status}",
                        baseCurrency, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();

                // ExchangeRate-API'nin döndüğü JSON yapısı:
                // { "base_code": "USD", "rates": { "TRY": 32.50, "EUR": 0.92 } }
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // result: "success" mi kontrol et
                if (root.GetProperty("result").GetString() != "success")
                    return null;

                var rates = root.GetProperty("rates");
                var quoteDict = new Dictionary<string, decimal>();

                // Tüm kurları dictionary'e aktar
                foreach (var rate in rates.EnumerateObject())
                {
                    quoteDict[rate.Name] = rate.Value.GetDecimal();
                }

                return new FinnhubCurrencyDto
                {
                    Base = baseCurrency.ToUpper(),
                    Quote = quoteDict
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching forex rates for: {Currency}", baseCurrency);
                return null;
            }
        }
        /*
        public async Task<FinnhubCurrencyDto?> GetForexRatesAsync(string baseCurrency)
        {
            try
            {
                // /forex/rates?base=USD&token=API_KEY şeklinde istek at
                var url = $"/api/v1/forex/rates?base={baseCurrency.ToUpper()}&token={_apiKey}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Finnhub forex request failed for {Currency}. Status: {Status}",
                        baseCurrency, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<FinnhubCurrencyDto>(json);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching forex rates for: {Currency}", baseCurrency);
                return null;
            }
        }*/
    }
}
