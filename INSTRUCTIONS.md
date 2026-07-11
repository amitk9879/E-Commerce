# Build and Run Instructions

## Prerequisites

Ensure the following software is installed before running the application.

### Backend

- Visual Studio 2026
- .NET 9 SDK
- SQL Server LocalDB

### Frontend

- Visual Studio Code
- Node.js (v20 or later)
- Angular CLI

Install Angular CLI globally (if it is not already installed):

```bash
npm install -g @angular/cli
```

## Backend Setup

### 1. Open the Solution

Launch **Visual Studio 2026** and open:

```text
AirmasterCommerce.slnx
```

Allow Visual Studio to restore the NuGet packages automatically.

### 2. Run the Backend

Configure the following projects as **Multiple Startup Projects**:

- Gateway
- Identity.API
- Catalog.API
- Ordering.API
- Payment
- Shipping
- Notification.API

Press **F5** or click **Start** to launch the application.

> **Note:** Entity Framework Core automatically applies any pending migrations and creates the databases if they do not already exist.

## Frontend Setup

Open the Angular project in **Visual Studio Code**.

Install dependencies:

```bash
npm install
```

Run the application:

```bash
ng serve
```

The frontend will be available at:

```text
http://localhost:4200
```

## Technologies Used

### Backend

- ASP.NET Core 9
- Entity Framework Core
- SQL Server LocalDB
- RabbitMQ (Cloud Hosted)
- SignalR
- JWT Authentication
- Polly
- YARP API Gateway
- IMemoryCache

### Frontend

- Angular
- TypeScript
- Bootstrap
- RxJS

## Notes

- The application uses a **cloud-hosted RabbitMQ** instance that is already configured in the application settings. No local RabbitMQ installation or configuration is required.
- The Catalog service uses **ASP.NET Core IMemoryCache** to cache frequently accessed data during development. For production deployments, **Redis** is recommended as a distributed caching solution.
- SQL Server LocalDB is used for development and can be replaced with SQL Server or Azure SQL Database in a production environment.

## Test Credentials

| Role | Email | Password |
|------|-------|----------|
| User | hvac.engineer@daikin.com | password123 |
| User | contractor@delhi-hvac.in | password123 |
| Admin | admin.portal@daikin.com | password123 |
