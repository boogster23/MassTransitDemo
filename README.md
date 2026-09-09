# MassTransit Demo

A sample .NET solution demonstrating messaging patterns, consumers, and workflows using [MassTransit](https://masstransit.io/).

---

## 📌 Features & Patterns

- **Publish/Subscribe:** Event-driven architecture with loosely coupled consumers.
- **Request/Client/Response:** Asynchronous request-reply messaging.
- **Fault Tolerance:** Retry policies, redelivery, and dead-letter queue (DLQ) handling.
- **Transactional Outbox:** Guaranteed at-least-once message delivery with EF Core (if enabled).
- **Sagas / State Machines:** Distributed workflow orchestration using Automatonymous / MassTransit State Machine.

---

## 🚀 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (or .NET 9+)
- [Docker Desktop](https://www.docker.com/) (recommended for local message brokers like RabbitMQ)

---

## 📂 Project Structure

```text
MassTransitDemo/
├── src/
│   └── MassTransitDemo.slnx      # Solution file (.slnx XML format)
│   # (Add your API, Worker, Contracts, and Core projects here)
├── .gitignore
└── README.md
```

---

## 🛠️ Getting Started

### 1. Build the Solution

```bash
dotnet build src/MassTransitDemo.slnx
```

### 2. (Optional) Run RabbitMQ via Docker

If using RabbitMQ as your message transport:

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:4-management
```

- **AMQP Port:** `5672`
- **Management UI:** `http://localhost:15672` (Username: `guest`, Password: `guest`)

---

## 📦 Useful dotnet CLI Commands

* **Add a new Worker / Consumer project:**
  ```bash
  dotnet new worker -n MassTransitDemo.Worker -o src/MassTransitDemo.Worker
  dotnet sln src/MassTransitDemo.slnx add src/MassTransitDemo.Worker
  ```

* **Add a Contracts / Messages class library:**
  ```bash
  dotnet new classlib -n MassTransitDemo.Contracts -o src/MassTransitDemo.Contracts
  dotnet sln src/MassTransitDemo.slnx add src/MassTransitDemo.Contracts
  ```

* **Install MassTransit packages:**
  ```bash
  # Core MassTransit
  dotnet add src/MassTransitDemo.Worker package MassTransit

  # If using RabbitMQ
  dotnet add src/MassTransitDemo.Worker package MassTransit.RabbitMQ
  ```

