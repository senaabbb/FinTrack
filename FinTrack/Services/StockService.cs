using FinTrack.DTOs;
using FinTrack.Interfaces;
using FinTrack.Models;

namespace FinTrack.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IStockPriceRepository _stockPriceRepository;
        private readonly IFinnhubClient _finnhubClient;
        private readonly ILogger<StockService> _logger;

        public StockService(
            IStockRepository stockRepository,
            IStockPriceRepository stockPriceRepository,
            IFinnhubClient finnhubClient,
            ILogger<StockService> logger)
        {
            _stockRepository = stockRepository;
            _stockPriceRepository = stockPriceRepository;
            _finnhubClient = finnhubClient;
            _logger = logger;
        }

        public async Task<IEnumerable<StockDto>> GetAllStocksAsync()
        {
            var stocks = await _stockRepository.GetAllAsync();

            // Her Stock modelini StockDto'ya çevir
            return stocks.Select(s => new StockDto
            {
                Id = s.Id,
                Symbol = s.Symbol,
                CompanyName = s.CompanyName,
                Sector = s.Sector,
                CreatedAt = s.CreatedAt,

                // Fiyat geçmişi varsa en son fiyatı ekle
                LatestPrice = s.Prices.Any()
                    ? s.Prices.OrderByDescending(p => p.FetchedAt).First().Price
                    : null,

                LatestPriceFetchedAt = s.Prices.Any()
                    ? s.Prices.OrderByDescending(p => p.FetchedAt).First().FetchedAt
                    : null
            });
        }

        public async Task<StockDto> AddStockAsync(CreateStockDto dto)
        {
            // Aynı sembol zaten watchlist'te var mı kontrol et
            var exists = await _stockRepository.ExistsAsync(dto.Symbol);
            if (exists)
                throw new InvalidOperationException($"Stock '{dto.Symbol}' is already in the watchlist.");

            var stock = new Stock
            {
                Symbol = dto.Symbol.ToUpper(),
                CompanyName = dto.CompanyName,
                Sector = dto.Sector
            };

            var saved = await _stockRepository.AddAsync(stock);

            return new StockDto
            {
                Id = saved.Id,
                Symbol = saved.Symbol,
                CompanyName = saved.CompanyName,
                Sector = saved.Sector,
                CreatedAt = saved.CreatedAt
            };
        }

        public async Task<bool> DeleteStockAsync(string symbol)
        {
            return await _stockRepository.DeleteAsync(symbol);
        }

        public async Task<StockPriceDto?> FetchAndSavePriceAsync(string symbol)
        {
            // Hisse watchlist'te var mı kontrol et
            var stock = await _stockRepository.GetBySymbolAsync(symbol);
            if (stock == null)
            {
                _logger.LogWarning("Stock {Symbol} not found in watchlist", symbol);
                return null;
            }

            // Finnhub'dan anlık fiyatı çek
            var quote = await _finnhubClient.GetStockQuoteAsync(symbol);
            if (quote == null)
            {
                _logger.LogWarning("Could not fetch price for {Symbol} from Finnhub", symbol);
                return null;
            }

            // Fiyatı veritabanına kaydet
            var stockPrice = new StockPrice
            {
                StockId = stock.Id,
                Price = quote.CurrentPrice,
                OpenPrice = quote.OpenPrice,
                HighPrice = quote.HighPrice,
                LowPrice = quote.LowPrice,
                Currency = "USD",
                FetchedAt = DateTime.UtcNow
            };

            await _stockPriceRepository.AddAsync(stockPrice);

            return new StockPriceDto
            {
                Symbol = symbol.ToUpper(),
                CurrentPrice = quote.CurrentPrice,
                OpenPrice = quote.OpenPrice,
                HighPrice = quote.HighPrice,
                LowPrice = quote.LowPrice,
                Currency = "USD",
                FetchedAt = stockPrice.FetchedAt
            };
        }

        public async Task<IEnumerable<StockAnalyticsDto>> GetTopGrowingStocksAsync(int count)
        {
            var stocks = await _stockRepository.GetAllAsync();
            var result = new List<StockAnalyticsDto>();

            foreach (var stock in stocks)
            {
                // En az 2 fiyat kaydı olmayanları analiz edemeyiz
                if (stock.Prices.Count < 2)
                    continue;

                var ordered = stock.Prices.OrderBy(p => p.FetchedAt).ToList();
                var oldest = ordered.First().Price;
                var latest = ordered.Last().Price;

                // Sıfıra bölme hatasını önle
                if (oldest == 0)
                    continue;

                var growthAmount = latest - oldest;
                var growthPercentage = (growthAmount / oldest) * 100;

                result.Add(new StockAnalyticsDto
                {
                    Symbol = stock.Symbol,
                    CompanyName = stock.CompanyName,
                    LatestPrice = latest,
                    OldestPrice = oldest,
                    GrowthAmount = Math.Round(growthAmount, 2),
                    GrowthPercentage = Math.Round(growthPercentage, 2)
                });
            }

            // En çok büyüyenden en aza doğru sırala ve ilk N tanesini döndür
            // Strategy Pattern: Sıralama mantığı buraya izole edildi
            // Farklı sıralama stratejileri (by volume, by price) kolayca eklenebilir
            return result
                .OrderByDescending(r => r.GrowthPercentage)
                .Take(count);
        }
    }
}
