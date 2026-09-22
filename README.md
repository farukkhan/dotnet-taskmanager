# .NET Task Manager

A hands-on application built with **.NET 10**, **ASP.NET Core**, **Entity Framework Core**, and **PostgreSQL**.

The project is used to explore modern .NET technologies, architectural patterns, database development, testing, cloud technologies, and modern software engineering practices through a continuously evolving application.

The application currently provides basic task management functionality. Over time, the project may evolve into a real-world business application, potentially including **Samsa Supplier Management** and related business functionality.

---

## Goals

The project provides a practical environment for exploring:

* .NET 10 and modern C#
* ASP.NET Core
* Entity Framework Core
* PostgreSQL
* Clean Architecture
* SOLID principles
* Domain-driven design concepts
* Repository and use-case patterns
* Automated testing
* Optimistic concurrency
* SQL performance and indexing
* Pagination and filtering
* Caching
* Background processing
* Resilience
* Docker
* Azure
* CI/CD
* Logging and observability
* AI-assisted development

The application is intentionally developed incrementally. New technologies and architectural approaches are introduced as the application evolves.

---

## Current Features

The current backend provides:

* Create tasks
* Retrieve tasks
* Retrieve a task by ID
* Update tasks
* Delete tasks
* Complete/reopen tasks
* PostgreSQL persistence
* Entity Framework Core migrations
* Optimistic concurrency using a version number
* Global API exception handling
* Environment-based database configuration
* Docker Compose for PostgreSQL
* Automated tests

A simple frontend will be added to interact with the API.

---

## Technology Stack

| Technology            | Purpose                    |
| --------------------- | -------------------------- |
| .NET 10               | Application platform       |
| C#                    | Programming language       |
| ASP.NET Core          | Web API                    |
| Entity Framework Core | ORM / data access          |
| PostgreSQL            | Relational database        |
| Docker Compose        | Local database environment |
| xUnit                 | Testing                    |
| OpenAPI               | API documentation          |

---

## Solution Structure

```text
dotnet-taskmanager/
│
├── src/
│   ├── Domain/
│   │   └── Domain models and business rules
│   │
│   ├── Application/
│   │   └── Use cases, interfaces and application exceptions
│   │
│   ├── Infrastructure/
│   │   └── EF Core, PostgreSQL and repositories
│   │
│   └── TaskManager.Api/
│       └── ASP.NET Core API
│
├── tests/
│   └── Automated tests
│
├── docs/
│   ├── architecture/
│   └── interview-challenges/
│
├── docker-compose.yml
├── .env.example
├── .gitignore
└── dotnet-taskmanager.slnx
```

> The project structure will evolve as new domains and technologies are introduced.

---

# Getting Started

## Prerequisites

Install the following:

* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/)
* Git

Verify the .NET installation:

```bash
dotnet --version
```

Verify Docker:

```bash
docker --version
```

---

## Clone the Repository

```bash
git clone <repository-url>
cd dotnet-taskmanager
```

---

# Local Environment Configuration

The application uses a local `.env` file for environment-specific configuration such as the PostgreSQL connection string.

The actual `.env` file is **not committed to Git** because it may contain credentials or other machine-specific configuration.

A `.env.example` file is committed to the repository and acts as the template for developers.

### 1. Create your local `.env`

From the project root, copy:

```text
.env.example
```

to:

```text
.env
```

For example, on Windows:

```powershell
Copy-Item .env.example .env
```

On Linux/macOS:

```bash
cp .env.example .env
```

### 2. Configure the `.env`

Open `.env` and update the values for your local environment.

Example:

```env
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=taskmanager;Username=postgres;Password=your-password
```

### Important

Do **not** commit `.env`.

The repository already includes `.env` in `.gitignore`.

When configuration changes are required, update `.env.example` with the required variable names and safe example values instead.

For example:

```text
.env.example  → committed
.env          → local only
```

The API uses the `DotNetEnv` package to load `.env` during development.

