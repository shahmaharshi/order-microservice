# Order Processing Microservice System

A production-grade microservices architecture built with **ASP.NET Core 8**, **Azure Service Bus**, **Docker**, **Clean Architecture**, and **CQRS** pattern.

## Architecture Overview

```
┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────────┐
│  Order Service  │────▶│  Azure Service Bus   │────▶│ Inventory Service   │
│  (REST API)     │     │                      │     │ (Stock Reservation) │
│  Port: 5001     │     │  Topics:             │     │  Port: 5002         │
└─────────────────┘     │  - order-created     │     └─────────────────────┘
        ▲               │  - stock-reserved    │              │
        │               └──────────────────────┘              │
        │                          │                          │
        └──────────────────────────┼──────────────────────────┘
                                   │
                                   ▼
                        ┌─────────────────────┐
                        │ Notification Service │
                        │ (Email on Confirm)   │
                        │  Port: 5003          │
                        └─────────────────────┘
```

### Event Flow

1. Client calls `POST /api/v1/orders` on **Order Service**
2. Order Service creates order (status: `Pending`) → publishes **OrderCreated** to Service Bus
3. **Inventory Service** receives OrderCreated → reserves stock → publishes **StockReserved** (success/failure)
4. Order Service receives StockReserved → updates order to `Confirmed` or `Failed`
5. **Notification Service** receives StockReserved (success) → sends confirmation email to customer

---

## Project Structure

```
OrderProcessing/
├── OrderService/
│   ├── src/
│   │   ├── OrderService.Domain/          # Entities, Enums, Domain Events
│   │   │   ├── Entities/
│   │   │   │   ├── Order.cs
│   │   │   │   └── OrderItem.cs
│   │   │   ├── Enums/
│   │   │   │   └── OrderStatus.cs
│   │   │   └── Events/
│   │   │       ├── OrderCreatedEvent.cs
│   │   │       └── StockReservedEvent.cs
│   │   ├── OrderService.Application/     # CQRS Commands, Queries, Interfaces
│   │   │   ├── Commands/
│   │   │   │   ├── PlaceOrderCommand.cs
│   │   │   │   ├── CancelOrderCommand.cs
│   │   │   │   └── StockReservedEventHandler.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetOrderByIdQuery.cs
│   │   │   │   └── GetOrdersByCustomerQuery.cs
│   │   │   ├── DTOs/
│   │   │   │   └── OrderDtos.cs
│   │   │   └── Interfaces/
│   │   │       ├── IOrderRepository.cs
│   │   │       └── IMessagePublisher.cs
│   │   ├── OrderService.Infrastructure/  # EF Core, Azure Service Bus, Repos
│   │   │   ├── Persistence/
│   │   │   │   └── OrderDbContext.cs
│   │   │   ├── Repositories/
│   │   │   │   └── OrderRepository.cs
│   │   │   └── Messaging/
│   │   │       ├── AzureServiceBusPublisher.cs
│   │   │       └── ServiceBusConsumer.cs
│   │   └── OrderService.API/             # Controllers, Program.cs
│   │       ├── Controllers/
│   │       │   └── OrdersController.cs
│   │       ├── Program.cs
│   │       └── appsettings.json
│   ├── tests/
│   │   └── OrderService.Tests/
│   │       ├── Commands/
│   │       │   └── PlaceOrderCommandHandlerTests.cs
│   │       └── Queries/
│   │           └── GetOrderByIdQueryHandlerTests.cs
│   └── Dockerfile
│
├── InventoryService/
│   ├── src/
│   │   ├── InventoryService.Domain/
│   │   │   └── Entities/InventoryItem.cs
│   │   ├── InventoryService.Application/
│   │   │   ├── Handlers/OrderCreatedEventHandler.cs
│   │   │   └── Interfaces/IInventoryRepository.cs
│   │   ├── InventoryService.Infrastructure/
│   │   │   ├── Persistence/InventoryDbContext.cs
│   │   │   ├── Repositories/InventoryRepository.cs
│   │   │   └── Messaging/InventoryMessaging.cs
│   │   └── InventoryService.API/Program.cs
│   └── Dockerfile
│
├── NotificationService/
│   ├── src/
│   │   ├── NotificationService.Application/
│   │   │   ├── Handlers/OrderConfirmedNotificationHandler.cs
│   │   │   └── Interfaces/IEmailSender.cs
│   │   ├── NotificationService.Infrastructure/
│   │   │   └── Email/SmtpEmailSender.cs
│   │   └── NotificationService.API/Program.cs
│   └── Dockerfile
│
├── docker-compose.yml
├── .env.example
└── README.md
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 |
| Pattern | Clean Architecture + CQRS (MediatR) |
| Messaging | Azure Service Bus (Topics + Subscriptions) |
| Database | SQL Server + Entity Framework Core 8 |
| Containers | Docker + Docker Compose |
| Auth | JWT Bearer Tokens |
| Testing | xUnit + Moq |
| API Docs | Swagger / OpenAPI |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Azure Service Bus namespace](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-create-namespace-portal) (or use the Azure free tier)

---

## Azure Service Bus Setup

You need to create the following in your Azure Service Bus namespace:

### Topics
| Topic Name | Description |
|---|---|
| `order-created` | Published by Order Service when order is placed |
| `stock-reserved` | Published by Inventory Service after stock check |

### Subscriptions
| Topic | Subscription Name | Subscriber |
|---|---|---|
| `order-created` | `inventory-service-sub` | Inventory Service |
| `order-created` | `notification-service-sub` | Notification Service |
| `stock-reserved` | `order-service-sub` | Order Service |

You can create these via Azure Portal, Azure CLI, or the Azure SDK. Example with Azure CLI:

```bash
az servicebus topic create --name order-created --namespace-name YOUR_NAMESPACE --resource-group YOUR_RG
az servicebus topic create --name stock-reserved --namespace-name YOUR_NAMESPACE --resource-group YOUR_RG

