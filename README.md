# 🎬 Cinema Platform API

Backend API for a Cinema Management System developed as part of the **SoftServe Practice**. This solution provides a comprehensive RESTful API for managing movies, sessions, halls, and ticket bookings using Clean Architecture principles.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=flat&logo=postgresql)
![Redis](https://img.shields.io/badge/Redis-Cache-DC382D?style=flat&logo=redis)
![Supabase](https://img.shields.io/badge/Supabase-Database-3ECF8E?style=flat&logo=supabase)
![SignalR](https://img.shields.io/badge/SignalR-RealTime-512BD4?style=flat)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat&logo=docker)
![License](https://img.shields.io/badge/License-MIT-green)

> **Frontend:** Check out [cinema-platform-front](https://github.com/stkossman/cinema-platform-front) for the React + TypeScript client.

---

## 🏗️ Architecture & Tech Stack

The project follows **Clean Architecture** combined with the **CQRS** (Command Query Responsibility Segregation) pattern.

| Layer | Technologies |
|---|---|
| **API** | ASP.NET Core 8 Web API, Swagger/OpenAPI, SignalR |
| **Application** | MediatR (CQRS), FluentValidation, Mapster, Domain Events |
| **Domain** | Entities, Value Objects, Enums, Domain Exceptions, Result<T> pattern |
| **Infrastructure** | EF Core, PostgreSQL (Supabase), Redis, Refit (TMDB), Hangfire, Serilog |
| **Auth** | ASP.NET Core Identity, JWT (Access + Refresh Tokens) |
| **Patterns** | Idempotency, Outbox, Repository, Unit of Work |
| **DevOps** | Docker & Docker Compose |

---

## 📂 Project Structure

```
Cinema.Api/                # Controllers, Middleware, SignalR Hubs, Entry Point
├── Controllers/           # Account, Auth, Genres, Halls, Movies, Orders, Pricings, etc.
├── Hubs/                 # TicketHub (SignalR real-time notifications)
├── ExceptionHandlers/    # GlobalExceptionHandler
├── Middleware/           # RequestLogContextMiddleware
└── Services/             # CurrentUserService, SignalRTicketNotifier

Cinema.Application/        # Business Logic, CQRS Handlers, DTOs, Validators
├── Account/              # Profile & password commands/queries
├── Auth/                 # Login, Register, RefreshToken
├── Genres/               # Genre CRUD
├── Halls/                # Hall CRUD + technologies management
├── Movies/               # Movie CRUD + TMDB import
├── Orders/               # CreateOrder, CancelOrder, GetMyOrders, ValidateTicket
├── Pricings/             # Pricing CRUD + price calculation
├── Seats/                # Seat types, locking logic
├── Sessions/             # Session scheduling, reschedule, cancel
├── Technologies/         # Hall technologies (IMAX, 3D, Dolby Atmos)
├── Users/                # Role management
├── Jobs/                 # CancelExpiredOrdersJob (Hangfire recurring job)
└── Common/
    ├── Behaviours/       # ValidationBehavior, IdempotencyBehavior
    ├── Interfaces/       # IPaymentService, IPriceCalculator, ITicketNotifier, etc.
    └── Mappings/         # Mapster configuration

Cinema.Domain/             # Core: Entities, Value Objects, Enums, Events
├── Entities/             # Movie, Hall, Session, Seat, Order, Ticket, Genre, Pricing, etc.
├── Enums/                # MovieStatus, SeatStatus, SessionStatus, OrderStatus, TicketStatus
├── Events/               # OrderPaidEvent (domain events)
└── Shared/               # Result<T>, Error, EntityId<T>

Cinema.Infrastructure/     # Data, Caching, External APIs, Identity
├── Persistence/
│   ├── Configurations/   # EF Core entity configurations
│   ├── Migrations/       # Database migrations
│   └── ApplicationDbContext.cs
└── Services/             # Identity, Token, Redis SeatLocking, TMDB, Payment, etc.
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- **Supabase** account (for PostgreSQL) *or* local PostgreSQL
- **Redis** instance

### Option 1 — Docker Compose *(Recommended)*

Spins up the API, Redis, and Redis Commander:

1. Create a `.env` file in the root directory:
```env
SUPABASE_CONNECTION_STRING=Host=<your-supabase-host>;Database=postgres;Username=postgres;Password=<your-password>
REDIS_PASSWORD=your-redis-password
JWT_SECRET=your-super-secret-jwt-key-min-32-chars
```

2. Run:
```bash
docker-compose up -d --build
```

The API will be available at `http://localhost:5000` and `https://localhost:5001`.

### Option 2 — Run Locally

1. Clone the repository:
```bash
git clone https://github.com/your-username/cinema-platform-backend.git
cd cinema-platform-backend
```

2. Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=cinema;Username=postgres;Password=postgres",
    "RedisConnection": "localhost:6379"
  },
  "JwtSettings": {
    "Secret": "your-32-char-secret-key",
    "Issuer": "CinemaApi",
    "Audience": "CinemaClient"
  }
}
```

3. Restore, migrate, and run:
```bash
dotnet restore
dotnet ef database update --project Cinema.Infrastructure --startup-project Cinema.Api
dotnet run --project Cinema.Api
```

---

## 🗺️ Roadmap & Requirements

Progress tracking based on **SoftServe Practice** requirements.

### 👤 Administrator

- [x] **Movie Management** — Create, update, delete movies (including TMDB import)
- [x] **Session Management** — Schedule sessions, detect overlaps, manage pricing policies
- [x] **Hall Management** — Configure halls, seat layouts, and technologies (IMAX, 3D, etc.)
- [x] **Genre Management** — CRUD operations for movie genres
- [x] **Pricing Management** — Create dynamic pricing policies with seat type multipliers
- [ ] **Sales Statistics** — View sales stats and key metrics *(Extra Task)*

### 👤 Client

- [x] **Browse Offers** — View current movies and new releases
- [x] **Schedule Filtering** — View sessions with filters by date, time, and genre
- [x] **Movie Details** — Description, trailers, cast, ratings
- [x] **Authentication** — Registration and login via ASP.NET Core Identity
- [x] **Ticket Booking** — **COMPLETE ORDER FLOW IMPLEMENTED**
  - [x] Seat locking via Redis distributed lock (10-min TTL)
  - [x] Order creation with payment processing
  - [x] Automatic ticket generation
  - [x] Order history (active & past orders)
  - [x] Real-time ticket notifications via SignalR
  - [x] Ticket validation system
- [x] **Order Management**
  - [x] View my orders
  - [x] Cancel pending orders
  - [x] Automatic expiration of unpaid orders (Hangfire job)
- [ ] **Personalized Recommendations** — Based on booking history *(Extra Task)*

> ⭐ Items marked as *Extra Task* are bonus features from the practice requirements.

---

## 🔑 Key Implementation Details

### Complete Order & Payment Flow
The system now implements a **full end-to-end booking flow**:

1. **Seat Selection** → User browses available sessions and seats
2. **Seat Locking** → Redis distributed lock (10-min TTL) prevents double-booking
3. **Price Calculation** → Dynamic pricing based on seat type and session pricing policy
4. **Order Creation** → `CreateOrderCommand` generates order with `Pending` status
5. **Payment Processing** → `IPaymentService` handles payment (mock implementation included)
6. **Ticket Generation** → On payment success, tickets are auto-generated with `Valid` status
7. **Real-Time Notification** → SignalR pushes ticket data to client via `TicketHub`
8. **Order Expiration** → Hangfire job `CancelExpiredOrdersJob` auto-cancels unpaid orders

**Idempotency**: Duplicate order requests are prevented via `IdempotencyBehavior` using request IDs.

**Domain Events**: `OrderPaidEvent` triggers `OrderPaidEventHandler` to unlock seats and notify users.

### Seat Locking System
- **Technology**: Redis with distributed locks (StackExchange.Redis)
- **TTL**: 10 minutes (configurable in `RedisSeatLockingService`)
- **Resilience**: Polly retry policy with exponential backoff
- **Key Format**: `seat_lock:{sessionId}:{seatId}`
- **Auto-Release**: Locks expire automatically if payment isn't completed

### TMDB Integration
Movies can be imported directly from [The Movie Database](https://www.themoviedb.org/) by ID via the **Refit** HTTP client. Cast, posters, genres, and details are pulled automatically. Background import jobs are handled by **Hangfire**.

### Pricing System
- **Dynamic Pricing**: Admin creates pricing policies with base prices
- **Seat Type Multipliers**: Different seat types (Standard, VIP, Premium) apply multipliers
- **Price Calculator**: `IPriceCalculator` interface calculates final ticket prices
- **Session-Level Pricing**: Each session can use a different pricing policy

### Session Conflict Detection
The scheduling service checks for time-slot overlaps within the same hall before allowing a new session to be created or an existing one to be rescheduled.

### Authentication & Authorization
- **Registration**: Users register with email/password via ASP.NET Core Identity
- **Login**: Returns short-lived **Access Token** (JWT, 60-min) + long-lived **Refresh Token** (stored in DB)
- **Token Refresh**: Secure token rotation via `/api/auth/refresh`
- **Roles**: `Admin` and `User` roles with role-based endpoint protection

### Real-Time Features (SignalR)
- **Hub**: `TicketHub` at `/hubs/ticket`
- **Events**: `TicketCreated`, `OrderStatusChanged`
- **Client Integration**: Frontend subscribes to receive instant ticket updates after payment

### Background Jobs (Hangfire)
- **CancelExpiredOrdersJob**: Runs every 5 minutes to cancel orders that remain `Pending` past expiration time
- **Dashboard**: Hangfire dashboard available at `/hangfire` (dev environment)

### Object Mapping
- **Mapster**: High-performance object-to-object mapper
- **Configuration**: Mapping profiles in `Cinema.Application/Common/Mappings/`
- **Usage**: `entity.Adapt<Dto>()` for clean, type-safe transformations

---

## 📡 API Endpoints Overview

### Movies
- `GET    /api/movies` — List movies with pagination & filters
- `GET    /api/movies/{id}` — Get movie details
- `POST   /api/movies` — Create movie (Admin)
- `PUT    /api/movies/{id}` — Update movie (Admin)
- `DELETE /api/movies/{id}` — Delete movie (Admin)
- `POST   /api/movies/import` — Import from TMDB (Admin)

### Sessions
- `GET    /api/sessions` — List sessions with filters
- `GET    /api/sessions/{id}` — Get session details
- `POST   /api/sessions` — Create session (Admin)
- `PUT    /api/sessions/{id}/reschedule` — Reschedule (Admin)
- `PUT    /api/sessions/{id}/cancel` — Cancel session (Admin)

### Seats & Locking
- `POST   /api/seats/lock` — Lock a seat for booking (authenticated)
- `PUT    /api/seats/{id}/type` — Change seat type (Admin)

### Orders & Tickets
- `POST   /api/orders` — Create order (purchase tickets)
- `GET    /api/orders` — Get my orders
- `GET    /api/orders/{id}` — Get order details
- `PUT    /api/orders/{id}/cancel` — Cancel order
- `GET    /api/tickets/{id}` — Get ticket details
- `POST   /api/tickets/{id}/validate` — Validate ticket (Admin)

### Halls & Technologies
- `GET    /api/halls` — List halls
- `POST   /api/halls` — Create hall (Admin)
- `GET    /api/technologies` — List technologies (IMAX, 3D, etc.)

### Genres
- `GET    /api/genres` — List genres
- `POST   /api/genres` — Create genre (Admin)

### Pricings
- `GET    /api/pricings` — List pricing policies
- `POST   /api/pricings` — Create pricing (Admin)

### Auth & Account
- `POST   /api/auth/register` — Register new user
- `POST   /api/auth/login` — Login (returns JWT + Refresh Token)
- `POST   /api/auth/refresh` — Refresh access token
- `GET    /api/account/profile` — Get my profile
- `PUT    /api/account/profile` — Update profile
- `POST   /api/account/change-password` — Change password

---

## 🧪 Testing

### Postman Collection
A complete automated test suite is included: **Cinema Booking Flow (Auto).postman_collection.json**

**Test Flow:**
1. Login → Get JWT token
2. Get Sessions → Find `OpenForSales` session
3. Get Hall Seats → Pick 2 `Active` seats
4. Lock Seat 1 & 2 → Redis locks with 10-min TTL
5. Create Order → Process payment, generate tickets
6. Get My Orders → Verify order + save `ticketId`
7. Get Ticket Details → View ticket with QR code

**Environment Variables Required:**
- `baseUrl`: `http://localhost:5000`
- `email`: Your test user email
- `password`: Your test user password

---

## 🐳 Docker Deployment

The `docker-compose.yaml` includes:
- **cinema-api**: Main .NET 8 API container
- **redis**: Redis cache with password protection
- **redis-commander**: Web UI for Redis at `http://localhost:8081`

**Environment Variables** (in `.env` file):
- `SUPABASE_CONNECTION_STRING` — PostgreSQL connection string
- `REDIS_PASSWORD` — Redis password
- `JWT_SECRET` — JWT signing key (min 32 chars)

---

## 📜 License

This project is licensed under the **MIT License**.
