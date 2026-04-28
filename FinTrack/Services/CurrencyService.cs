using FinTrack.DTOs;
using FinTrack.Interfaces;
using FinTrack.Models;

namespace FinTrack.Services
{
    public class CurrencyService : ICurrencyService
    {

        private readonly ICurrencyRepository _currencyRepository;
        private readonly IFinnhubClient _finnhubClient;
        private readonly ILogger<CurrencyService> _logger;

        public CurrencyService(
            ICurrencyRepository currencyRepository,
            IFinnhubClient finnhubClient,
            ILogger<CurrencyService> logger)
        {
            _currencyRepository = currencyRepository;
            _finnhubClient = finnhubClient;
            _logger = logger;
        }

        public async Task<CurrencyDto?> FetchAndSaveRateAsync(string from, string to)
        {
            // Finnhub'dan baz para biriminin tüm kurlarını çek
            var rates = await _finnhubClient.GetForexRatesAsync(from);

            if (rates == null || !rates.Quote.ContainsKey(to.ToUpper()))
            {
                _logger.LogWarning("Could not fetch rate for {From}/{To}", from, to);
                return null;
            }

            var rate = rates.Quote[to.ToUpper()];

            // Kuru veritabanına kaydet
            var currency = new Currency
            {
                FromCurrency = from.ToUpper(),
                ToCurrency = to.ToUpper(),
                Rate = rate,
                FetchedAt = DateTime.UtcNow
            };

            await _currencyRepository.AddAsync(currency);

            return new CurrencyDto
            {
                FromCurrency = currency.FromCurrency,
                ToCurrency = currency.ToCurrency,
                Rate = currency.Rate,
                FetchedAt = currency.FetchedAt
            };
        }

        public async Task<IEnumerable<CurrencyDto>> GetAllRatesAsync()
        {
            var currencies = await _currencyRepository.GetAllAsync();

            return currencies.Select(c => new CurrencyDto
            {
                FromCurrency = c.FromCurrency,
                ToCurrency = c.ToCurrency,
                Rate = c.Rate,
                FetchedAt = c.FetchedAt
            });
        }
    }
}
