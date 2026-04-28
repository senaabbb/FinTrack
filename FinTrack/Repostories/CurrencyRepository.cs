using FinTrack.Data;
using FinTrack.Interfaces;
using FinTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Repostories
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly AppDbContext _context;

        public CurrencyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Currency> AddAsync(Currency currency)
        {
            _context.Currencies.Add(currency);
            await _context.SaveChangesAsync();
            return currency;
        }

        public async Task<Currency?> GetLatestRateAsync(string from, string to)
        {
            return await _context.Currencies
                .Where(c => c.FromCurrency.ToUpper() == from.ToUpper() &&
                            c.ToCurrency.ToUpper() == to.ToUpper())
                .OrderByDescending(c => c.FetchedAt)
                .FirstOrDefaultAsync();

        }

        public async Task<IEnumerable<Currency>> GetAllAsync()
        {
            return await _context.Currencies
                .OrderByDescending(c => c.FetchedAt)
                .ToListAsync();
        }
    }
}
