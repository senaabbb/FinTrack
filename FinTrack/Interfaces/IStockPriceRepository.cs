using FinTrack.Models;

namespace FinTrack.Interfaces
{
    public interface IStockPriceRepository
    {
        Task<StockPrice> AddAsync(StockPrice stockPrice);

        Task<IEnumerable<StockPrice>> GetByStockIdAsync(int stockId);

        Task<IEnumerable<StockPrice>> GetLatestBySymbolAsync(string symbol, int count = 10);
    }
}