az servicebus topic subscription create --name inventory-service-sub --topic-name order-created --namespace-name YOUR_NAMESPACE --resource-group YOUR_RG
az servicebus topic subscription create --name notification-service-sub --topic-name order-created --namespace-name YOUR_NAMESPACE --resource-group YOUR_RG
az servicebus topic subscription create --name order-service-sub --topic-name stock-reserved --namespace-name YOUR_NAMESPACE --resource-group YOUR_RG
```

---

## Local Setup

### 1. Clone and configure environment

```bash
git clone https://github.com/maharshishah/order-microservice.git
cd order-microservice
cp .env.example .env
```

Edit `.env` with your Azure Service Bus connection string, JWT key, and SMTP settings.

### 2. Run with Docker Compose

```bash
docker-compose up --build
```

This starts:
- SQL Server on port `1433`
- Order Service on port `5001`
- Inventory Service on port `5002`
- Notification Service on port `5003`

EF Core migrations run automatically on startup.

### 3. Access Swagger UI

| Service | URL |
|---|---|
| Order Service | http://localhost:5001/swagger |
| Inventory Service | http://localhost:5002/swagger |
| Notification Service | http://localhost:5003/swagger |

---

## Running Without Docker (Development)

```bash
# Terminal 1 - Order Service
cd OrderService/src/OrderService.API
dotnet run

# Terminal 2 - Inventory Service
cd InventoryService/src/InventoryService.API
dotnet run

# Terminal 3 - Notification Service
cd NotificationService/src/NotificationService.API
dotnet run
```

Update `appsettings.json` in each service with your local connection strings before running.

---

## API Reference

### Order Service (Port 5001)

All endpoints require `Authorization: Bearer {token}` header.

#### Place an Order
```http
POST /api/v1/orders
Content-Type: application/json

{
  "customerId": "cust-001",
  "customerEmail": "customer@example.com",
  "items": [
    {
      "productId": "prod-1",
      "productName": "Widget A",
      "quantity": 2,
      "unitPrice": 9.99
    }
  ]
}
```

**Response (201 Created)**
```json
{
  "success": true,
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

#### Get Order by ID
```http
GET /api/v1/orders/{id}
```

**Response (200 OK)**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerId": "cust-001",
  "customerEmail": "customer@example.com",
  "status": "Confirmed",
  "totalAmount": 19.98,
  "createdAt": "2026-01-15T10:30:00Z",
  "updatedAt": "2026-01-15T10:30:05Z",
  "items": [
    {
      "productId": "prod-1",
      "productName": "Widget A",
      "quantity": 2,
      "unitPrice": 9.99
    }
  ]
}
```

#### Get Orders by Customer
```http
GET /api/v1/orders/customer/{customerId}
```

#### Cancel Order
```http
DELETE /api/v1/orders/{id}
```

### Order Status Values
| Status | Meaning |
|---|---|
| `Pending` | Order created, awaiting stock check |
| `StockConfirmed` | Stock reserved by Inventory Service |
| `Confirmed` | Fully confirmed, email sent |
| `Cancelled` | Cancelled by user |
| `Failed` | Stock reservation failed |

---

## Running Tests

```bash
cd OrderService
dotnet test tests/OrderService.Tests/OrderService.Tests.csproj --verbosity normal
```

### Test Coverage
- `PlaceOrderCommandHandlerTests` — valid order, empty items, empty customer ID, event publishing, total amount calculation
- `GetOrderByIdQueryHandlerTests` — existing order returns DTO, missing order returns null

---

## Key Design Decisions

### Why CQRS with MediatR?
Commands (write) and Queries (read) are separated — controllers stay thin (1-2 lines), all business logic lives in handlers. MediatR routes the request to the right handler without controllers needing to know the implementation. Easy to add cross-cutting concerns (logging, validation) as pipeline behaviors.

### Why Azure Service Bus (not direct HTTP calls)?
Services are fully decoupled. If Inventory Service goes down, the `order-created` message waits in the queue with its TTL. When the service restarts, it picks up the message and processes it — no orders are lost. Direct HTTP would fail immediately and require retry logic in every caller.

### Why Clean Architecture?
Dependency rule flows inward — Infrastructure knows about Application, Application knows about Domain, Domain knows nothing. Swapping SQL Server for another database means changing only the Infrastructure layer. Business logic in Domain and Application is completely framework-agnostic and fully unit-testable without spinning up a database.

### Concurrency (RowVersion)
The `Order` entity has a `RowVersion` (timestamp) column. EF Core uses optimistic concurrency — if two requests try to update the same order simultaneously, the second one throws `DbUpdateConcurrencyException`, which is caught and returned as a conflict response.

---

## Deployment to Azure

Each service can be deployed independently as an Azure App Service container:

```bash
# Build and push to Azure Container Registry
az acr login --name YOUR_REGISTRY
docker build -t YOUR_REGISTRY.azurecr.io/order-service:latest ./OrderService
docker push YOUR_REGISTRY.azurecr.io/order-service:latest

# Deploy to Azure App Service
az webapp create --name order-service-app --plan YOUR_PLAN --resource-group YOUR_RG \
  --deployment-container-image-name YOUR_REGISTRY.azurecr.io/order-service:latest
```

---

## License

MIT
