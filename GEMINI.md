# 🚀 Project Instructions: Service Template

This file contains foundational instructions and workflows for the Service Template project. Adhere to these standards to maintain architectural integrity and consistency.

## 🏗️ Architecture: Clean Architecture

We follow a strict Clean Architecture pattern. Dependencies MUST always point inward.

1.  **Domain (Service.Domain)**: Pure business logic and entities. No external dependencies.
2.  **Application (Service.Application)**: Use Cases, Commands/Queries, Validators, and Repository Interfaces.
3.  **Infrastructure (Service.Infrastructure)**: EF Core, Repository Implementations, and external integrations.
4.  **API (Service.Api)**: Minimal API Endpoints, Request/Response DTOs, and Middleware.

## 📁 Naming & File Conventions

Always organize new features by domain entity (e.g., `Features/Products` in API).

| Type | Pattern | Location |
| :--- | :--- | :--- |
| **Endpoint** | `Map{Entity}Endpoints` | `Service.Api/Features/{Entity}/` |
| **Request DTO** | `{Action}{Entity}Request` | `Service.Api/Features/{Entity}/` |
| **Response DTO** | `{Entity}Response` | `Service.Api/Features/{Entity}/` |
| **Use Case** | `{Action}{Entity}UseCase` | `Service.Application/{Entity}/` |
| **Command/Query**| `{Action}{Entity}Command/Query` | `Service.Application/{Entity}/` |
| **Validator** | `{Action}{Entity}CommandValidator` | `Service.Application/{Entity}/` |
| **Repository** | `I{Entity}Repository` | `Service.Application/{Entity}/` |
| **Domain Entity**| `{Entity}` | `Service.Domain/{Entity}/` |

## 🛠️ Implementation Workflow

When adding a new feature:

1.  **Domain**: Create/Update the Domain Entity in `Service.Domain`.
2.  **Application**:
    - Define `I{Entity}Repository` if new.
    - Create `Command/Query` and its `Validator`.
    - Implement `UseCase`.
    - Register in `DependencyInjection.cs`.
3.  **Infrastructure**:
    - Implement the repository.
    - Update `AppDbContext` and add a Migration if schema changed.
4.  **API**:
    - Create Request/Response DTOs.
    - Add/Update `Endpoints.cs`.
    - Map endpoints in `Program.cs`.

## 🧪 Testing Standards

Every feature MUST have accompanying tests.

-   **Unit Tests (`Service.UnitTests`)**:
    -   Use **Fake** repositories (not Mocks) for UseCase tests.
    -   Follow **AAA** (Arrange-Act-Assert) pattern.
    -   Naming: `[MethodName]_[Scenario]_[ExpectedResult]`.
    -   Test Validators, UseCases, and Domain rules.
-   **Integration Tests (`Service.IntegrationTests`)**:
    -   Verify DI registration.
    -   Test authentication flows and critical infrastructure paths.

## 🚨 Guidelines & Constraints

-   **Validation**: Always use `FluentValidation` in the Application layer.
-   **Errors**: Use `OperationResult` or `ProblemDetails` for error handling. Do not throw business exceptions for flow control.
-   **Async**: All I/O operations (DB, API) MUST use `async/await` and accept `CancellationToken`.
-   **Minimal API**: Keep endpoints thin; delegate logic to UseCases.
-   **Migrations**: Use `dotnet ef migrations add [Name]` and verify the generated code.

## 📖 Reference Documentation
- Architecture Details: `docs/architecture.md`
- Testing Guide: `docs/testing.md`
