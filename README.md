# BookTracker

Full-stack technical exercise: .NET 10 Web API + Angular 18 SPA + PostgreSQL.

> **User story** — As a book enthusiast, I want to log books I've read with a rating and a short review, so I can keep a personal history of my reading and see my progress over time.

## Architecture

Clean Architecture + DDD, four .NET projects with one-way dependencies:

```
backend/
├── BookTracker.slnx
├── Directory.Build.props
├── src/
│   ├── BookTracker.Domain          → Aggregates, value objects, repository contracts
│   │   ├── Common/                 — Entity, AggregateRoot, ValueObject, DomainException
│   │   ├── Users/                  — User (root), Email (VO), IUserRepository
│   │   └── Books/                  — Book (root), Rating (VO), IBookRepository
│   ├── BookTracker.Application     → Use cases, app-level abstractions, DTOs, validators
│   ├── BookTracker.Infrastructure  → ADO.NET repos, BCrypt, JWT, DbUp migrations
│   └── BookTracker.Api             → Controllers, JWT auth, Swagger, middleware
└── tests/
    └── one xUnit project per src/ project (Domain, App, Infra, Api)
frontend/
└── Angular 18 standalone SPA (signals, Reactive Forms, Tailwind)
```

Dependency rule — `Domain ← Application ← Infrastructure ← Api`. Domain references nothing.

### DDD building blocks

- **Aggregate roots** — `User` and `Book` each own their consistency boundary. Cross-aggregate references are by `Guid`, never by navigation.
- **Value objects** — `Email` and `Rating` enforce invariants at construction, expose value equality, and are immutable. A `Book` cannot exist with an invalid `Rating`; a `User` cannot exist with an invalid `Email`.
- **Repository contracts live in Domain** (`Users/IUserRepository.cs`, `Books/IBookRepository.cs`). Infrastructure implements them. Application orchestrates them.
- **No anemic entities** — `Book.Log`, `Book.Revise`, `User.Register` are intent-revealing factories/methods rather than property setters.

### Other design choices

