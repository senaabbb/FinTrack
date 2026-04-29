using FinTrack.DTOs;
using FinTrack.Interfaces;
using FinTrack.Models;
using FinTrack.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinTrack.Tests.Services
{
    public class StockServiceTests
    {
        private readonly Mock<IStockRepository> _mockStockRepo;
        private readonly Mock<IStockPriceRepository> _mockPriceRepo;
        private readonly Mock<IFinnhubClient> _mockFinnhubClient;
        private readonly Mock<ILogger<StockService>> _mockLogger;

        public StockServiceTests()
        {
            _mockStockRepo = new Mock<IStockRepository>();
            _mockPriceRepo = new Mock<IStockPriceRepository>();
            _mockFinnhubClient = new Mock<IFinnhubClient>();
            _mockLogger = new Mock<ILogger<StockService>>();
        }

        private StockService CreateService() => new StockService(
            _mockStockRepo.Object,
            _mockPriceRepo.Object,
            _mockFinnhubClient.Object,
            _mockLogger.Object
        );

        // ═══════════════════════════════════════════════════════════════
        // TEST 1: Duplicate hisse ekleme koruması
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public async Task AddStockAsync_ShouldThrowException_WhenStockAlreadyExists()
        {

            var dto = new CreateStockDto
            {
                Symbol = "AAPL",
                CompanyName = "Apple Inc.",
                Sector = "Technology"
            };

            _mockStockRepo
                .Setup(r => r.ExistsAsync("AAPL"))
                .ReturnsAsync(true);

            var service = CreateService();

            await service
                .Invoking(s => s.AddStockAsync(dto))
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*AAPL*");

            _mockStockRepo.Verify(r => r.ExistsAsync("AAPL"), Times.Once);

            _mockStockRepo.Verify(r => r.AddAsync(It.IsAny<Stock>()), Times.Never);
        }

        // ═══════════════════════════════════════════════════════════════
        // TEST 2: Başarılı hisse ekleme
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public async Task AddStockAsync_ShouldReturnStockDto_WhenStockIsNew()
        {
            var dto = new CreateStockDto
            {
                Symbol = "TSLA",
                CompanyName = "Tesla Inc.",
                Sector = "Automotive"
            };

            _mockStockRepo
                .Setup(r => r.ExistsAsync("TSLA"))
                .ReturnsAsync(false);

            _mockStockRepo
                .Setup(r => r.AddAsync(It.IsAny<Stock>()))
                .ReturnsAsync(new Stock
                {
                    Id = 1,
                    Symbol = "TSLA",
                    CompanyName = "Tesla Inc.",
                    Sector = "Automotive",
                    CreatedAt = DateTime.UtcNow
                });

            var service = CreateService();

            var result = await service.AddStockAsync(dto);

            result.Should().NotBeNull();
            result.Symbol.Should().Be("TSLA");
            result.CompanyName.Should().Be("Tesla Inc.");
            result.Sector.Should().Be("Automotive");

            _mockStockRepo.Verify(r => r.AddAsync(It.IsAny<Stock>()), Times.Once);
        }

        // ═══════════════════════════════════════════════════════════════
        // TEST 3: Analytics — Fiyat geçmişi yokken boş liste dönmeli
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetTopGrowingStocksAsync_ShouldReturnEmpty_WhenNoPriceHistory()
        {

            var stocksWithNoPrices = new List<Stock>
            {
                new Stock
                {
                    Id = 1,
                    Symbol = "AAPL",
                    CompanyName = "Apple Inc.",
                    Sector = "Technology",
                    Prices = new List<StockPrice>()
                    // Prices bos — hic fiyat cekilmemis
                }
            };

            _mockStockRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(stocksWithNoPrices);

            var service = CreateService();

            var result = await service.GetTopGrowingStocksAsync(5);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // ═══════════════════════════════════════════════════════════════
        // TEST 4: Analytics — Büyüme doğru hesaplanıyor mu?
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetTopGrowingStocksAsync_ShouldCalculateGrowthCorrectly()
        {
            var stocksWithPrices = new List<Stock>
            {
                new Stock
                {
                    Id = 1,
                    Symbol = "AAPL",
                    CompanyName = "Apple Inc.",
                    Sector = "Technology",
                    Prices = new List<StockPrice>
                    {
                        new StockPrice
                        {
                            Price = 100m,
                            FetchedAt = DateTime.UtcNow.AddDays(-7)
                        },
                        new StockPrice
                        {
                            Price = 150m,
                            FetchedAt = DateTime.UtcNow
                        }
                    }
                }
            };

            _mockStockRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(stocksWithPrices);

            var service = CreateService();

            var result = await service.GetTopGrowingStocksAsync(5);
            var topStock = result.First();

            result.Should().HaveCount(1);

            topStock.Symbol.Should().Be("AAPL");
            topStock.OldestPrice.Should().Be(100m);
            topStock.LatestPrice.Should().Be(150m);
            topStock.GrowthAmount.Should().Be(50m);
            topStock.GrowthPercentage.Should().Be(50m);
        }

        // ═══════════════════════════════════════════════════════════════
        // TEST 5: Fiyat çekme — Watchlist'te olmayan hisse
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public async Task FetchAndSavePriceAsync_ShouldReturnNull_WhenStockNotInWatchlist()
        {
            _mockStockRepo
                .Setup(r => r.GetBySymbolAsync("XYZ"))
                .ReturnsAsync((Stock?)null);

            var service = CreateService();

            var result = await service.FetchAndSavePriceAsync("XYZ");

            result.Should().BeNull();

            _mockFinnhubClient.Verify(
                c => c.GetStockQuoteAsync(It.IsAny<string>()),
                Times.Never
            );
        }
    }
}
