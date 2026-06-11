# Load Test Comparison — NBomber vs JMeter

## Setup

- **Endpoint:** `http://localhost:5041/api/books` (GET all books)
- **NBomber scenario:** `list_all_books` — 100 req/sec for 30 seconds (Simulation.Inject)
- **JMeter scenario:** `read-scenario.jmx` — 50 threads, ramp-up 10s, loop 10

---

## Results

| Metric              | NBomber | JMeter |
|---------------------|---------|--------|
| Total requests      |         |        |
| Throughput (req/s)  |         |        |
| Average latency     |         |        |
| Min latency         |         |        |
| Max latency         |         |        |
| p50 (median)        |         | —      |
| p95                 |         | —      |
| p99                 |         | —      |
| Error rate (%)      |         |        |

---

## Analysis

<!-- Write 3–5 sentences explaining:
     - Are the results similar?
     - What could cause differences?
     - What does each tool do better? -->

<!-- Example:
     The results are broadly similar, with both tools reporting sub-100ms average latency.
     NBomber shows slightly higher throughput because it uses async I/O natively,
     while JMeter (Java) has higher baseline overhead per thread.
     NBomber's reporting is better for percentile analysis (p50/p95/p99 built in),
     while JMeter's GUI makes it easier to spot trends over time.
     Differences in max latency may come from GC pauses in the .NET app under test,
     which both tools capture equally well.
-->

---

## CI/CD Recommendation

<!-- Which tool would you use for running load tests in a CI/CD pipeline, and why?
     Example:
     I would use NBomber in CI/CD because it runs as a .NET test alongside other tests,
     requires no GUI or Java runtime, and the pass/fail assertions can be integrated
     directly into dotnet test.
-->
