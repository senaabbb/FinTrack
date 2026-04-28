using FinTrack.Data;
using FinTrack.Interfaces;
using FinTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Repostories
{
    public class StockPriceRepository : IStockPriceRepository
    {
        private readonly AppDbContext _context;

        public StockPriceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StockPrice> AddAsync(StockPrice stockPrice)
        {
            _context.StockPrices.Add(stockPrice);
            await _context.SaveChangesAsync();
            return stockPrice;
        }

        public async Task<IEnumerable<StockPrice>> GetByStockIdAsync(int stockId)
        {
            return await _context.StockPrices
                .Where(sp => sp.StockId == stockId)
                .OrderByDescending(sp => sp.FetchedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockPrice>> GetLatestBySymbolAsync(string symbol, int count = 10)
        {
            return await _context.StockPrices
                .Include(sp => sp.Stock)
                .Where(sp => sp.Stock.Symbol.ToUpper() == symbol.ToUpper())
                .OrderByDescending(sp => sp.FetchedAt)
                .Take(count) // Sadece ilk N kaydı al
                .ToListAsync();
        }
    }
}
