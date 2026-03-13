# MerchantDeviceManager

Portal multi-tenant para gestão de merchants e terminais POS. Portfolio project aligned with Senior .NET Backend (fintech/POS) roles.

## Context

MerchantDeviceManager is an MVC portal for managing **merchants** and **POS devices** in a multi-tenant scenario. Planned features include role-based access, Redis cache, and tenant isolation.

**Use case:** Back-office for a fintech or acquirer: each merchant has terminals (POS devices); operators have different roles (admin, support, viewer).

**Planned tech:** .NET 8, ASP.NET Core MVC, multi-tenant, roles, Redis (cache).

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│  MerchantDeviceManager.Web (MVC)                                        │
└─────────────────────────────────────────────────────────────────────────┘
         │                                    │
         ▼                                    ▼
┌──────────────────────────┐     ┌─────────────────────┐
│ MerchantDeviceManager.   │────▶│ MerchantDeviceManager│
│ Infrastructure           │     │ .Domain              │
│ (EF Core, Redis)         │     │ (entities, logic)   │
└──────────────────────────┘     └─────────────────────┘
```

| Project | Description |
|---------|-------------|
| **MerchantDeviceManager.Web** | ASP.NET Core MVC (.NET 8). Portal UI. |
| **MerchantDeviceManager.Domain** | Domain entities (Merchant, Device) and enums. |
| **MerchantDeviceManager.Infrastructure** | EF Core SQLite, DbContext, entity configurations. |

## Prerequisites

- .NET 8 SDK

## How to run

```bash
dotnet run --project src/MerchantDeviceManager.Web
```

## Domain model

| Entity | Description |
|--------|-------------|
| **Merchant** | Tenant: Id, Name, Document (CNPJ/CPF), Status (Active/Inactive/Suspended), CreatedAtUtc |
| **Device** | POS terminal: Id, MerchantId, SerialNumber, Model, Status (Active/Inactive/Blocked), CreatedAtUtc |

SerialNumber is unique per merchant. Document is unique globally.

## Status

**Etapa 2 complete.** Domain model (Merchant, Device), EF Core, SQLite. Next: multi-tenant setup, roles.

See `portfolio-notes.md` for the roadmap and execution history.
