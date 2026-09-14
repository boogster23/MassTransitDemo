# MassTransit & .NET Aspire Demo

A distributed .NET 10 messaging solution demonstrating event-driven pub/sub architecture using [MassTransit](https://masstransit.io/) and orchestrated locally with [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/).

---

## 🏗️ Architecture & Messaging Flow

```mermaid
flowchart LR
    Client["Client (Bruno / Hurl / HTTP)"] -->|"POST /api/orders/*"| API["ApiService<br/>(Rate Limiting & Debouncing)"]
    API -->|"publishEndpoint.Publish(SubmitOrder)"| RMQ_EX["RabbitMQ Exchange<br/>(MassTransitDemo.Contracts:SubmitOrder)"]
    RMQ_EX -->|"Route"| RMQ_Q["RabbitMQ Queue<br/>(submit-order)"]
    RMQ_Q -->|"Consume"| Worker["Worker Service<br/>(SubmitOrderConsumer)"]
    Worker -->|"context.Publish(OrderSubmitted)"| NextEvent["Next Event Subscribers"]
```

---

## 🚀 Key Features

- **Event-Driven Messaging**: MassTransit RabbitMQ pub/sub integration.
- **Fixed Window Rate Limiting**: Protects endpoints against traffic spikes using ASP.NET Core rate limiting (`5 requests / 10s` per client IP / user, zero queue for instant rejection with `429 Too Many Requests`).
- **Request Debouncer**: In-memory caching guard (`DebounceGuard`) that suppresses rapid duplicate submissions within a configurable interval (default `500ms`), preventing double-submits.
- **Observability & Health Checks**: OpenTelemetry metrics, Prometheus exporter, Grafana dashboards, and Aspire distributed tracing.

---

## 📡 API Endpoints

All order endpoints accept a JSON payload:
```json
{
  "customerNumber": "CUST-001",
  "amount": 199.99
}
```

| Method | Route | Description | Resilience Behavior |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/orders` | Standard order submission | Direct submission; returns `202 Accepted` with generated `OrderId`. |
| `POST` | `/api/orders/rate-limited` | Rate-limited order submission | Protected by `orders-fixed` policy (5 requests / 10s window). Returns `429 Too Many Requests` when exceeded. |
| `POST` | `/api/orders/debounced` | Debounced order submission | Guarded by `DebounceGuard`. Rapid duplicate submissions within the debounce interval (500ms) are suppressed (`202 Accepted` with suppression message). |

### Debounce Configuration

Configured in `appsettings.json`:
```json
"Debounce": {
  "DefaultInterval": "00:00:00.500"
}
```

---

## 📂 Project Structure

```text
MassTransitDemo/
├── src/
│   ├── MassTransitDemo.slnx            # Modern XML Solution format (.slnx)
│   ├── MassTransitDemo.AppHost/        # Aspire Orchestrator (RabbitMQ, Postgres, Grafana, Prometheus)
│   ├── MassTransitDemo.ServiceDefaults/# Shared OpenTelemetry, Prometheus exporter, health checks
│   ├── MassTransitDemo.Contracts/      # Shared message contracts (SubmitOrder, OrderSubmitted)
│   ├── MassTransitDemo.ApiService/     # Web API (Message Publisher / Minimal API)
│   │   ├── Configurations/             # Options classes (DebounceOptions)
│   │   ├── Endpoints/                  # Minimal API route groups (OrderEndpoints)
│   │   ├── Helpers/                    # DebounceGuard with IMemoryCache
│   │   └── Services/                   # OrderService publisher
│   └── MassTransitDemo.Worker/         # Background Worker (MassTransit Consumer)
├── bruno/                              # Bruno API test collection with assertions
├── tests/
│   └── hurl/                           # Hurl integration test scripts (debounced, rate-limited, normal)
├── .gitignore
└── README.md
```

---

## 🛠️ Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/) or [OrbStack](https://orbstack.dev/)
- [Aspire CLI](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling) (`aspire` command)
- [Bruno](https://www.usebruno.com/) or [Hurl](https://hurl.dev/) (for API testing)

---

## 🏃 Getting Started

### 1. Start the Solution via Aspire

Aspire spins up all containers (RabbitMQ, PostgreSQL, pgAdmin, Grafana, Prometheus) and service dependencies automatically:

```bash
aspire run
# or
dotnet run --project src/MassTransitDemo.AppHost
```

### 2. Open the Aspire Dashboard

When Aspire starts, click the dashboard URL printed in your terminal (e.g. `https://localhost:17...`).

In the dashboard, you'll find:
- **`apiservice`**: Assigned API endpoint.
- **`worker`**: Background consumer service.
- **`messaging`**: RabbitMQ instance with a direct link to the **RabbitMQ Management UI**.
- **`postgres` & `appdb`**: PostgreSQL database with a link to **pgAdmin**.
- **`grafana`**: Visualization container (`http://localhost:3000`).
- **`prometheus`**: Metrics scraper (`http://localhost:9090`).
- **Distributed Traces & Structured Logs**: Real-time OpenTelemetry tracking from the HTTP request to the consumer.

---

## 🧪 Testing

### Option 1: Bruno Collection

1. Open the [Bruno](https://www.usebruno.com/) desktop app.
2. Click **Open Collection** and select the `bruno/` directory.
3. Select the **Local** environment (`http://localhost:5142`).
4. Available requests:
   - **Submit Order** (`POST /api/orders`): Verifies successful order submission and response assertions.
   - **Rate Limited Order** (`POST /api/orders/rate-limited`): Tests submission under rate limiting.
   - **Debounced Order** (`POST /api/orders/debounced`): Tests rapid repeat suppression.
   - **Health / Alive** (`GET /alive`): Validates Aspire health checks.

### Option 2: Hurl Integration Tests

Automated CLI test scripts are located in `tests/hurl/`:

```bash
# Test normal submission
hurl tests/hurl/orders-normal.hurl

# Test rate limiting (verifies 5 permits succeed, 6th returns 429)
hurl tests/hurl/orders-rate-limited.hurl

# Test debouncing (verifies rapid duplicate suppression)
hurl tests/hurl/orders-debounced.hurl
```

---

## 📦 Key Packages Used

- **MassTransit** (`8.3.6`) & **MassTransit.RabbitMQ** (`8.3.6`) — Messaging framework.
- **Aspire.Hosting.AppHost** (`13.5.3`) — Distributed application host.
- **Aspire.Hosting.RabbitMQ** (`13.5.3`) — RabbitMQ container orchestration.
- **Aspire.Hosting.PostgreSQL** (`13.5.3`) — PostgreSQL container & pgAdmin.
- **OpenTelemetry.Exporter.Prometheus.AspNetCore** — Prometheus metrics scraping endpoint.
