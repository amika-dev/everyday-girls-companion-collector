# Testing Guide (Everyday Girls: Companion Collector)

## Tech

* .NET 10
* Test framework: xUnit
* Mocking: Moq
* Integration: Microsoft.AspNetCore.Mvc.Testing + SQLite (in-memory)

## Projects

* Unit tests: `EverydayGirls.Tests.Unit`
* Integration tests: `EverydayGirls.Tests.Integration`

## Authoritative requirements

Use these docs as the source of truth:

* `PROJECT_OVERVIEW.md`
* `UI_DESIGN_CONTRACT.md`

## Conventions

* Prefer testing *services/domain logic* over controllers where possible.
* Keep tests deterministic:

  * If time is involved, code should use an injected clock abstraction (e.g., `IClock.UtcNow`).
  * If randomness is involved, code should use an injected random abstraction (e.g., `IRandom.NextDouble()`).
* Name tests using the pattern: `Method_Condition_ExpectedResult`.
* Keep unit tests fast: no HTTP, no database unless explicitly needed.
* Integration tests may use HTTP + DB and should focus on wiring, auth, routing, and persistence behaviors.
