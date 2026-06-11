# LibraryManagement  -  Production-Ready Testing

A .NET 10 Library Management API with comprehensive testing: unit, integration, load, architecture, mutation, and snapshot tests.

## Architecture

```
API (LibraryManagement.API)
  └── Services (LibraryManagement.Services)
        └── Data (LibraryManagement.Data)
```

- **API**  -  Minimal API endpoints (`/api/books`, `/api/members`, `/api/loans`)
- **Services**  -  Business logic (`BookService`, `MemberService`, `LoanService`)
- **Data**  -  EF Core DbContext, entities, repositories, migrations (SQL Server)

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 10 |
| Database | SQL Server (localdb dev, SQL 2022 Express Docker) |
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

The API becomes available at `http://localhost:5041`.

### Without Docker

Requires SQL Server (localdb or full instance). Update the connection string in `appsettings.json` then:

```bash
dotnet run --project src/LibraryManagement.API
```

## Run Tests

```bash
# Unit tests
dotnet test tests/LibraryManagement.UnitTests

# Architecture tests
dotnet test tests/LibraryManagement.ArchitectureTests

# Mutation testing (requires Docker for integration test containers)
dotnet stryker --config-file stryker-config.json

# Snapshot tests
dotnet test tests/LibraryManagement.ScratchTests

# Load tests
dotnet test tests/LibraryManagement.LoadTests
```

## Testing Stats

| Suite | Tests | Status |
|-------|-------|--------|
| Unit | 65 | ✅ |
| Architecture | 3 | ✅ |
| Scratch (incl. snapshot) | 6 | ✅ |
| Mutation score | 81.82% | ✅ (threshold: 75%) |
| Integration | 15 | ✅ (with Docker) |
| Load (NBomber) | 3 scenarios | ✅ |

## Mutation Testing

Stryker.NET runs against `LibraryManagement.Services` targeting `LibraryManagement.UnitTests`.

Minimum acceptable score: **75%** (configured in `stryker-config.json`).

The current score of **81.82%** means 90 of 110 non-trivial mutants were killed. The 20 survivors are mainly default string values in DTOs and boundary equality mutations  -  many are equivalent mutants that cannot practically be killed.

100% mutation score is not always the goal because:
- Equivalent mutants (code changes that produce identical behaviour) inflate the denominator
- DTO default initializers and exception message strings have minimal risk
- The marginal effort to kill the last few mutants often outweighs the benefit

## Load Tests

- **NBomber**  -  read + borrow scenarios with assertions, integrated as xUnit tests
- **JMeter**  -  read-scenario + stress-test `.jmx` files
- See [`load-test-comparison.md`](./load-test-comparison.md) for full comparison

## Snapshot Tests

`BookSnapshotTests` in `LibraryManagement.ScratchTests` uses Verify.Xunit to capture the `BookDto` JSON shape as a golden file. Any unintended change to the DTO output (field rename, type change, value shift) is detected on the next test run.

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
