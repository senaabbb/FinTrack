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
    public class CurrencyServiceTests
    {
        private readonly Mock<ICurrencyRepository> _mockCurrencyRepo;
        private readonly Mock<IFinnhubClient> _mockFinnhubClient;
        private readonly Mock<ILogger<CurrencyService>> _mockLogger;

        public CurrencyServiceTests()
        {
            _mockCurrencyRepo = new Mock<ICurrencyRepository>();
            _mockFinnhubClient = new Mock<IFinnhubClient>();
            _mockLogger = new Mock<ILogger<CurrencyService>>();
        }

        private CurrencyService CreateService() => new CurrencyService(
            _mockCurrencyRepo.Object,
            _mockFinnhubClient.Object,
            _mockLogger.Object
        );

        // ═══════════════════════════════════════════════════════════════
        // TEST 1: Desteklenmeyen kur çifti — null dönmeli
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public async Task FetchAndSaveRateAsync_ShouldReturnNull_WhenCurrencyNotSupported()
        {
            _mockFinnhubClient
                .Setup(c => c.GetForexRatesAsync("USD"))
                .ReturnsAsync(new FinnhubCurrencyDto
                {
                    Base = "USD",
                    Quote = new Dictionary<string, decimal>
                    {
                        { "TRY", 32.50m },
                        { "EUR", 0.92m }
                        // "XYZ" yok!
                    }
                });

            var service = CreateService();

            var result = await service.FetchAndSaveRateAsync("USD", "XYZ");

            result.Should().BeNull();

            _mockCurrencyRepo.Verify(
                r => r.AddAsync(It.IsAny<Currency>()),
                Times.Never
            );
        }

        // ═══════════════════════════════════════════════════════════════
        // TEST 2: Başarılı kur çekme — DB'ye kaydedilmeli
        // ═══════════════════════════════════════════════════════════════

        [Fact]
        public async Task FetchAndSaveRateAsync_ShouldReturnCurrencyDto_WhenRateExists()
        {
            _mockFinnhubClient
                .Setup(c => c.GetForexRatesAsync("USD"))
                .ReturnsAsync(new FinnhubCurrencyDto
                {
                    Base = "USD",
                    Quote = new Dictionary<string, decimal>
                    {
                        { "TRY", 32.50m },
                        { "EUR", 0.92m }
                    }
                });

            _mockCurrencyRepo
                .Setup(r => r.AddAsync(It.IsAny<Currency>()))
                .ReturnsAsync(new Currency
                {
                    Id = 1,
                    FromCurrency = "USD",
                    ToCurrency = "TRY",
                    Rate = 32.50m,
                    FetchedAt = DateTime.UtcNow
                });

            var service = CreateService();

            var result = await service.FetchAndSaveRateAsync("USD", "TRY");

            result.Should().NotBeNull();
            result!.FromCurrency.Should().Be("USD");
            result.ToCurrency.Should().Be("TRY");
            result.Rate.Should().Be(32.50m);

            _mockCurrencyRepo.Verify(
                r => r.AddAsync(It.IsAny<Currency>()),
                Times.Once
            );
        }
    }
}
