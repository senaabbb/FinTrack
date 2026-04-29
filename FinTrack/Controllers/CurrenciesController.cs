using FinTrack.DTOs;
using FinTrack.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Controllers
{
    [ApiController]
    [Route("api/currencies")]
    public class CurrenciesController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CurrenciesController> _logger;

        public CurrenciesController(
            ICurrencyService currencyService,
            ILogger<CurrenciesController> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        /// <summary>
        /// Lists all previously fetched currency exchange rates
        /// stored in the local database.
        /// </summary>
        // GET /api/currencies/history
        [HttpGet("history")]
        public async Task<IActionResult> ListSavedExchangeRates()
        {
            var rates = await _currencyService.GetAllRatesAsync();
            return Ok(rates);
        }

        /// <summary>
        /// Fetches a live exchange rate from ExchangeRate-API
        /// for the given currency pair and persists it to the database.
        /// Example: GET /api/currencies/live-rate?from=USD&to=TRY
        /// </summary>
        // GET /api/currencies/live-rate?from=USD&to=TRY
        [HttpGet("live-rate")]
        public async Task<IActionResult> FetchAndPersistLiveExchangeRate(
            [FromQuery] string from = "USD",
            [FromQuery] string to = "TRY")
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                return BadRequest(new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Both 'from' and 'to' currency codes are required.",
                    Path = Request.Path
                });

            if (from.ToUpper() == to.ToUpper())
                return BadRequest(new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Source and target currencies cannot be the same.",
                    Detail = $"Received: from={from.ToUpper()}, to={to.ToUpper()}",
                    Path = Request.Path
                });

            if (from.Length != 3 || to.Length != 3)
                return BadRequest(new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Currency codes must be exactly 3 characters.",
                    Detail = "Use standard ISO 4217 codes such as USD, TRY, EUR, GBP.",
                    Path = Request.Path
                });

            var rate = await _currencyService.FetchAndSaveRateAsync(from, to);

            if (rate == null)
                return NotFound(new ErrorResponse
                {
                    StatusCode = 404,
                    Message = $"Could not fetch live rate for {from.ToUpper()}/{to.ToUpper()}.",
                    Detail = "Please verify the currency codes are valid ISO 4217 codes.",
                    Path = Request.Path
                });

            return Ok(rate);
        }
    }
}
