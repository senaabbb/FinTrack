using FinTrack.DTOs;

namespace FinTrack.Interfaces
{
    public interface ICurrencyService
    {
        Task<CurrencyDto?> FetchAndSaveRateAsync(string from, string to);

        Task<IEnumerable<CurrencyDto>> GetAllRatesAsync();
    }
}
