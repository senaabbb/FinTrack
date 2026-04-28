using FinTrack.DTOs;
using FinTrack.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [controller] otomatik olarak "stocks" olur — sınıf adından türetilir
    public class StocksController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly ILogger<StocksController> _logger;

        public StocksController(IStockService stockService, ILogger<StocksController> logger)
        {
            _stockService = stockService;
            _logger = logger;
        }

        // GET /api/stocks
        // Watchlist'teki tüm hisseleri döndür
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var stocks = await _stockService.GetAllStocksAsync();
                return Ok(stocks);
                // Ok() → HTTP 200 döner
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all stocks");
                return StatusCode(500, "An error occurred while fetching stocks.");
                // 500 → Sunucu hatası — raw exception değil, anlamlı mesaj dön
            }
        }

        // POST /api/stocks
        // Watchlist'e yeni hisse ekle
        [HttpPost]
        public async Task<IActionResult> AddStock([FromBody] CreateStockDto dto)
        {
            // Model validation — zorunlu alanlar boş mu?
            if (string.IsNullOrWhiteSpace(dto.Symbol))
                return BadRequest("Stock symbol is required.");
            // BadRequest() → HTTP 400 döner

            if (string.IsNullOrWhiteSpace(dto.CompanyName))
                return BadRequest("Company name is required.");

            try
            {
                var stock = await _stockService.AddStockAsync(dto);
                return CreatedAtAction(nameof(GetAll), new { }, stock);
                // CreatedAtAction() → HTTP 201 döner (kaynak oluşturuldu)
            }
            catch (InvalidOperationException ex)
            {
                // Aynı hisse zaten watchlist'te — bunu 409 Conflict olarak dön
                return Conflict(ex.Message);
                // Conflict() → HTTP 409 döner
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding stock {Symbol}", dto.Symbol);
                return StatusCode(500, "An error occurred while adding the stock.");
            }
        }

        // DELETE /api/stocks/{symbol}
        // Watchlist'ten hisse çıkar
        [HttpDelete("{symbol}")]
        public async Task<IActionResult> DeleteStock(string symbol)
        {
            try
            {
                var deleted = await _stockService.DeleteStockAsync(symbol);

                if (!deleted)
                    return NotFound($"Stock '{symbol}' not found in watchlist.");
                // NotFound() → HTTP 404 döner

                return NoContent();
                // NoContent() → HTTP 204 döner (başarılı, dönecek içerik yok)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting stock {Symbol}", symbol);
                return StatusCode(500, "An error occurred while deleting the stock.");
            }
        }

        // GET /api/stocks/{symbol}/price
        // Finnhub'dan anlık fiyat çek ve kaydet
        [HttpGet("{symbol}/price")]
        public async Task<IActionResult> GetPrice(string symbol)
        {
            try
            {
                var price = await _stockService.FetchAndSavePriceAsync(symbol);

                if (price == null)
                    return NotFound($"Could not fetch price for '{symbol}'. " +
                        $"Make sure the stock is in your watchlist and the symbol is valid.");

                return Ok(price);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching price for {Symbol}", symbol);
                return StatusCode(500, "An error occurred while fetching the price.");
            }
        }

        // GET /api/stocks/analytics/top?count=5
        // En çok büyüyen N hisseyi getir
        [HttpGet("analytics/top")]
        public async Task<IActionResult> GetTopGrowing([FromQuery] int count = 5)
        {
            // count parametresi mantıklı bir aralıkta mı?
            if (count < 1 || count > 50)
                return BadRequest("Count must be between 1 and 50.");

            try
            {
                var analytics = await _stockService.GetTopGrowingStocksAsync(count);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top growing stocks");
                return StatusCode(500, "An error occurred while calculating analytics.");
            }
        }
    }
}