---

# Start PostgreSQL

Start the PostgreSQL container:

```bash
docker compose up -d
```

Check the container:

```bash
docker compose ps
```

Stop the database:

```bash
docker compose down
```

---

# Entity Framework Core

The application uses EF Core migrations to manage the database schema.

## Apply Existing Migrations

After configuring `.env` and starting PostgreSQL:

```bash
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/TaskManager.Api
```

On Windows PowerShell, the command can also be written on one line:

```powershell
dotnet ef database update --project src/Infrastructure --startup-project src/TaskManager.Api
```

## Create a New Migration

When the database model changes:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Infrastructure \
  --startup-project src/TaskManager.Api
```

Example:

```bash
dotnet ef migrations add AddTaskPriority \
  --project src/Infrastructure \
  --startup-project src/TaskManager.Api
```

Then apply it:

```bash
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/TaskManager.Api
```

---

# Run the Application

## 1. Start PostgreSQL

```bash
docker compose up -d
```

## 2. Configure `.env`

Make sure the local `.env` file exists and contains the correct database connection string.

## 3. Apply database migrations

```bash
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/TaskManager.Api
```

## 4. Start the API

```bash
dotnet run --project src/TaskManager.Api
```

ASP.NET Core will display the URLs where the API is listening.

---

# API

The current API supports the basic Task CRUD lifecycle.

Typical operations include:

```text
POST   /api/tasks
GET    /api/tasks
GET    /api/tasks/{id}
PUT    /api/tasks/{id}
DELETE /api/tasks/{id}
```

The exact routes and request/response models are defined by the API implementation.

OpenAPI/Swagger can be used to inspect and manually test the API when enabled.

---

# Architecture

The application currently follows a layered architecture with clear dependency direction.

```text
                 ┌───────────────────┐
                 │   TaskManager.Api │
                 └─────────┬─────────┘
                           │
                           ▼
                 ┌───────────────────┐
                 │   Application     │
                 └─────────┬─────────┘
                           │
                           ▼
                 ┌───────────────────┐
                 │      Domain       │
                 └───────────────────┘

                 ┌───────────────────┐
                 │  Infrastructure   │
                 └───────┬───────────┘
                         │
                   ┌─────┴─────┐
                   ▼           ▼
              Application    Domain
```

## Domain

Contains the core business model and business rules.

The current primary domain entity is:

```text
TaskItem
 ├── Id
 ├── Title
 ├── Description
 ├── IsCompleted
 ├── CreatedAt
 ├── UpdatedAt
 └── Version
