# service-template-workspace

Minimal .NET service template with a sample Products CRUD feature.

## Structure

- `src/Service.Api` - Minimal API endpoints and request/response DTOs.
- `src/Service.Application` - Use cases, validation, and repository contracts.
- `src/Service.Domain` - Domain entities and business rules.
- `src/Service.Infrastructure` - EF Core DbContext and repository implementations.
- `tests` - Unit and integration test projects.

## Sample CRUD

The template includes a Products feature:

- `GET /products`
- `GET /products/{id}`
- `POST /products`
- `PUT /products/{id}`
- `DELETE /products/{id}`

Example create request:

```json
{
  "name": "Sample product",
  "price": 199.00
}
```

OpenAPI and Scalar are available when the app is running:

- `/openapi/v1.json`
- `/scalar/v1`

## Configuration

Set the database connection string outside source control for real projects:

```powershell
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=ServiceDb;Username=postgres;Password=your-password" --project .\src\Service.Api
```

JWT bearer authentication is enabled only when both values are configured:

```powershell
dotnet user-secrets set "Authentication:Authority" "https://issuer.example.com" --project .\src\Service.Api
dotnet user-secrets set "Authentication:Audience" "service-api" --project .\src\Service.Api
```

## Useful commands

```powershell
dotnet restore
dotnet test .\Service.slnx
dotnet run --project .\src\Service.Api
```
