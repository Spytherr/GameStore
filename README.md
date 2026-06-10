# 🎮 GameStore API

[![Live Demo on Azure](https://img.shields.io/badge/Live_Demo-Azure-0078D4?style=for-the-badge&logo=microsoft-azure)](https://gamestore-api-spytherr-f3b8hbf2gwcafqbb.swedencentral-01.azurewebsites.net/scalar/v1)

A REST API for a game marketplace where users can browse games, sell their copies, and buy from other players.

Built with **.NET 10 Minimal API**, deployed on **Azure** as a Docker container.

## What does it do?

- **Browse games** — search, filter by genre/platform/sale status, sort by price/rating/release date, with pagination
- **Sell games** — sellers can create offers with price and stock for specific platforms
- **Discounts** — sellers can apply and remove percentage-based discounts on their offers
- **Buy games** — buyers can place orders with automatic stock management and payment processing
- **Cancel orders** — buyers can cancel paid orders (stock is restored automatically)
- **User accounts** — register and log in with JWT authentication
- **Two roles** — Buyer and Seller, each with different permissions
- **Real game data** — games are seeded from the [RAWG API](https://rawg.io/apidocs) and can also be imported manually
- **API docs** — interactive documentation with Scalar UI

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 10 Minimal API |
| Database | SQL Server (Azure SQL) |
| ORM | Entity Framework Core 10 |
| Auth | ASP.NET Identity + JWT Bearer |
| Validation | FluentValidation |
| API Docs | Scalar (OpenAPI) |
| Testing | xUnit, NSubstitute, WebApplicationFactory |
| Deployment | Docker → Azure Container Apps |

## Project Structure

```
GameStore/
├── GameStore.api/
│   ├── Data/                   # Database context, migrations, seeding
│   ├── Dtos/                   # Request/response records
│   │   └── Rawg/               # RAWG API specific DTOs
│   ├── Endpoints/              # API route definitions
│   ├── Middleware/              # Validation filter
│   ├── Models/                 # Entity models
│   ├── Services/               # Business logic (interfaces + implementations)
│   ├── Validators/             # FluentValidation rules
│   ├── Dockerfile
│   └── Program.cs
│
├── GameStore.api.Tests/
│   ├── GameOffersServiceTests  # Discount validation, authorization
│   ├── GenresServiceTests      # CRUD with ServiceResult pattern
│   ├── OrdersServiceTests      # Orders, stock, payments, transactions
│   ├── GenresEndpointsTests    # Integration: HTTP pipeline
│   ├── OrdersEndpointsTests    # Integration: auth + endpoints
│   └── TestAuthHandler         # Fake auth for testing
│
└── GameStore.slnx
```

## API Endpoints

### Auth
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/auth/register` | Create a new account (Buyer or Seller) | No |
| POST | `/auth/login` | Log in and get a JWT token | No |

### Games
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/games` | List games (filter, sort, paginate) | No |
| GET | `/games/{id}` | Get game details with offers | No |
| POST | `/games` | Add a new game to the catalog | No |
| PUT | `/games/{id}` | Update game info | No |
| DELETE | `/games/{id}` | Delete a game (only if no active offers) | No |

### Offers
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/games/{gameId}/offers` | Get all offers for a game | No |
| POST | `/games/{gameId}/offers` | Create a new offer | Seller |
| PUT | `/games/{gameId}/offers/{id}` | Update price and stock | Seller |
| DELETE | `/games/{gameId}/offers/{id}` | Delete your offer | Seller |
| POST | `/games/{gameId}/offers/{id}/discount` | Apply a discount (1-90%) | Seller |
| DELETE | `/games/{gameId}/offers/{id}/discount` | Remove discount | Seller |

### Orders
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/orders` | Place a new order | Buyer |
| GET | `/orders` | Get your order history | Buyer |
| GET | `/orders/{id}` | Get order details | Buyer |
| POST | `/orders/{id}/cancel` | Cancel a paid order | Buyer |

### Other
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/genres` | List all genres | No |
| GET | `/genres/{id}` | Get genre by ID | No |
| GET | `/platforms` | List all platforms | No |
| GET | `/health` | Health check endpoint for UptimeRobot | No |
| GET | `/rawg/search?query=` | Search games on RAWG | No |
| GET | `/rawg/{id}` | Get RAWG game details | No |
| POST | `/rawg/import` | Import a game from RAWG into the catalog | No |

## Key Design Decisions

### ServiceResult Pattern
Instead of throwing exceptions for expected errors, all services return a `ServiceResult<T>` object. This provides typed error handling with categories like `NotFound`, `ValidationError`, `Conflict`, and `Forbidden`. Endpoints then map these results to proper HTTP status codes.

### Database Transactions
Order creation uses `BeginTransactionAsync()` with `CommitAsync()`/`RollbackAsync()`. If payment fails after stock has been deducted, the transaction is rolled back to keep data consistent.

### Stock Management
When a buyer places an order, stock is automatically deducted from the offer. When an order is cancelled, stock is restored. Orders are rejected if there isn't enough stock.

### Discount System
Sellers can apply percentage-based discounts (1-90%) to their offers. The discounted price is calculated as `Price * (1 - DiscountPercentage / 100)` and rounded to 2 decimal places. Games with active discounts can be filtered using the `onSale` query parameter.

### Payment Processing
The payment service is abstracted behind `IPaymentService`. The current implementation is a mock that always succeeds, but the architecture makes it easy to swap in a real provider (e.g. Stripe). The service returns a `PaymentResult` with a transaction ID.

### Cold Start Optimization
To prevent the Azure Free/Basic tier from putting the container to sleep after periods of inactivity, the API includes a lightweight `/health` endpoint. This is paired with an external monitoring service (like UptimeRobot) that pings the endpoint every 10-15 minutes. This eliminates the 15-30 second cold start delay that users would otherwise experience when loading the API documentation or making their first request.

## How to Run Locally

### Requirements
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or LocalDB)
- RAWG API key (optional) — get one at [rawg.io](https://rawg.io/apidocs)

### Steps

1. **Clone the repo**
   ```bash
   git clone https://github.com/Spytherr/GameStore.git
   cd GameStore
   ```

2. **Set up secrets**
   ```bash
   cd GameStore.api
   dotnet user-secrets set "ConnectionStrings:GameStoreContext" "Server=(localdb)\mssqllocaldb;Database=GameStore;Trusted_Connection=True;TrustServerCertificate=True;"
   dotnet user-secrets set "Jwt:Key" "YourSuperSecretKeyThatIsAtLeast32Characters!"
   dotnet user-secrets set "Rawg:ApiKey" "your-rawg-api-key"
   ```

3. **Run the API**
   ```bash
   dotnet run
   ```

4. **Open the docs**

   Go to `http://localhost:5000/scalar/v1` for the interactive API documentation.

## How to Run Tests

```bash
dotnet test
```

All **16 tests** should pass. The test suite includes:
- **Unit tests** — service logic with in-memory database and mocked payment service (NSubstitute)
- **Integration tests** — full HTTP pipeline with `WebApplicationFactory` and custom auth handler
- **Parameterized tests** — boundary value testing for discount validation

## Rate Limiting

| Policy | Limit | Window | Used for |
|---|---|---|---|
| `global` | 30 requests | 10 seconds | Read endpoints |
| `auth` | 5 requests | 1 minute | Login / Register |
| `write` | 10 requests | 1 minute | Create / Update / Delete |
