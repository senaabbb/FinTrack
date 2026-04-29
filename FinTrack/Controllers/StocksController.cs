using FinTrack.DTOs;
using FinTrack.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Controllers
{
    [ApiController]
    [Route("api/stocks")]
    public class StocksController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly ILogger<StocksController> _logger;

        public StocksController(IStockService stockService, ILogger<StocksController> logger)
        {
            _stockService = stockService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all stocks currently in the watchlist.
        /// </summary>
        // GET /api/stocks/watchlist
        [HttpGet("watchlist")]
        public async Task<IActionResult> RetrieveWatchlist()
        {
            var stocks = await _stockService.GetAllStocksAsync();
            return Ok(stocks);
        }

        /// <summary>
        /// Adds a new stock to the watchlist by symbol.
        /// Returns 409 if the stock already exists.
        /// </summary>
        // POST /api/stocks/watchlist
        [HttpPost("watchlist")]
        public async Task<IActionResult> RegisterStockToWatchlist([FromBody] CreateStockDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Symbol))
                return BadRequest(new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Stock symbol is required.",
                    Path = Request.Path
                });

            if (dto.Symbol.Length > 10)
                return BadRequest(new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Stock symbol cannot exceed 10 characters.",
                    Path = Request.Path
                });

            if (string.IsNullOrWhiteSpace(dto.CompanyName))
                return BadRequest(new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Company name is required.",
                    Path = Request.Path
                });

            var stock = await _stockService.AddStockAsync(dto);

            return CreatedAtAction(nameof(RetrieveWatchlist), stock);
        }

        /// <summary>
        /// Removes a stock from the watchlist by its symbol.
        /// Returns 404 if the symbol does not exist in the watchlist.
        /// </summary>
        // DELETE /api/stocks/{symbol}/watchlist
        [HttpDelete("{symbol}/watchlist")]
        public async Task<IActionResult> RemoveStockFromWatchlist(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return BadRequest(new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Stock symbol is required.",
                    Path = Request.Path
                });

            var deleted = await _stockService.DeleteStockAsync(symbol);

            if (!deleted)
                return NotFound(new ErrorResponse
                {
                    StatusCode = 404,
                    Message = $"Stock '{symbol.ToUpper()}' was not found in your watchlist.",
                    Path = Request.Path
                });

            return NoContent();
        }

        /// <summary>
        /// Fetches the live market price for a stock from Finnhub
        /// and persists it to the database.
        /// </summary>
        // GET /api/stocks/{symbol}/live-price
        [HttpGet("{symbol}/live-price")]
        public async Task<IActionResult> FetchAndPersistLivePrice(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return BadRequest(new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Stock symbol is required.",
                    Path = Request.Path
                });

            var price = await _stockService.FetchAndSavePriceAsync(symbol);

            if (price == null)
                return NotFound(new ErrorResponse
                {
                    StatusCode = 404,
                    Message = $"Could not fetch live price for '{symbol.ToUpper()}'.",
                    Detail = "Ensure the stock is in your watchlist and the symbol is valid.",
                    Path = Request.Path
                });

            return Ok(price);
        }

        /// <summary>
        /// Returns the top N stocks by growth percentage
        /// based on persisted price history.
        /// Requires at least 2 price records per stock.
        /// </summary>
        // GET /api/stocks/analytics/top-growing?count=5
        [HttpGet("analytics/top-growing")]
        public async Task<IActionResult> RetrieveTopGrowingStocks([FromQuery] int count = 5)
        {
            if (count < 1 || count > 50)
                return BadRequest(new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Count must be between 1 and 50.",
                    Path = Request.Path
                });

            var analytics = await _stockService.GetTopGrowingStocksAsync(count);

            return Ok(analytics);
        }
    }
}