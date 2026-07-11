# Airmaster E-Commerce Platform - Design Document

## Overview

The **Airmaster E-Commerce Platform** is a cloud-ready, microservices-based e-commerce solution designed to safely handle **10+ million daily orders** and **100,000+ concurrent users**. The platform follows modern distributed system principles including domain-driven design, asynchronous messaging, resilience patterns, and independent service scalability.

---

# 1. System Architecture

The platform uses a **decentralized Microservices Architecture**.

### Core Components

- **API Gateway (YARP)**
  - Single entry point for all client applications.
  - Handles routing, rate limiting, and CORS.

- **Microservices**
  - Identity
  - Catalog
  - Ordering
  - Payment
  - Shipping
  - Notification

- **RabbitMQ Event Bus**
  - Enables asynchronous communication.
  - Guarantees eventual consistency.
  - Decouples services.

- **SQL Server**
  - Database-per-service pattern.
  - Managed using Entity Framework Core Code-First Migrations.

---

## Architecture Flow

```text
                        +----------------------+
                        |    Client (Web/App)  |
                        +----------+-----------+
                                   |
                                   |
                           HTTPS / JWT
                                   |
                     +-------------v-------------+
                     |      API Gateway (YARP)   |
                     |---------------------------|
                     | Routing                   |
                     | Rate Limiting             |
                     | Authentication            |
                     | CORS                      |
                     +-------------+-------------+
                                   |
         ---------------------------------------------------------
         |          |            |            |                  |
         |          |            |            |                  |
+--------v--+ +-----v-----+ +----v-----+ +----v-----+ +----------v---------+
| Identity  | | Catalog   | | Ordering | | Shipping | | Notification       |
|    API    | |    API    | |    API   | |    API   | | API (SignalR)      |
+-----------+ +-----------+ +-----+----+ +----------+ +--------------------+
                                   |
                            Publish Event
                                   |
                                   v
                         +------------------+
                         |     RabbitMQ     |
                         +------------------+
                                   |
             -----------------------------------------------
             |                                             |
             |                                             |
     +-------v--------+                          +----------v---------+
     | Payment Worker |                          | Shipping Worker    |
     +----------------+                          +--------------------+
             |
      External Payment
        Gateway (Stripe/
          PayPal etc.)
```

---

# 2. Database Schema

The platform follows the **Database-per-Service Pattern**, ensuring loose coupling and independent scalability.

---

## Catalog Database (`Airmaster_CatalogDb`)

### Product

| Column | Type |
|---------|------|
| Id | GUID |
| Name | String |
| Description | String |
| Price | Decimal |
| StockQuantity | Integer |
| CategoryId | GUID (FK) |

### Category

| Column | Type |
|---------|------|
| Id | GUID |
| Name | String |

---

## Ordering Database (`Airmaster_OrderingDb`)

### Order

| Column | Type |
|---------|------|
| Id | GUID |
| UserId | GUID |
| OrderDateUtc | DateTime |
| Status | Enum (Pending, Paid, Shipped) |
| TotalAmount | Decimal |

### OrderItem

| Column | Type |
|---------|------|
| Id | GUID |
| OrderId | GUID (FK) |
| ProductId | GUID |
| UnitPrice | Decimal |
| Quantity | Integer |

---

## Payment Database (`Airmaster_PaymentDb`)

### PaymentTransaction

| Column | Type |
|---------|------|
| Id | GUID |
| OrderId | GUID |
| UserId | GUID |
| Amount | Decimal |
| TransactionStatus | String |
| GatewayReference | String |
| ProcessedAtUtc | DateTime |

---

## Shipping Database (`Airmaster_ShippingDb`)

### Shipment

| Column | Type |
|---------|------|
| Id | GUID |
| OrderId | GUID |
| Carrier | String |
| TrackingNumber | String |
| DispatchedAtUtc | DateTime |

---

## Identity Database (`Airmaster_IdentityDb`)

### User

| Column | Type |
|---------|------|
| Id | GUID |
| Email | String |
| PasswordHash | String |
| FirstName | String |
| LastName | String |
| Role | String |

---

# 3. Microservice Boundaries

Each bounded context owns its own business logic and database.

| Service | Responsibility |
|----------|----------------|
| **Gateway (YARP)** | Reverse proxy, routing, CORS, authentication, and rate limiting |
| **Identity.API** | User authentication, JWT generation, authorization, and profile management |
| **Catalog.API** | Product catalog, categories, pricing, and stock availability |
| **Ordering.API** | Shopping cart checkout, order creation, and order lifecycle |
| **Payment.API** | Background payment processing with resilient HTTP clients (Polly) |
| **Shipping.API** | Shipment creation, carrier assignment, and tracking generation |
| **Notification.API** | Real-time SignalR/WebSocket notifications for order status updates |

---

# 4. Scaling Strategy

