#  GameStore API

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
| POST | `/games` | Add a new game to the catalog | Seller |
| PUT | `/games/{id}` | Update game info | Seller |
| DELETE | `/games/{id}` | Delete a game (only if no active offers) | Seller |

### Offers
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/games/{gameId}/offers` | Get all offers for a game | No |
| POST | `/games/{gameId}/offers` | Create a new offer | Seller |
| PUT | `/games/{gameId}/offers/{offerId}` | Update price and stock | Seller |
| DELETE | `/games/{gameId}/offers/{offerId}` | Delete your offer | Seller |
| POST | `/games/{gameId}/offers/{offerId}/discount` | Apply a discount (1-90%) | Seller |
| DELETE | `/games/{gameId}/offers/{offerId}/discount` | Remove discount | Seller |

### Orders
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/orders` | Place a new order | Buyer |
| GET | `/orders` | Get your order history | Buyer |
| GET | `/orders/{id}` | Get order details | Buyer |
| POST | `/orders/{id}/cancel` | Cancel a paid order | Buyer |

### Genres & Platforms
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/genres` | List all genres | No |
| GET | `/platforms` | List all platforms | No |

### RAWG Integration
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/rawg/search?query=` | Search games on RAWG | Seller |
| GET | `/rawg/games/{rawgId}` | Get RAWG game details | Seller |
| POST | `/rawg/import` | Import a game from RAWG into the catalog | Seller |

### Health
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET, HEAD | `/health` | Health check (used by UptimeRobot) | No |

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

## Rate Limiting

| Policy | Limit | Window | Applied to |
|---|---|---|---|
| `global` | 30 requests | 10 seconds | `/games` endpoint group |
| `auth` | 5 requests | 1 minute | `/auth` endpoint group (`Login` / `Register`) |
| `write` | 10 requests | 1 minute | `/offers`, `/orders`, and `/rawg` endpoint groups |

---

> **Note:**The database may be reset periodically.
