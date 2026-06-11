# Load Test Comparison - NBomber vs JMeter

## Setup

- **Endpoint:** `http://localhost:5041`
- **NBomber scenarios:**
  - `list_all_books` - GET /api/books, 100 req/s for 30 s
  - `get_single_book` - GET /api/books/{id}, 50 req/s for 30 s
  - `borrow_book` - POST /api/loans, 20 req/s for 60 s
- **JMeter scenarios:**
  - `read-scenario.jmx` - 50 threads, ramp-up 10 s, loop 10
  - `stress-test.jmx` - progressive ramp-up stress test

## Results

| Metric              | NBomber | JMeter |
|---------------------|---------|--------|
| Total requests      | -       | -      |
| Throughput (req/s)  | -       | -      |
| Average latency     | -       | -      |
| Min latency         | -       | -      |
| Max latency         | -       | -      |
| p50 (median)        | -       | -      |
| p95                 | -       | -      |
| p99                 | -       | -      |
| Error rate (%)      | -       | -      |

<!-- Run both tools against the same environment and fill in the table above. -->

## Analysis

NBomber and JMeter target the same HTTP endpoints but differ in how they generate load. NBomber is written in .NET and runs as an xUnit test; it uses async I/O natively with `HttpClient.SendAsync`, making it lightweight per virtual user. JMeter runs on the JVM and each thread represents a heavier OS-level user, which can limit concurrency on lower-end machines.

NBomber gives richer built-in percentile reporting (p50, p95, p99) and is directly integrable into CI via `dotnet test`. JMeter provides a GUI for designing complex workflows and its `.jmx` format is portable across teams.

## CI/CD Recommendation

NBomber is the better fit for CI/CD because:
- It runs as a `dotnet test` alongside other test projects - no extra runtime or GUI needed
- Assertions (e.g., p99 < 300 ms, zero failures) are written in C# and enforced at build time
- Reports are generated as HTML in the pipeline artifacts

JMeter is better suited for exploratory or ad-hoc load testing where the GUI helps iterate quickly on test design.
