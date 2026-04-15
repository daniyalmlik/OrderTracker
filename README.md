# OrderPulse

A portfolio project demonstrating event-driven architecture with Apache Kafka, real-time updates via SignalR, and cookie-based JWT authentication — built with ASP.NET Core 8 and React.

## Architecture

```
React (port 5173)
    │
    ▼  HTTP / SignalR
ASP.NET Core 8 API (port 5098)
    │
    ├──► SQL Server (local)
    │
    └──► Kafka topic: "order-events"
              │
              ▼
    KafkaConsumerService (BackgroundService)
              │
              ▼
    SignalR Hub ──► React (live order updates)
```

Single API project — no microservices. Kafka runs in Docker (KRaft mode, no Zookeeper). SQL Server runs locally.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18, Vite, TypeScript, Tailwind CSS |
| Backend | ASP.NET Core 8, Entity Framework Core |
| Messaging | Apache Kafka 3.7 (KRaft mode) |
| Real-time | SignalR |
| Database | SQL Server |
| Auth | JWT (in-memory) + HTTP-only refresh cookies |
| Testing | xUnit, Moq, WebApplicationFactory |

## Features

- **Order management** — create, update status, cancel orders
- **Live updates** — Kafka consumer pushes status changes to connected clients via SignalR, no polling
- **Event log** — every order transition stored with an idempotency key; duplicate Kafka messages are safely skipped
- **JWT auth** — access tokens kept in React memory only; refresh tokens in HTTP-only cookies with rotation
- **Theme preference** — cookie-based dark/light mode toggle (demonstrates non-HttpOnly cookie contrast with auth cookies)

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 18+
- Docker & Docker Compose
- SQL Server (local instance)

### 1. Start Kafka

```bash
docker compose up -d
```

This starts a single-broker Kafka cluster in KRaft mode and creates the `order-events` topic (3 partitions).

### 2. Configure the database

Create `src/OrderTracker.Api/appsettings.Development.json` (gitignored):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OrderPulse;User Id=sa;Password=<your-password>;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "<at-least-32-char-secret>",
    "Issuer": "OrderPulse",
    "Audience": "OrderPulse"
  }
}
```

Apply migrations:

```bash
dotnet ef database update --project src/OrderTracker.Api
```

### 3. Run the API

```bash
cd src/OrderTracker.Api
dotnet run
# API available at http://localhost:5098
```

### 4. Run the frontend

```bash
cd src/orderpulse-client
npm install
npm run dev
# App available at http://localhost:5173
```

## API Reference

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/auth/register` | — | Create account |
| POST | `/api/auth/login` | — | Returns JWT + sets refresh cookie |
| POST | `/api/auth/refresh` | cookie | Rotate refresh token |
| POST | `/api/auth/logout` | JWT | Clear cookie + invalidate token |
| GET | `/api/orders` | JWT | List your orders |
| POST | `/api/orders` | JWT | Create order → publishes `OrderPlaced` |
| PATCH | `/api/orders/{id}/status` | JWT | Update status → publishes `StatusChanged` |
| DELETE | `/api/orders/{id}` | JWT | Cancel → publishes `OrderCancelled` |
| GET | `/api/orders/{id}/events` | JWT | Full event history for an order |
| GET | `/api/preferences/theme` | — | Read theme cookie |
| POST | `/api/preferences/theme` | — | Set theme cookie |

## Key Design Decisions

### Kafka
- Partition key is `OrderId` — guarantees all events for one order land on the same partition, preserving ordering
- Manual offset commits (`EnableAutoCommit = false`) — committed only after DB write + SignalR push
- Idempotency key format: `order-{id}-{eventType}-{newStatus}` — checked against `OrderEvents` table before processing; duplicates are skipped, giving effectively exactly-once semantics

### Authentication
- Access tokens (JWT, 15 min) returned in response body and stored in React state only — never `localStorage`
- Refresh tokens stored in HTTP-only, `SameSite=Lax` cookies scoped to `/api/auth`
- On 401, the Axios interceptor silently attempts a refresh and retries the original request

### SignalR
- Hub at `/hubs/orders` requires `[Authorize]`
- On connect, client joins group `user-{userId}`; Kafka consumer pushes updates to that group

## Running Tests

```bash
dotnet test
```

Unit and integration tests cover the Orders and Preferences features using `WebApplicationFactory` with an in-memory SQL Server substitute.

## Project Structure

```
OrderTracker/
├── src/
│   ├── OrderTracker.Api/
│   │   ├── Features/
│   │   │   ├── Auth/
│   │   │   ├── Orders/
│   │   │   └── Preferences/
│   │   ├── Hubs/
│   │   ├── Infrastructure/
│   │   │   ├── Kafka/
│   │   │   └── Persistence/
│   │   └── Program.cs
│   └── orderpulse-client/
│       └── src/
│           ├── api/
│           ├── components/
│           ├── contexts/
│           ├── hooks/
│           └── pages/
├── tests/
│   └── OrderTracker.Api.Tests/
├── docker-compose.yml
└── OrderTracker.sln
```
