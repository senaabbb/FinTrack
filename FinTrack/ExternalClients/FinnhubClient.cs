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
        }
    }
}