- **No EF Core / no Dapper / no MediatR** (per exercise constraints). Persistence is raw `NpgsqlCommand` with typed `NpgsqlDbType` parameters. Migrations run via DbUp from embedded SQL scripts.
- **Primary constructors** (C# 12+) throughout services and use cases — less ceremony, fewer assignments.
- **Use cases are plain classes** invoked directly from controllers — no mediator pipeline.
- **Ownership enforced at the repository layer**: every `Book` query and command requires `user_id`. A user cannot read or mutate another user's books.
- **Aggregate roots use `Hydrate` factories** that re-validate invariants — corrupt DB rows fail fast at materialization time rather than silently leaking.
- **JWT bearer auth** with HS256 and a >=32 char signing key. `ICurrentUser` resolves `sub` from the JWT into a `Guid`.
- **RFC 7807 ProblemDetails** error responses through a single exception middleware (`NotFound`/`Conflict`/`Unauthorized`/`Validation`/`Domain` → corresponding status codes).
- **FluentValidation** at the API boundary; domain invariants enforced again inside aggregates (defense in depth).

## Stack

| Layer | Choice |
| --- | --- |
| Runtime | .NET 10 (C# 14) |
| DB | PostgreSQL 16 |
| Driver | Npgsql 10 (raw ADO.NET) |
| Migrations | DbUp 7 (embedded `.sql`) |
| Auth | JWT Bearer + BCrypt.Net-Next |
| Validation | FluentValidation 12 |
| API docs | Swashbuckle / Swagger UI |
| Tests | xUnit, FluentAssertions, NSubstitute, Testcontainers |
| Frontend | Angular 18 (standalone, signals), Reactive Forms, Tailwind |

## Prerequisites

- .NET 10 SDK
- Docker (Postgres runs in a container)
- Node 20+ (for the Angular app)

## Run

```bash
# 1. start Postgres
docker-compose up -d

# 2. run the API (migrations apply on startup, including the demo seed)
dotnet run --project backend/src/BookTracker.Api
#   → API on http://localhost:5000
#   → Swagger UI on http://localhost:5000/swagger

# 3. in another terminal, run the SPA
cd frontend
npm install
npm start
#   → Angular dev server on http://localhost:4200
```

### Demo credentials

```
email:    demo@demo.com
password: Demo@123
```

The seed migration inserts the demo user with 2 sample books.

## Tests

```bash
cd backend && dotnet test
```

- **Domain.Tests** — 29 unit tests, entity invariants
- **Application.Tests** — 14 unit tests with NSubstitute, use case behavior + auth ownership
- **Infrastructure.Tests** — 8 integration tests via Testcontainers Postgres, repo round-trips + cross-user isolation
- **Api.Tests** — 2 end-to-end tests via `WebApplicationFactory` + Testcontainers, register → login → create book flow

> 53 tests total. Infrastructure and Api tests need Docker running.

## API surface

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| POST | `/api/v1/auth/register` | anon | Create account, returns JWT |
| POST | `/api/v1/auth/login` | anon | Exchange creds for JWT |
| GET | `/api/v1/books` | JWT | List current user's books |
| GET | `/api/v1/books/{id}` | JWT | Get single book (owned) |
| POST | `/api/v1/books` | JWT | Create book |
| PUT | `/api/v1/books/{id}` | JWT | Update book (owned) |
| DELETE | `/api/v1/books/{id}` | JWT | Delete book (owned) |

OpenAPI document at `/openapi/v1.json` and Scalar reference UI at `/scalar/v1`.

## TDD workflow

Commit history shows the red-green-refactor cycle layer by layer:

```
scaffold → domain (tests + entities) → review fixes
        → application (use cases + abstractions, NSubstitute)
        → infrastructure (ADO.NET + Testcontainers integration tests)
        → api (controllers + WebApplicationFactory integration tests)
        → docker-compose + seed
        → angular scaffold → angular feature commit
```

`git log --oneline` makes the progression auditable.

## Repository layout

```
.
├── README.md
├── docker-compose.yml
├── backend/
│   ├── BookTracker.slnx
│   ├── Directory.Build.props      # shared TFM, nullable, warnings-as-errors
│   ├── src/                       # 4 .NET projects
│   └── tests/                     # 4 xUnit projects, Directory.Build.props relaxes warnings
└── frontend/                      # Angular 18 SPA
```

## GenAI tooling — disclosure

Generative AI was used as a coding assistant throughout this exercise. The deliberate workflow:

### Where AI helped

- Scaffolding ceremony — generating boilerplate for `.csproj` references, FluentValidation rule blocks, Swashbuckle bearer config, DbUp wiring, Angular service skeletons.
- Drafting JWT setup, exception-to-ProblemDetails mapping, and `WebApplicationFactory` Testcontainers fixture wiring.
- Translating between layers — given a use case, producing the matching DTO record and FluentValidation validator.

### What I had to correct or push back on

These are the recurring failure modes I caught in the AI output and fixed:

1. **SQL injection risk** — first repository draft concatenated `userId` into the SQL string. Forced it back to `NpgsqlCommand.Parameters.Add(..., NpgsqlDbType.Uuid)` everywhere. Every dynamic value in `BookRepository`/`UserRepository` is now a typed parameter.
2. **Ownership leak** — AI initially generated `BookRepository.FindAsync(Guid id)` without scoping by `user_id`. Anyone with a book GUID could read another user's data. Re-shaped the interface to require both `id` *and* `userId` on every query and command, then added an integration test that proves a user-B request for a user-A book returns `null`.
3. **Password equality vs verify** — first draft of `LoginUseCase` compared `request.Password == user.PasswordHash`. Replaced with `IPasswordHasher.Verify` which calls `BCrypt.Verify`. Added a unit test that exercises the wrong-password path.
4. **EF Core "helpfulness"** — even with an explicit ban in the prompt, the assistant suggested `DbContext` patterns more than once. Had to reinforce raw ADO.NET in the constraints and reject completions that pulled in `Microsoft.EntityFrameworkCore.*`.
5. **`Hydrate` skipping validation** — the original `Book.Hydrate` bypassed invariants for performance. A code-review pass flipped that decision: corrupt rows should fail at materialization, not at the next `Update`. Same change applied to `User.Hydrate`.
6. **DbUp variable substitution** — seed migration with the BCrypt hash `$2a$12$...` initially exploded because DbUp parses `$varname$` as a variable. Fix: `.WithVariablesDisabled()` on the builder.
7. **Microsoft.OpenApi 2.x namespace move** — Swashbuckle 10.x's old `OpenApiSecurityScheme { Reference = ... }` no longer exists; replaced with `OpenApiSecuritySchemeReference("Bearer")`. Caught by a build failure, not the AI.
8. **Test conventions** — generated tests sometimes used `Assert.True(...)` for booleans. Standardized on FluentAssertions and added the `Should().Throw<DomainException>()` pattern for invariant tests.

### What this demonstrates

AI accelerates the typing, not the thinking. Every security-sensitive boundary (auth, persistence, multi-tenant ownership) had a defect in the first AI-generated draft. The test suite — particularly the integration tests with a real Postgres — is what surfaced those defects. The workflow I'd recommend: **prompt for scaffolding, write the integration tests yourself, then let the test failures drive the correction loop.**
