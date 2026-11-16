# PetStore API

REST API for an online pet store, built with **ASP.NET Core 8**, **Entity Framework Core** and **SQL Server**. It covers the full shop flow: catalog, cart, checkout with Stripe, order tracking, reviews and an admin back office with reports and bulk import from Excel.

## Tech stack

- ASP.NET Core 8 Web API, C#
- Entity Framework Core 8 (SQL Server, code-first migrations)
- JWT authentication with role-based authorization (`Administrador`, `Gerente`, customer)
- Stripe.net (checkout + signed webhooks)
- AutoMapper (entity ↔ DTO mapping)
- EPPlus / ExcelDataReader (Excel import and templates)
- Swagger / OpenAPI with Bearer auth

## Features

**Customers**
- Registration with email verification, login, password reset with one-time codes
- Profile, addresses, preferences and the customer's own pets
- Cart → order → Stripe checkout; payment status confirmed through the Stripe webhook
- Order tracking with a timeline of status events and email notifications
- Product reviews, average rating and rating distribution; review reminders after delivery
- Advanced product search, saved searches/filters and **price-drop alerts**
- Shipping cost calculation by zone and shipping method

**Back office**
- CRUD for products, categories and animals (role-protected)
- User management: change role, activate/deactivate
- Dashboard summary and reports: low stock, frequent customers, sales by category, pending orders
- Bulk import from Excel (categories, products, animals) with **preview**, auto-detection of the sheet type and downloadable templates

**Background jobs** (`BackgroundService`)
- `ReservaCleanupService`: releases expired stock reservations every minute
- `MonitorBackgroundService`: checks saved searches and price alerts every 6 hours and notifies users

## Project structure

```
Controllers/         HTTP endpoints (one per resource)
Services/            Business logic behind interfaces (I*Service), injected via DI
Models/              EF Core entities
Dto/                 Request/response contracts
AutoMapperProfiles/  Mapping profiles
Data/                DbContext
Migrations/          EF Core migrations
```

## Running locally

Requirements: .NET 8 SDK and SQL Server (LocalDB works).

```bash
dotnet restore

# Secrets are read from configuration; keep them out of appsettings.json
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "<a long random string>"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_..."
dotnet user-secrets set "SmtpSettings:Username" "you@gmail.com"
dotnet user-secrets set "SmtpSettings:Password" "<gmail app password>"

dotnet ef database update
dotnet run
```

Swagger UI is available at `/swagger` in Development. Use `POST /api/auth/login`, then click **Authorize** and paste `Bearer <token>`.

To test payments locally, forward Stripe events with the Stripe CLI:

```bash
stripe listen --forward-to https://localhost:<port>/api/stripe/webhook
```