The platform is designed for high throughput and low latency.

## 1. Asynchronous Processing

Instead of processing payments during the HTTP request:

1. User submits an order.
2. Ordering API saves the order.
3. Ordering API publishes an event to RabbitMQ.
4. HTTP request immediately returns **202 Accepted**.
5. Payment processing happens asynchronously.
6. Shipping begins after payment succeeds.

This keeps web servers responsive under heavy traffic.

---

## 2. Dead-Letter Queues (DLQ)

Failed messages are automatically routed to a Dead-Letter Exchange.

```text
Order Created
      |
      v
 RabbitMQ Queue
      |
      v
 Payment Consumer
      |
   Failure
      |
      v
Dead Letter Exchange
(AirmasterCentralExchange.DLX)
```

Benefits:

- Zero message loss
- Manual replay capability
- Easier troubleshooting
- Improved reliability

---

## 3. Cloud Deployment Strategy (Azure)

Future production deployment includes:

- Azure Container Apps
- Azure SQL Database Serverless
- Azure Container Registry
- Azure Key Vault
- Azure Monitor
- Application Insights

### Auto Scaling

Services scale horizontally based on:

- CPU usage
- Memory usage
- RabbitMQ queue depth (KEDA)
- Concurrent requests

---

## 4. Distributed Caching

To reduce database load:

```
Client
   |
IMemoryCache
   |
Catalog API
   |
SQL Server
```

Benefits:

- Sub-millisecond product retrieval
- Reduced SQL load
- Higher throughput
- Lower response latency

---

# 5. Security Considerations

The platform implements multiple layers of security aligned with modern best practices.

---

## Authentication & Authorization

- JSON Web Tokens (JWT)
- HMAC-SHA256 signatures
- SymmetricSecurityKey
- Token expiration: **15 minutes**
- Role-based authorization

---

## API Gateway Rate Limiting

### Token Bucket Limiter

- 20 tokens per IP
- Suitable for read-heavy endpoints

### Fixed Window Limiter

- 100 requests per minute
- Used for authentication and sensitive endpoints

Benefits:

- DDoS protection
- API abuse prevention
- Fair resource allocation

---

## Circuit Breakers (Polly)

Payment gateway integrations use:

```csharp
.AddStandardResilienceHandler()
```

Features include:

- Retry policies
- Circuit breakers
- Timeouts
- Resilience pipelines

If Stripe or PayPal experiences an outage, the circuit opens, preventing excessive retries and protecting internal resources.

---

## SQL Injection Prevention

All database access is performed through:

- Entity Framework Core
- LINQ
- Parameterized SQL queries

This significantly reduces SQL injection risks.

---

# Technology Stack

| Layer | Technology |
|--------|------------|
| Backend | ASP.NET Core 9 |
| API Gateway | YARP |
| ORM | Entity Framework Core |
| Database | Microsoft SQL Server |
| Messaging | RabbitMQ |
| Authentication | JWT |
| Resilience | Polly |
| Real-Time Notifications | SignalR |
| Containerization | Docker |
| Cloud Platform | Microsoft Azure |
| Caching (Future) | Redis |

---

# End-to-End Order Flow

```text
Client
   |
   v
API Gateway
   |
   v
Ordering API
   |
   | Save Order
   |
   +--------------------+
                        |
                        v
                   RabbitMQ
                        |
                OrderCreated Event
                        |
                        v
                 Payment Service
                        |
           Payment Successful
                        |
                        v
                PaymentCompleted
                        |
                        v
                 Shipping Service
                        |
                Shipment Created
                        |
                        v
            Notification Service
                        |
                        v
        SignalR Push Notification
                        |
                        v
                     Client
```

---

# Key Design Principles

- Microservices Architecture
- Domain-Driven Design (DDD)
- Database-per-Service Pattern
- Event-Driven Communication
- Eventual Consistency
- Asynchronous Processing
- Horizontal Scalability
- High Availability
- Fault Tolerance
- Resilience with Polly
- Secure API Gateway
- Cloud-Native Design
- Independent Service Deployment

---

# Future Enhancements

- Redis Distributed Cache
- Elasticsearch Product Search
- Azure Service Bus Migration
- Kubernetes (AKS) Deployment
- Distributed Tracing with OpenTelemetry
- Saga Pattern for Long-Running Transactions
- CQRS + MediatR
- Event Sourcing
- API Versioning
- Multi-region Azure Deployment
- CDN Integration
- AI-based Product Recommendations

---

# Conclusion

The Airmaster E-Commerce Platform is designed as a scalable, resilient, and cloud-ready distributed system capable of handling enterprise-scale workloads. By combining microservices, asynchronous messaging, resilient communication patterns, independent databases, and modern cloud-native practices, the platform provides a strong architectural foundation for supporting millions of daily transactions while maintaining high availability, performance, and security.