```

The Domain project does not depend on Entity Framework Core or ASP.NET Core.

## Application

Contains application-level behavior and abstractions, including:

* Use cases
* Repository interfaces
* Application exceptions
* Application-specific logic

## Infrastructure

Contains implementation details such as:

* Entity Framework Core
* PostgreSQL
* `DbContext`
* Database entities
* Repository implementations
* Domain/persistence mapping

## API

Contains the HTTP-facing layer:

* Controllers/endpoints
* Dependency injection
* Exception handling
* OpenAPI configuration
* Environment configuration

---

# Optimistic Concurrency

Task updates use an application-managed integer `Version`.

For example:

```text
Version = 5
```

When a client reads a task, it receives the current version.

An update must provide the version that the client originally read.

If another client has already modified the task:

```text
Client expects: 5
Database has:   6
```

the update results in a concurrency conflict.

The API returns:

```text
409 Conflict
```

This prevents an older client from silently overwriting a newer change.

Entity Framework Core's concurrency-token mechanism protects the actual database update from race conditions.

---

# Exception Handling

The API uses ASP.NET Core's global exception handling mechanism.

Application exceptions are translated into appropriate HTTP responses.

| Exception              | HTTP Status |
| ---------------------- | ----------: |
| `NotFoundException`    |         404 |
| `ConcurrencyException` |         409 |
| Unexpected exception   |         500 |

HTTP-specific behavior remains in the API layer rather than being spread throughout the domain and infrastructure layers.

---

# Testing

Run all tests:

```bash
dotnet test
```

The test suite covers and will continue to expand around:

* Domain behavior
* Application use cases
* Repository behavior
* API behavior
* Database integration
* Validation
* Concurrency
* Edge cases

---

# Development Roadmap

The project will evolve incrementally.

## Phase 1 — Backend Foundation

* [x] Basic API
* [x] PostgreSQL
* [x] Docker Compose
* [x] Environment configuration
* [x] EF Core
* [x] EF Core migrations
* [x] CRUD operations
* [x] Domain model
* [x] Repository
* [x] Application use cases
* [x] Global exception handling
* [x] Optimistic concurrency
* [x] Automated tests

## Phase 2 — Frontend

* [ ] Simple frontend application
* [ ] Task list
* [ ] Create task
* [ ] Edit task
* [ ] Delete task
* [ ] Complete/reopen task
* [ ] API integration
* [ ] Validation and error handling
* [ ] Handle `409 Conflict`

The frontend will initially remain simple so that the backend and technology exploration remain the primary focus.

## Phase 3 — Architecture & Domain Exploration

* [ ] Review and improve architecture boundaries
* [ ] Explore alternative repository approaches
* [ ] Explore DTO and mapping strategies
* [ ] Validation strategies
* [ ] Domain events
* [ ] Aggregate boundaries
* [ ] Feature-oriented organization

## Phase 4 — Database & Performance

* [ ] Indexes
* [ ] Query optimization
* [ ] `AsNoTracking`
* [ ] Projection
* [ ] Pagination
* [ ] Filtering
* [ ] Sorting
* [ ] `ExecuteUpdate`
* [ ] `ExecuteDelete`
* [ ] Query analysis
* [ ] N+1 query scenarios

## Phase 5 — Advanced Backend

* [ ] Caching
* [ ] Background processing
* [ ] Retry strategies
* [ ] Resilience
* [ ] Rate limiting
* [ ] Idempotency
* [ ] Advanced concurrency scenarios

## Phase 6 — Cloud & DevOps

* [ ] Azure
* [ ] CI/CD
* [ ] Containerization
* [ ] Application configuration
* [ ] Structured logging
* [ ] Metrics
* [ ] Distributed tracing
* [ ] Observability

## Phase 7 — Real-World Domain

The generic task management functionality may eventually evolve into a real business application.

One possible direction is **Samsa Supplier Management**, potentially covering:

```text
Supplier
 ├── Company information
 ├── Contacts
 ├── Products
 ├── Pricing
 ├── MOQ
 ├── Lead times
 └── Documents

Product
 ├── SKU
 ├── Supplier
 ├── Cost
 ├── Selling price
 ├── Stock
 └── Packaging

Purchase Order
 ├── Supplier
 ├── Products
 ├── Quantities
 ├── Costs
 ├── Order date
 ├── Expected delivery
 └── Status
```

This is a potential direction rather than a fixed design. The actual domain model will be based on real requirements as the application evolves.

---

# Development Philosophy

The project favors **incremental development** over designing the complete system upfront.

The general approach is:

```text
Build
  ↓
Use
  ↓
Identify a real problem
  ↓
Explore alternative approaches
  ↓
Improve
  ↓
Add the next capability
```

When introducing a new technology or architectural pattern, the goal is to understand:

* Why it is useful
* When it should be used
* What alternatives exist
* What the trade-offs are
* How older applications commonly solve the same problem
* How the implementation behaves in real-world scenarios

The application can therefore evolve naturally from a simple Task Management API into a more complete real-world business application.

---

# Future Application Direction

The current project name and structure are intentionally kept generic while the technology foundation is being developed.

If the application evolves into a Samsa-focused business application, the repository, solution, projects, and domain terminology can be renamed while preserving the existing Git history and reusable technical foundation.
