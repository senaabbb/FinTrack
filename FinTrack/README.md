# 📈 FinTrack — Financial Data Tracker

A RESTful backend application built with **.NET 8** that tracks stock prices and currency exchange rates using the **Finnhub API**. Designed as a lightweight internal tool for financial data monitoring.

---

## 🎯 Project Purpose

FinTrack allows users to:
- Maintain a **stock watchlist** (add/remove stocks)
- **Fetch real-time prices** from Finnhub and persist them locally
- **Track currency exchange rates** (e.g. USD → TRY)
- View **analytical insights** such as top growing stocks by percentage

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 8 Web API |
| Database | SQLite via Entity Framework Core 8 |
| External API | Finnhub (https://finnhub.io) |
| API Docs | Swagger / OpenAPI |
| ORM | Entity Framework Core (Code First) |

---

## 📡 Why Finnhub?

Finnhub was chosen for the following reasons:
- **Generous free tier** — 60 requests per minute (no daily cap)
- Covers **stocks, forex, and crypto** in a single API
- Simple and well-documented REST API
- No credit card required for registration

---

## 🗄️ Why SQLite?

- **Zero configuration** — no server installation needed
- File-based database (`fintrack.db`) created automatically on startup
- Perfect for lightweight internal tools and assessments
- Easily swappable with SQL Server via EF Core configuration

---

## 🏗️ Project Structure

```
FinTrack/
├── Controllers/       → HTTP layer — routing and response only
├── Services/          → Business logic layer
├── Repositories/      → Data access layer
├── Interfaces/        → Abstractions for all layers
├── Models/            → Database entities (Stock, StockPrice, Currency)
├── DTOs/              → Request and response objects
├── Data/              → AppDbContext and database configuration
└── ExternalClients/   → Finnhub HTTP client
```

---

## 🎨 Design Patterns Used

### 1. Repository Pattern
**Where:** `Repositories/` layer — `StockRepository`, `StockPriceRepository`, `CurrencyRepository`

**Why:** Abstracts all database operations away from the service layer. Services interact only with interfaces (`IStockRepository`, etc.), not directly with `AppDbContext`. This means the database engine can be swapped without touching business logic.

```csharp
// StockRepository.cs
// Repository Pattern: Data access is fully isolated here.
// Service layer never sees DbContext directly.
public class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;
    ...
}
```

### 2. Strategy Pattern (Implicit)
**Where:** `StockService.GetTopGrowingStocksAsync()`

**Why:** The sorting and ranking logic is isolated within the service method. Different analytical strategies (e.g. top by volume, top by price drop) can be introduced without changing the controller or repository layers.

```csharp
// StockService.cs
// Strategy Pattern: Ranking logic is isolated here.
// Alternative strategies (TopByVolume, TopByPriceDrop) can be
// added and swapped without affecting other layers.
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
git clone https://github.com/senaabbb/FinTrack.git
cd FinTrack
```

### 2. Add Your Finnhub API Key
Open `appsettings.json` and replace the placeholder:
```json
"Finnhub": {
    "ApiKey": "your_real_api_key_here",
    "BaseUrl": "https://finnhub.io/api/v1"
}
```

### 3. Run the Application

**Visual Studio:**
Press **F5** or click the green Run button.

**Command Line:**
```bash
dotnet run
```

The SQLite database (`fintrack.db`) is created automatically on first run.

### 4. Open Swagger UI
Navigate to:
```
https://localhost:{7295}/swagger
```

---

## 📋 API Endpoints

### Stocks

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/stocks` | Get all stocks in watchlist |
| `POST` | `/api/stocks` | Add a stock to watchlist |
| `DELETE` | `/api/stocks/{symbol}` | Remove a stock from watchlist |
| `GET` | `/api/stocks/{symbol}/price` | Fetch and save live price from Finnhub |
| `GET` | `/api/stocks/analytics/top?count=5` | Get top N growing stocks |

### Currencies

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/currencies` | Get all saved currency rates |
| `GET` | `/api/currencies/rate?from=USD&to=TRY` | Fetch and save live exchange rate |

---

## 📬 Example Requests

### Add a Stock
```json
POST /api/stocks
{
  "symbol": "AAPL",
  "companyName": "Apple Inc.",
  "sector": "Technology"
}
```

### Fetch Live Price
```
GET /api/stocks/AAPL/price
```

### Get Top 3 Growing Stocks
```
GET /api/stocks/analytics/top?count=3
```

### Fetch USD to TRY Rate
```
GET /api/currencies/rate?from=USD&to=TRY
```

---

## ⚠️ Trade-offs & Known Limitations

- **No authentication** — this is an internal tool prototype, auth was out of scope
- **Price history is manual** — prices are only saved when the endpoint is called, no background scheduler
- **Analytics requires 2+ price records** — stocks with only one price fetch are excluded from growth calculations
- **Finnhub free tier** — limited to 60 requests/minute; no retry logic implemented

---

## 🔮 Potential Improvements

- Add a background service (Hosted Service) to auto-fetch prices on a schedule
- Implement JWT authentication
- Add Docker support
- Add unit tests with xUnit
- Replace implicit Strategy Pattern with a formal implementation

---

*Built as part of the Rasyonet Software Engineering Internship Technical Assessment.*