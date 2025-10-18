# Multi tenant E-Commerce Microservices Platform
> **Status:** *Work in Progress*  

A modular, scalable, and event-driven e-commerce marketplace — built with modern backend design principles and clean architecture.
Each tenant (store) operates in isolation while sharing a common infrastructure.

---

## Architecture Highlights

| Feature | Description |
|----------|-------------|
| **Multitenancy** | Each tenant (store) has isolated data and context-aware middleware for request handling |
| **Microservices** | Independently deployable services (Auth, Customer, Product, Order, etc.) |
| **Event-Driven Architecture (EDA)** | Asynchronous inter-service communication using **RabbitMQ** |
| **MediatR** | Simplifies request handling and promotes clean separation between features |
| **FluentValidation** | Declarative and consistent input validation |
| **JWT Authentication** | Secure, stateless API authentication and authorization |
| **Vertical Slice Architecture** | Self-contained features with their own logic, validation, and handlers |
| **EF Core** | ORM for data persistence and migrations |
| **SQL Server** | Dedicated database per microservice for full data isolation |


## Tech Stack

- **.NET 8**
- **Entity Framework Core**
- **MediatR**
- **FluentValidation**
- **RabbitMQ**
- **ASP.NET Minimal APIs**
- **Docker**
- **SQL Server**

## Architecture

| Layer | Description |
|-------|--------------|
| **Shared/** | Common abstractions and base projects (DataAccess, Messaging, Contexts) |
| **Services/** | Independent microservices (Auth, Customers, etc.) |
