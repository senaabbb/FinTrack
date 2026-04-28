using FinTrack.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrenciesController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CurrenciesController> _logger;

        public CurrenciesController(ICurrencyService currencyService, ILogger<CurrenciesController> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        // GET /api/currencies
        // Kayıtlı tüm döviz kurlarını getir
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var rates = await _currencyService.GetAllRatesAsync();
                return Ok(rates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all currency rates");
                return StatusCode(500, "An error occurred while fetching currency rates.");
            }
        }

        // GET /api/currencies/rate?from=USD&to=TRY
        // Finnhub'dan döviz kuru çek ve kaydet
        [HttpGet("rate")]
        public async Task<IActionResult> GetRate(
            [FromQuery] string from = "USD",
            [FromQuery] string to = "TRY")
        {
            // Parametre kontrolü
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                return BadRequest("Both 'from' and 'to' currency codes are required.");

            if (from.ToUpper() == to.ToUpper())
                return BadRequest("'from' and 'to' currencies cannot be the same.");

            try
            {
                var rate = await _currencyService.FetchAndSaveRateAsync(from, to);

                if (rate == null)
                    return NotFound($"Could not fetch rate for {from.ToUpper()}/{to.ToUpper()}. " +
                        $"Please check the currency codes.");

                return Ok(rate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching rate for {From}/{To}", from, to);
                return StatusCode(500, "An error occurred while fetching the currency rate.");
            }
        }
    }
}
