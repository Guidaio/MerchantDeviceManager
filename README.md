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

Open http://localhost:5000 (or the port shown). Select a merchant (tenant), then manage devices. Use "Switch Merchant" to change tenant.

## Multi-tenant (Etapa 3)

- **Tenant resolution:** Header `X-Tenant-Id` or cookie `TenantId` (set when selecting merchant).
- **ITenantContext:** Scoped service provides `CurrentMerchantId` for the request.
- **Data filter:** DevicesController filters by `MerchantId = CurrentMerchantId`; each operator sees only their merchant's data.
- **TenantResolutionMiddleware:** Validates merchant exists before setting tenant.

## Role-based access (Etapa 4)

- **Roles:** Admin (full access), Support (create/view), Viewer (read-only).
- **Role resolution:** Header `X-Role` or cookie `Role`; selected when choosing merchant.
- **IRoleContext:** Scoped service provides `CurrentRole`, `CanCreate`, `CanDelete`.
- **RequireRoleAttribute:** Authorization filter; Create requires Admin or Support; Viewer gets 403 on create.

## Domain model

| Entity | Description |
|--------|-------------|
| **Merchant** | Tenant: Id, Name, Document (CNPJ/CPF), Status (Active/Inactive/Suspended), CreatedAtUtc |
| **Device** | POS terminal: Id, MerchantId, SerialNumber, Model, Status (Active/Inactive/Blocked), CreatedAtUtc |

SerialNumber is unique per merchant. Document is unique globally.

## Status

**Etapa 4 complete.** Role-based access (Admin, Support, Viewer). Next: Redis cache.

See `portfolio-notes.md` for the roadmap and execution history.
