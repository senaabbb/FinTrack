using FinTrack.Models;

namespace FinTrack.Interfaces
{
    public interface ICurrencyRepository
    {
        Task<Currency> AddAsync(Currency currency);

        Task<Currency?> GetLatestRateAsync(string from, string to);

        Task<IEnumerable<Currency>> GetAllAsync();
    }
}
