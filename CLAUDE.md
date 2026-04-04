# LUM Store API — CLAUDE.md

## Project Overview

**LUM Store API** is a Content Management System (CMS) and E-Commerce Platform built on **.NET 10.0 / ASP.NET Core**. It provides a hierarchical tree-based content management engine, product catalog, media library, user authentication, and audit logging.

---

## Solution Structure (Clean Architecture)

```
lum-store-api/
├── LumStoreAPI/                    # Web API entry point — controllers, Program.cs, middleware
├── LumStoreAPI.Application/        # Application layer — services, DTOs, business logic
├── LumStoreAPI.Core/               # Domain layer — entities, interfaces, enums
├── LumStoreAPI.DataEngine/         # Custom query engine — tree navigation, paging
├── LumStoreAPI.Infrastructure/     # Data access — EF Core, repositories, migrations
├── LumStoreAPI.Libraries/          # Shared helpers and utilities
└── LumStoreAPI.Tasks/              # Background/hosted services (email queue)
```

---

## Technology Stack

| Area | Technology |
|------|-----------|
| Runtime | .NET 10.0 / ASP.NET Core |
| ORM | Entity Framework Core 10.0.3 (SQL Server) |
| Auth | JWT Bearer + HttpOnly cookies (BCrypt passwords) |
| Email | MailKit 4.15.1 (async queue via background service) |
| Logging | Serilog 4.3.1 (console + rolling file) + EventLog DB table |
| Cache | IMemoryCache (in-memory, dependency-tracked) |
| Docs | Scalar + OpenAPI (Swagger UI at `/scalar`) |
| Database | SQL Server Express — local dev `.\SQLEXPRESS`, DB `LumDbContext` |

---

## Business Domain

The platform manages **hierarchical content nodes** (tree structure), where each node can represent a page, product, category, folder, or other typed content. Supports:

1. **CMS Tree** — parent-child `DocumentNode` hierarchy with deep ancestor tracking
2. **Product Catalog** — polymorphic `Product` pages with categories
3. **Media Library** — file upload/folder management, served from `wwwroot/Medias`
4. **User Auth** — JWT + refresh token pair stored in HttpOnly cookies
5. **Email Queue** — async SMTP sending via `EmailSenderBackgroundService`
6. **Audit Log** — `EventLog` table capturing errors, warnings, and activity

---

## Key Entities

### User & Auth
- `User` — account with role (ADMIN / USER), `AccountStatus` (ACTIVE / LOCKED / INACTIVE)
- `UserToken` — refresh token storage (7-day expiry)

### Content Tree
- `DocumentNode` — tree node with parent-child relationship
- `DocumentPage` — typed content page (polymorphic base)
- `DocumentLinkedNode` — ancestor-descendant closure table for deep queries
- `HomePage`, `Product`, `ProductCategory` — concrete page types extending `DocumentPage`

### Media
- `MediaLibrary` — file metadata (name, size, dimensions, extension)
- `MediaLibraryCategory` — folder organization

### System
- `EventLog` — audit trail (ERROR / WARNING / INFORMATION)
- `EmailQueue` — email staging with status (Pending / Success / Failed)
- `SettingKeyValue` — key-value configuration storage

**Base classes:**
- `BaseItem` — `CreatedAt`, `UpdatedAt` (auto-set by DbContext)
- `BaseClassItem` (extends BaseItem) — adds `ItemID`, `IsDeleted`, `IsArchived`

---

## API Endpoints

### Auth — `/api/auth`
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/Register` | Anonymous | Register user, send verification email |
| POST | `/Login` | Anonymous | Login, set JWT + refresh cookies |
| POST | `/Logout` | Anonymous | Clear auth cookies |
| POST | `/Verify` | Pre role | Verify email code, activate account |
| GET | `/CurrentUser` | Authenticated | Get current user info |

### Media — `/api/media` (ADMIN only)
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/Files` | List files (paginated) |
| GET | `/Folders` | List folders |
| POST | `/Files` | Upload file (multipart) |
| POST | `/Folders` | Create folder |
| GET | `/getFile` | Download file by ID (anonymous) |
| PUT | `/Files` | Update file metadata |
| DELETE | `/Files/{fileID}` | Delete file |
| PUT | `/Folders/{folderID}` | Rename folder |
| DELETE | `/Folders/{folderID}` | Delete folder |

