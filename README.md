# 📈 FinTrack — Financial Data Tracker

A RESTful backend application built with **.NET 8** that tracks stock prices and currency exchange rates using the **Finnhub API**. Designed as a lightweight internal tool for financial data monitoring.

---

## 🎯 Project Purpose

FinTrack allows users to:
- Maintain a **stock watchlist** (add/remove stocks)
- **Fetch real-time prices** from Finnhub and persist them locally
- **Track currency exchange rates** (e.g. USD → TRY) via ExchangeRate-API
- View **analytical insights** such as top growing stocks by growth percentage

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 8 Web API |
| Database | SQLite via Entity Framework Core 8 |
| ORM Approach | Code First |
| Stock & Price API | Finnhub (https://finnhub.io) |
| Currency API | ExchangeRate-API (https://open.er-api.com) |
| API Documentation | Swagger / OpenAPI |
| Testing | xUnit + Moq + FluentAssertions |

---

## 📡 Why Finnhub?

- **Generous free tier** — 60 requests per minute
- Covers **stocks, forex, and crypto** in a single API
- Simple and well-documented REST API
- No credit card required for registration

> ⚠️ **Note:** Finnhub's `/forex/rates` endpoint requires a premium subscription.
> Currency rates are fetched via **ExchangeRate-API** (free, no registration needed).

---

## 🗄️ Why SQLite?

- **Zero configuration** — no server installation needed
- File-based database (`fintrack.db`) created automatically on startup
- Perfect for lightweight internal tools
- Easily swappable with SQL Server via a single EF Core config change

---

## 🏗️ Project Structure

```
FinTrack/
├── Controllers/       → HTTP layer — routing and response only
├── Services/          → Business logic layer
├── Repositories/      → Data access layer
├── Interfaces/        → Abstractions for all layers
├── Models/            → Database entities (Stock, StockPrice, Currency)
├── DTOs/              → Request/response objects + ErrorResponse
├── Data/              → AppDbContext and DB configuration
├── ExternalClients/   → Finnhub & ExchangeRate-API HTTP clients
└── Middleware/        → Global exception handler

FinTrack.Tests/
└── Services/          → Unit tests for StockService and CurrencyService
```

---

## 🎨 Design Patterns Used

### 1. Repository Pattern
**Where:** `Repositories/` — `StockRepository`, `StockPriceRepository`, `CurrencyRepository`

**Why:** Abstracts all database operations from the service layer. Services interact only with interfaces, not directly with `AppDbContext`. The database engine can be swapped without touching business logic.

```csharp
// Repository Pattern: Data access is fully isolated here.
// Service layer never sees DbContext directly.
public class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;
    ...
}
```

### 2. Strategy Pattern
**Where:** `StockService.GetTopGrowingStocksAsync()`

**Why:** Sorting and ranking logic is isolated within the service method. Alternative strategies (e.g. top by volume, top by price drop) can be added without changing the controller or repository layers.

```csharp
// Strategy Pattern: Ranking logic is isolated here.
// Alternative strategies can be swapped without affecting other layers.
return result
    .OrderByDescending(r => r.GrowthPercentage)
    .Take(count);
```

---

## 🚀 Setup & Run Instructions

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2019 or later](https://visualstudio.microsoft.com/)
- A free [Finnhub API key](https://finnhub.io)

### 1. Clone the Repository
```bash
git clone https://github.com/YOUR_USERNAME/FinTrack.git
cd FinTrack
```

### 2. Add Your Finnhub API Key
Open `FinTrack/appsettings.json` and replace the placeholder:
```json
"Finnhub": {
    "ApiKey": "your_real_api_key_here",
    "BaseUrl": "https://finnhub.io/api/v1"
}
```

### 3. Run the Application

**Visual Studio:**
Open `FinTrack.sln` → Press **F5**

**Command Line:**
```bash
cd FinTrack
dotnet run
```

The SQLite database (`fintrack.db`) is created automatically on first run.

### 4. Open Swagger UI
```
https://localhost:{PORT}/swagger
```
Port number can be found in `FinTrack/Properties/launchSettings.json`.

---

## 📋 API Endpoints

### Stocks

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/stocks/watchlist` | List all stocks in watchlist |
| `POST` | `/api/stocks/watchlist` | Add a stock to watchlist |
| `DELETE` | `/api/stocks/{symbol}/watchlist` | Remove a stock from watchlist |
| `GET` | `/api/stocks/{symbol}/live-price` | Fetch and save live price from Finnhub |
| `GET` | `/api/stocks/analytics/top-growing?count=5` | Get top N growing stocks |

### Currencies

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/currencies/history` | List all saved exchange rates |
| `GET` | `/api/currencies/live-rate?from=USD&to=TRY` | Fetch and save live exchange rate |

---

## 📬 Example Requests

### Add a Stock to Watchlist
```json
POST /api/stocks/watchlist
{
  "symbol": "AAPL",
  "companyName": "Apple Inc.",
  "sector": "Technology"
}
```

### Fetch Live Stock Price
```
GET /api/stocks/AAPL/live-price
```

### Get Top 3 Growing Stocks
```
GET /api/stocks/analytics/top-growing?count=3
```

### Fetch Live USD to TRY Rate
```
GET /api/currencies/live-rate?from=USD&to=TRY
```

---

## 🧪 Running the Tests

**Visual Studio:**
Test → Run All Tests

**Command Line:**
```bash
cd FinTrack.Tests
dotnet test
```

### Test Coverage

| Test | What It Validates |
|---|---|
| `AddStockAsync_ShouldThrowException_WhenStockAlreadyExists` | Duplicate stock protection |
| `AddStockAsync_ShouldReturnStockDto_WhenStockIsNew` | Successful stock creation |
| `GetTopGrowingStocksAsync_ShouldReturnEmpty_WhenNoPriceHistory` | Analytics edge case |
| `GetTopGrowingStocksAsync_ShouldCalculateGrowthCorrectly` | Growth percentage calculation |
| `FetchAndSavePriceAsync_ShouldReturnNull_WhenStockNotInWatchlist` | Missing stock guard |
| `FetchAndSaveRateAsync_ShouldReturnNull_WhenCurrencyNotSupported` | Unsupported currency guard |
| `FetchAndSaveRateAsync_ShouldReturnCurrencyDto_WhenRateExists` | Successful rate fetch and save |

---

## ⚠️ Trade-offs & Known Limitations

- **No authentication** — internal tool prototype, auth was out of scope
- **Manual price fetching** — prices saved only when endpoint is called, no background scheduler
- **Analytics requires 2+ price records** — stocks with one fetch are excluded from growth calculations
- **Finnhub free tier** — 60 req/min limit, no retry logic implemented
- **ExchangeRate-API** — used instead of Finnhub forex (Finnhub forex requires premium)

---

## 🔮 Potential Improvements

- Background service (Hosted Service) to auto-fetch prices on a schedule
- JWT authentication
- Docker support
- Pagination for large watchlists
- Formal Strategy Pattern implementation for analytics

---

*Built as part of the Rasyonet Software Engineering Internship Technical Assessment.*