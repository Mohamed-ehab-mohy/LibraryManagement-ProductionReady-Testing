# LibraryManagement - Production-Ready Testing

A .NET 10 Library Management API with comprehensive testing: unit, integration, load, architecture, mutation, and snapshot tests. **89 tests total, all passing.**

## Architecture

```
API (LibraryManagement.API)
  └── Services (LibraryManagement.Services)
        └── Data (LibraryManagement.Data)
```

- **API** - Minimal API endpoints (`/api/books`, `/api/members`, `/api/loans`)
- **Services** - Business logic (`BookService`, `MemberService`, `LoanService`)
- **Data** - EF Core DbContext, entities, repositories, migrations (SQL Server)

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 10 |
| Database | SQL Server (SQL 2022 Express Docker) |
| ORM | Entity Framework Core 10 |
| Testing | xUnit, Shouldly, Moq |
| Load tests | NBomber, JMeter |
| Mutation tests | Stryker.NET |
| Architecture tests | NetArchTest.Rules |
| Snapshot tests | Verify.Xunit |
| Database tests | Testcontainers.MsSql |
| Containerization | Docker, Docker Compose |

## Quick Start

### Docker (recommended)

```bash
docker compose up -d
```

The API becomes available at `http://localhost:5041` with 10 seed books.

### Without Docker

Requires SQL Server. Update the connection string in `appsettings.json` then:

```bash
dotnet run --project src/LibraryManagement.API
```

## Run Tests

```bash
# All tests
dotnet test --no-restore

# Unit tests (65)
dotnet test tests/LibraryManagement.UnitTests

# Architecture tests (3)
dotnet test tests/LibraryManagement.ArchitectureTests

# Snapshot tests (6)
dotnet test tests/LibraryManagement.ScratchTests

# Integration tests (13) - requires Docker
dotnet test tests/LibraryManagement.IntegrationTests

# Load tests (2) - requires Docker API on localhost:5041
dotnet test tests/LibraryManagement.LoadTests

# Mutation testing
dotnet stryker --config-file stryker-config.json
```

## Testing Stats

| Suite | Tests | Status |
|-------|-------|--------|
| Unit | 65 | ✅ |
| Architecture | 3 | ✅ |
| Scratch (incl. snapshot) | 6 | ✅ |
| Integration | 13 | ✅ (with Docker) |
| Load (NBomber) | 2 | ✅ |
| **Total** | **89** | **✅** |

## Mutation Testing

Stryker.NET runs against `LibraryManagement.Services` targeting `LibraryManagement.UnitTests`.

- **Score: 81.82%** (90 killed, 20 survived, 0 no coverage)
- **High threshold: 80% | Low threshold: 75%**

The 20 survivors are mainly default string values in DTOs and boundary equality mutations - many are equivalent mutants that cannot practically be killed.

## Load Tests

- **NBomber** - 2 read scenarios (list + single book), integrated as xUnit tests
- **JMeter** - read-scenario + progressive stress-test (848K req, ~3000 req/s, 0% error)
- See [`load-test-comparison.md`](./load-test-comparison.md) for full comparison

## Architecture Constraints

Tests enforce three layer rules via NetArchTest:
- Services may only depend on API (not the other way around)
- Data may only depend on Services and API
- Controllers must reference `LibraryDbContext`, not the entire Data namespace

## Snapshot Tests

`BookSnapshotTests` uses Verify.Xunit to capture the `BookDto` JSON shape as a golden file. Any unintended change to the DTO output is detected on the next test run.

## Docker Setup

The API and SQL Server run in Docker Compose with:
- Automatic migration on startup
- Seed data (10 books) if the database is empty
- Health check ensuring SQL Server is ready before the API starts

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/books` | List all books (`?available=true` to filter) |
| GET | `/api/books/{id}` | Get book by ID |
| POST | `/api/books` | Create a book |
| POST | `/api/loans/borrow` | Borrow a book |
| POST | `/api/loans/return/{id}` | Return a book |
| GET | `/api/loans/member/{memberId}` | Get member's loans |
| POST | `/api/members` | Register a member |
| GET | `/api/members/{id}` | Get member details |