### Tree Node Manager — `/api/treenode` (ADMIN only)
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List nodes (paginated, cached) |
| GET | `/{nodeId}` | Get single node |
| POST | `/` | Create node/page |
| PUT | `/` | Update page properties |
| PATCH | `/Rename` | Rename node |
| PATCH | `/ReOrder` | Move/reorder node in tree |
| DELETE | `/` | Delete node (soft or hard) |

### Other (ADMIN only)
- `GET /api/documenttype` — List registered page types
- `GET /api/documenttype/{className}` — Get schema for page type
- `GET /api/eventlog` — List audit logs (filterable)
- `GET /api/eventlog/{eventID}` — Get single event log

---

## Services

### Application Layer (`LumStoreAPI.Application/Services`)
- **AuthService** — register, login, logout, JWT + refresh token lifecycle
- **UserService** — current user resolution, account status checks
- **MediaService** — file upload to `wwwroot/Medias`, folder CRUD
- **EventLogService** — async DB event logging with IP/URL context

### Infrastructure Layer (`LumStoreAPI.Infrastructure`)
- **JwtTokenService** — token generation/validation (HS256, claims: id/name/email/jti/role)
- **CacheService** — memory cache with dependency invalidation builder
- **EmailService** — queue emails to DB; background service sends them (every 10s, max 5 concurrent)
- **DocumentTableService** — reflect `[RegisterPageType]` classes, return schema for UI
- **TreeNodeRepository** — CRUD + move/reorder + polymorphic page handling
- **PageRetrieveContext** — custom query engine with caching for page data

---

## Authentication & Authorization

- **JWT** with `HS256`, access token lifetime: 24h (or 7d with "remember me")
- **Refresh tokens** (Base64 random string) stored in `UserToken` table, expire in 7 days
- Tokens delivered as **HttpOnly cookies**: `AccessToken`, `RefreshToken`
- CORS allows `https://localhost:3000` with credentials
- **Roles**: `ADMIN`, `USER`, `pre` (pre-verified, can only call `/Verify`)
- Default `[Authorize]` policy requires ADMIN or USER role
- Media, TreeNode, DocumentType, EventLog controllers require **ADMIN** role

**Token validation pipeline:**
1. Extract from `Authorization: Bearer` header, fallback to cookie
2. Validate signature (lifetime check disabled — `ValidateLifetime: false`)
3. Verify user exists and account status
4. LOCKED → HTTP 423; INACTIVE → claim added

---

## Cross-Cutting Concerns

### Middleware Pipeline
```
ExceptionHandler → HTTPS Redirect → CORS → Authentication → Authorization → Controllers
```

### Global Exception Handler
- Catches all unhandled exceptions
- Logs via `EventLogService`
- Returns RFC 7807 `ProblemDetails`
- Special handling for `ForbidException` (403) and `AuthInvalidException` (401)

### Custom Authorization Handler (`AuthorizeHandler`)
- Overrides default 401/403 response format with custom JSON

### Caching
- `ICacheService` wraps `IMemoryCache`
- Cache keys built with `CacheDependency` pattern
- Invalidated on entity changes (tracked in `LumStoreContext.SaveChangesAsync`)

### Serilog
- Console + rolling daily file: `logs/log-*.txt`
- Error-level file sink

---

## DI Registration (Program.cs)

```csharp
builder.Services.LumStoreRepoConfigurations()   // Repos, DbContext
builder.Services.DataEngine()                   // PageRetrieveContext
builder.Services.LumStoreApplicationConfigurations()  // App services, JWT
builder.Services.RegisterTasks()                // Background services
builder.Services.AddCMSCache()                  // IMemoryCache
```

---

## Page Type System

Page types are registered via reflection using custom attributes:

```csharp
[RegisterPageType]
[DocumentName("Pages.Product")]
public class Product : DocumentPage { ... }
```

- `DocumentPageTypeHelper.RegisterPageTypes()` scans assemblies on startup
- `IDocumentTableService` returns schema info per class name
- Enables polymorphic content where the UI can render type-specific forms

---

## Infrastructure Notes

- **Migrations** in `LumStoreAPI.Infrastructure/Migrations/`
- **Entity configs** in `Infrastructure/Configurations/` (Fluent API via `IEntityTypeConfiguration`)
- **Media files** stored at `wwwroot/Medias` (configured via `MediaLibraryHelper.RootMediaPath`)
- **Ports**: HTTPS `7084`, HTTP `5062` (dev)
- **Background service**: `EmailSenderBackgroundService` — polls every 10s, sends up to 30 emails per batch with max 5 concurrent sends
