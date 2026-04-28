using FinTrack.DTOs;

namespace FinTrack.Interfaces
{
    public interface IStockService
    {
        Task<IEnumerable<StockDto>> GetAllStocksAsync();

        Task<StockDto> AddStockAsync(CreateStockDto dto);

        Task<bool> DeleteStockAsync(string symbol);

        Task<StockPriceDto?> FetchAndSavePriceAsync(string symbol);

        Task<IEnumerable<StockAnalyticsDto>> GetTopGrowingStocksAsync(int count);
    }
}
