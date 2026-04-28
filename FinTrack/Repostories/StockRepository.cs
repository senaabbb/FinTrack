using FinTrack.Data;
using FinTrack.Interfaces;
using FinTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Repostories
{
    public class StockRepository : IStockRepository
    {
        private readonly AppDbContext _context;

        public StockRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Stock>> GetAllAsync()
        {
            // Hisseleri fiyat geçmişleriyle birlikte getir
            return await _context.Stocks
                .Include(s => s.Prices)
                .ToListAsync();
        }

        public async Task<Stock?> GetBySymbolAsync(string symbol)
        {
            return await _context.Stocks
                .Include(s => s.Prices)
                .FirstOrDefaultAsync(s => s.Symbol.ToUpper() == symbol.ToUpper());
            // ToUpper: "aapl" ile "AAPL" aynı sonucu versin
        }

        public async Task<Stock> AddAsync(Stock stock)
        {
            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();
            return stock;
        }

        public async Task<bool> DeleteAsync(string symbol)
        {
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.Symbol.ToUpper() == symbol.ToUpper());

            if (stock == null)
                return false;

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string symbol)
        {
            return await _context.Stocks
                .AnyAsync(s => s.Symbol.ToUpper() == symbol.ToUpper());
        }
    }
}
