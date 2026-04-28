using FinTrack.DTOs;

namespace FinTrack.Interfaces
{
    public interface IFinnhubClient
    {
        Task<FinnhubQuoteDto?> GetStockQuoteAsync(string symbol);

        Task<FinnhubCurrencyDto?> GetForexRatesAsync(string baseCurrency);
    }
}
