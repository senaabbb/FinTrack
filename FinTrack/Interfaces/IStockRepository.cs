using FinTrack.Models;

namespace FinTrack.Interfaces
{
    public interface IStockRepository
    {
        Task<IEnumerable<Stock>> GetAllAsync();

        Task<Stock?> GetBySymbolAsync(string symbol);

        Task<Stock> AddAsync(Stock stock);

        Task<bool> DeleteAsync(string symbol);

        Task<bool> ExistsAsync(string symbol);
    }
}
