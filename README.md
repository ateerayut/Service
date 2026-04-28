# Service Template Workspace

Template นี้เป็นตัวอย่างโครงสร้าง .NET Minimal API สำหรับเริ่มต้นทำ service ใหม่ โดยจัด project ตามแนวคิด Clean Architecture เพื่อให้โค้ดอ่านง่าย แยกหน้าที่ชัดเจน ทดสอบง่าย และลดการผูกติดกับ framework หรือ database มากเกินไป

เหมาะสำหรับ Jr.Dev หรือทีมที่ต้องการ template กลางก่อนเริ่ม feature จริง

## Concept หลัก

Clean Architecture แบ่งโค้ดเป็นชั้น ๆ โดยชั้นในสุดคือ business rule และชั้นนอกสุดคือ framework, database, API, หรือ tool ต่าง ๆ

Dependency ควรไหลจากข้างนอกเข้าข้างใน:

```text
Service.Api
    -> Service.Application
        -> Service.Domain

Service.Infrastructure
    -> Service.Application
    -> Service.Domain
```

กฎสำคัญคือ `Domain` ไม่ควรรู้จัก API, Database, EF Core, HTTP, หรือ framework ใด ๆ ส่วน `Application` รู้ว่าระบบต้องทำอะไร แต่ไม่ควรรู้ว่าข้อมูลถูกเก็บจริงที่ไหน

## Project Structure

```text
src/
  Service.Api/
  Service.Application/
  Service.Domain/
  Service.Infrastructure/

tests/
  Service.UnitTests/
  Service.IntegrationTests/
```

## Service.Api

หน้าที่ของ project นี้คือรับ request จาก client และส่ง response กลับไป

สิ่งที่ควรอยู่ใน `Service.Api`:

- Minimal API endpoints
- Request DTO เช่น `CreateProductRequest`
- Response DTO เช่น `ProductResponse`
- HTTP status mapping เช่น `200 OK`, `201 Created`, `400 ValidationProblem`, `404 NotFound`
- OpenAPI, Scalar, authentication, authorization, middleware
- Dependency injection bootstrap ใน `Program.cs`

สิ่งที่ไม่ควรอยู่ใน `Service.Api`:

- Business rule หลักของระบบ
- EF Core query โดยตรง
- SQL หรือ database logic
- การคำนวณ domain สำคัญ ๆ

ตัวอย่างใน template:

- [ProductEndpoints.cs](src/Service.Api/Features/Products/ProductEndpoints.cs)
- [CreateProductRequest.cs](src/Service.Api/Features/Products/CreateProductRequest.cs)
- [UpdateProductRequest.cs](src/Service.Api/Features/Products/UpdateProductRequest.cs)
- [ProductResponse.cs](src/Service.Api/Features/Products/ProductResponse.cs)

แนวคิดคือ endpoint ควรบางที่สุด ทำหน้าที่แปลง HTTP request เป็น command/use case call แล้วแปลงผลลัพธ์กลับเป็น HTTP response

### Common API Foundation

โค้ดพื้นฐานของ API อยู่ที่ `src/Service.Api/Common` และถูก register ใน `Program.cs`

ไฟล์หลัก:

- [GlobalExceptionHandlingMiddleware.cs](src/Service.Api/Common/GlobalExceptionHandlingMiddleware.cs)
- [CorrelationIdMiddleware.cs](src/Service.Api/Common/CorrelationIdMiddleware.cs)

#### GlobalExceptionHandlingMiddleware.cs

middleware นี้เป็นจุดกลางสำหรับจับ exception ที่หลุดจาก endpoint หรือ middleware ด้านใน แล้วแปลงเป็น response รูปแบบ `application/problem+json`

ประโยชน์:

- response error มีรูปแบบมาตรฐาน
- client อ่าน error ได้ง่ายขึ้น
- log exception ได้จากจุดเดียว
- response มี `traceId` และ `correlationId` สำหรับไล่ปัญหา

ตัวอย่าง response:

```json
{
  "type": "about:blank",
  "title": "An unexpected error occurred.",
  "status": 500,
  "instance": "/products",
  "traceId": "0HN...",
  "correlationId": "019..."
}
```

ใน Development จะใส่ `detail` เป็น exception เต็มเพื่อ debug ง่ายขึ้น แต่ใน environment อื่นจะไม่ส่ง stack trace กลับไปให้ client

#### CorrelationIdMiddleware.cs

middleware นี้ใช้จัดการ `X-Correlation-Id`

ถ้า client ส่ง header นี้มา:

```http
X-Correlation-Id: my-request-id
```

ระบบจะใช้ค่านั้นต่อ และส่งกลับใน response header

ถ้า client ไม่ส่งมา ระบบจะสร้าง UUID v7 ให้ใหม่ แล้วใส่ใน:

- `HttpContext.Items`
- response header `X-Correlation-Id`
- Serilog log context

ประโยชน์คือเวลา debug production สามารถเอา correlation id จาก response ไปค้น log ที่เกี่ยวข้องกับ request นั้นได้ทันที

#### Serilog

template ใช้ Serilog สำหรับ structured logging และตั้งค่าใน `appsettings.json`

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      }
    ],
    "Enrich": [
      "FromLogContext"
    ],
    "Properties": {
      "Application": "Service.Api"
    }
  }
}
```

ใน `Program.cs` มี `UseSerilogRequestLogging()` เพื่อ log request แบบ structured และ enrich `CorrelationId` เข้า diagnostic context

#### Health Checks

template มี health endpoints:

- `GET /health/live`
- `GET /health/ready`

`/health/live` ใช้บอกว่า process ยังทำงานอยู่ เหมาะกับ liveness probe

`/health/ready` ใช้บอกว่า service พร้อมรับ traffic หรือไม่ ตอนนี้เช็ค database ด้วย `AppDbContext.Database.CanConnectAsync(...)` ถ้า database ต่อไม่ได้จะตอบ `503 Service Unavailable`

แนวคิด:

- live = app ยังไม่ตาย
- ready = app พร้อมให้ระบบอื่นเรียกใช้งานจริง

### Auth Feature

โค้ด Auth อยู่ที่ `src/Service.Api/Features/Auth` เพราะเป็นตัวอย่าง authentication สำหรับ API layer โดยตรง หน้าที่หลักคือออก JWT, refresh token, และ revoke refresh token เพื่อให้ลองใช้งาน protected endpoint ได้ทันที

ไฟล์ใน Auth feature:

- [AuthEndpoints.cs](src/Service.Api/Features/Auth/AuthEndpoints.cs)
- [TokenRequest.cs](src/Service.Api/Features/Auth/TokenRequest.cs)
- [TokenResponse.cs](src/Service.Api/Features/Auth/TokenResponse.cs)
- [RefreshTokenRequest.cs](src/Service.Api/Features/Auth/RefreshTokenRequest.cs)
- [RevokeTokenRequest.cs](src/Service.Api/Features/Auth/RevokeTokenRequest.cs)
- [JwtOptions.cs](src/Service.Api/Features/Auth/JwtOptions.cs)
- [TokenUserOptions.cs](src/Service.Api/Features/Auth/TokenUserOptions.cs)
- [TokenService.cs](src/Service.Api/Features/Auth/TokenService.cs)
- [RefreshTokenStore.cs](src/Service.Api/Features/Auth/RefreshTokenStore.cs)

#### AuthEndpoints.cs

เป็นตัวกำหนด route ของ authentication ทั้งหมด

- `POST /auth/token` ใช้ username/password เพื่อออก access token และ refresh token
- `POST /auth/refresh` ใช้ refresh token เพื่อออก token ชุดใหม่
- `POST /auth/revoke` ใช้ revoke refresh token ที่ไม่ต้องการให้ใช้ต่อ

ไฟล์นี้ควรทำหน้าที่บาง ๆ เหมือน endpoint อื่น คือรับ request, เรียก service ที่เกี่ยวข้อง, แล้วแปลงผลลัพธ์เป็น HTTP response

#### TokenRequest.cs

เป็น request body สำหรับ `POST /auth/token`

```json
{
  "username": "demo",
  "password": "demo"
}
```

ใน template ตั้ง default value เป็น `demo/demo` เพื่อให้ Scalar แสดงตัวอย่าง request ที่ใช้งานได้ทันที เหมาะกับการทดลอง local development เท่านั้น

#### TokenResponse.cs

เป็น response body หลัง login หรือ refresh token สำเร็จ

```json
{
  "accessToken": "<jwt>",
  "refreshToken": "<refresh-token>",
  "tokenType": "Bearer",
  "expiresAt": "2026-04-28T12:00:00+00:00",
  "refreshTokenExpiresAt": "2026-05-05T12:00:00+00:00"
}
```

ความหมายของแต่ละ field:

- `accessToken` คือ JWT ที่ใช้เรียก protected API
- `refreshToken` คือ token ที่ใช้ขอ access token ชุดใหม่เมื่อ access token หมดอายุ
- `tokenType` คือชนิด token ปัจจุบันเป็น `Bearer`
- `expiresAt` คือเวลาหมดอายุของ access token
- `refreshTokenExpiresAt` คือเวลาหมดอายุของ refresh token

#### RefreshTokenRequest.cs

เป็น request body สำหรับ `POST /auth/refresh`

```json
{
  "refreshToken": "<refresh-token>"
}
```

เมื่อ refresh สำเร็จ ระบบจะออก access token และ refresh token ชุดใหม่ให้ โดย refresh token ตัวเก่าจะถูกใช้แล้วทิ้ง

#### RevokeTokenRequest.cs

เป็น request body สำหรับ `POST /auth/revoke`

```json
{
  "refreshToken": "<refresh-token>"
}
```

ใช้ในกรณี logout หรือ client ต้องการยกเลิก refresh token ก่อนหมดอายุ endpoint นี้ต้องส่ง access token ด้วย เพราะถือว่าเป็น operation ของผู้ใช้ที่ login แล้ว

#### JwtOptions.cs

เป็น class สำหรับ bind ค่า config section `Jwt`

```json
{
  "Jwt": {
    "Issuer": "service-template",
    "Audience": "service-template-clients",
    "SigningKey": "CHANGE_ME_TO_A_SECRET_KEY_WITH_AT_LEAST_32_CHARS",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

ใช้ควบคุมการออกและ validate JWT:

- `Issuer` คือผู้ออก token
- `Audience` คือผู้รับ token
- `SigningKey` คือ secret key ที่ใช้ sign token
- `ExpirationMinutes` คืออายุของ access token
- `RefreshTokenExpirationDays` คืออายุของ refresh token

ค่าที่เป็น secret เช่น `SigningKey` ควรตั้งผ่าน user secrets, environment variables, หรือ secret store ใน environment จริง

#### TokenUserOptions.cs

เป็น class สำหรับ bind ค่า config section `TokenUser`

```json
{
  "TokenUser": {
    "Username": "demo",
    "Password": "demo"
  }
}
```

ใน template ใช้เป็น demo user เพื่อให้ทดลอง JWT ได้ทันทีโดยยังไม่ต้องสร้างระบบ user จริง เมื่อใช้ production ควรแทนด้วย user store หรือ identity provider เช่น database user table, ASP.NET Core Identity, Keycloak, Azure AD, Auth0 หรือระบบ auth กลางขององค์กร

#### TokenService.cs

เป็น service สำหรับสร้าง token pair

หน้าที่หลัก:

- สร้าง JWT access token
- ใส่ claims เช่น `sub`, `unique_name`, `jti`
- ใช้ UUID v7 เป็น `jti`
- สร้าง refresh token แบบ random bytes
- กำหนดเวลาหมดอายุของ access token และ refresh token

เหตุผลที่แยกออกจาก endpoint คือ endpoint จะได้ไม่เต็มไปด้วยรายละเอียดการสร้าง JWT และถ้าต้องเปลี่ยน claim, signing algorithm, หรือ token lifetime จะเปลี่ยนที่ service นี้เป็นหลัก

#### RefreshTokenStore.cs

เป็น abstraction และ implementation สำหรับเก็บ refresh token

มี interface:

```csharp
public interface IRefreshTokenStore
{
    void Store(string refreshToken, string username, DateTimeOffset expiresAt);
    bool TryConsume(string refreshToken, out string username);
    bool Revoke(string refreshToken);
}
```

ใน template มี `InMemoryRefreshTokenStore` เพื่อให้ลอง flow ได้ง่าย:

- `Store` บันทึก refresh token
- `TryConsume` ตรวจ token และ mark ว่าใช้แล้ว
- `Revoke` mark token ว่าถูกยกเลิก

ข้อจำกัดของ in-memory store:

- restart service แล้ว token หาย
- scale หลาย instance แล้ว token ไม่แชร์กัน
- ไม่เหมาะกับ production

ถ้าใช้จริงควรเปลี่ยน implementation ไปใช้ database หรือ Redis และควร hash refresh token ก่อนเก็บ เพื่อป้องกัน token รั่วจาก storage

## Service.Application

หน้าที่ของ project นี้คืออธิบายว่า application ทำ use case อะไรได้บ้าง เช่น create product, update product, delete product

สิ่งที่ควรอยู่ใน `Service.Application`:

- Use cases เช่น `CreateProductUseCase`
- Commands เช่น `CreateProductCommand`
- Query objects เช่น `ListProductsQuery`
- Validators เช่น `CreateProductValidator`
- Repository interfaces เช่น `IProductRepository`
- DTO ที่ใช้ส่งข้อมูลระหว่าง Application กับชั้นอื่น เช่น `ProductDto`
- Application-level result เช่น `OperationResult<T>`
- Shared result เช่น `PagedResult<T>`

สิ่งที่ไม่ควรอยู่ใน `Service.Application`:

- EF Core `DbContext`
- SQL command
- HTTP request/response type
- Minimal API result เช่น `IResult`
- การอ่านค่า config ของ infrastructure โดยตรง

ตัวอย่างใน template:

- [CreateProductUseCase.cs](src/Service.Application/Products/CreateProductUseCase.cs)
- [UpdateProductUseCase.cs](src/Service.Application/Products/UpdateProductUseCase.cs)
- [DeleteProductUseCase.cs](src/Service.Application/Products/DeleteProductUseCase.cs)
- [ListProductsQuery.cs](src/Service.Application/Products/ListProductsQuery.cs)
- [PagedResult.cs](src/Service.Application/Common/PagedResult.cs)
- [IProductRepository.cs](src/Service.Application/Products/IProductRepository.cs)

เหตุผลที่ `IProductRepository` อยู่ใน Application เพราะ use case ต้องบอกว่า “ฉันต้องการเก็บ/อ่าน Product” แต่ไม่ควรรู้ว่าใช้ PostgreSQL, SQL Server, MongoDB, หรือ external API

นี่คือหลัก Dependency Inversion: ชั้น Application พึ่งพา abstraction ส่วน Infrastructure เป็นคน implement abstraction นั้น

### Query Object และ Pagination

template มีตัวอย่าง query object สำหรับ list endpoint:

```csharp
public record ListProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null);
```

เหตุผลที่ใช้ query object แทนการส่ง parameter แยก ๆ ไปทุกชั้น:

- อ่านง่ายว่า list endpoint รับ filter อะไรบ้าง
- เพิ่ม filter ใหม่ได้โดยไม่ต้องแก้ method signature หลายตัว
- validator ทำงานกับ object เดียวได้
- repository รับ query เดียวแล้วแปลงเป็น EF query ได้ชัดเจน

`ListProductsValidator` ใช้คุม rule ของ query:

- `Page` ต้องมากกว่าหรือเท่ากับ 1
- `PageSize` ต้องอยู่ระหว่าง 1 ถึง 100
- `Search` ยาวไม่เกิน 200 ตัวอักษร

ผลลัพธ์ส่งกลับด้วย `PagedResult<T>`:

```csharp
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems)
```

และมี metadata เพิ่ม:

- `TotalPages`
- `HasPreviousPage`
- `HasNextPage`

ตัวอย่าง API:

```text
GET /products?page=1&pageSize=20&search=keyboard
```

ตัวอย่าง response:

```json
{
  "items": [
    {
      "id": "019...",
      "name": "Keyboard",
      "price": 1200
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

pattern นี้เหมาะกับ list/search endpoint ทั่วไป เช่น Customers, Orders, Users โดยยังไม่ต้องเพิ่ม generic repository ที่ซับซ้อนเกินไป

## Service.Domain

หน้าที่ของ project นี้คือเก็บ business rule ที่เป็นหัวใจของระบบ

สิ่งที่ควรอยู่ใน `Service.Domain`:

- Entities เช่น `Product`
- Value Objects
- Domain rules
- Domain methods เช่น `Product.Create(...)`, `Product.Update(...)`
- Validation ที่เป็นกฎแท้ของ domain และต้องถูกต้องเสมอ

สิ่งที่ไม่ควรอยู่ใน `Service.Domain`:

- EF Core attributes ที่ผูกกับ database มากเกินไป
- HTTP, API, Controller, Minimal API
- Logging, configuration, dependency injection
- External service calls

ตัวอย่างใน template:

- [Product.cs](src/Service.Domain/Products/Product.cs)
- [Customer.cs](src/Service.Domain/Customers/Customer.cs)
- [Order.cs](src/Service.Domain/Orders/Order.cs)
- [OrderItem.cs](src/Service.Domain/Orders/OrderItem.cs)

`Product` เป็นคนดูแล rule ของตัวเอง เช่น name ห้ามว่าง และ price ต้องมากกว่า 0 ต่อให้ Application validator หลุดไป Domain ก็ยังป้องกันข้อมูลผิดพลาดอยู่

ตัวอย่าง relation ใน domain:

- `Customer` เก็บข้อมูลลูกค้า เช่น `Id`, `Name`, `CreateDate`
- `Order` เก็บหัวบิลว่า order นี้ขายให้ `Customer` คนไหน
- `OrderItem` เก็บรายการสินค้าที่ขายใน order นั้น โดยอ้างถึง `Product`

โครงสร้างนี้ทำให้หนึ่ง order มีหลาย product ได้:

```text
Customer 1 -> many Orders
Order 1 -> many OrderItems
OrderItem many -> 1 Product
```

ใช้ `OrderItem` แทนการให้ `Order` ชี้หา `Product` ตรง ๆ เพราะในการขายจริงมักต้องเก็บข้อมูลต่อรายการ เช่น `Quantity`, `UnitPrice`, และ `TotalPrice`

## Service.Infrastructure

หน้าที่ของ project นี้คือเชื่อมต่อกับของจริงภายนอกระบบ เช่น database, file storage, message broker, email provider หรือ external API

สิ่งที่ควรอยู่ใน `Service.Infrastructure`:

- EF Core `DbContext`
- Repository implementation
- Database migrations
- External provider implementation
- Infrastructure dependency injection

สิ่งที่ไม่ควรอยู่ใน `Service.Infrastructure`:

- Business rule หลัก
- HTTP endpoint
- Request/response DTO ของ API

ตัวอย่างใน template:

- [AppDbContext.cs](src/Service.Infrastructure/Persistence/AppDbContext.cs)
- [ProductRepository.cs](src/Service.Infrastructure/Persistence/ProductRepository.cs)
- [DependencyInjection.cs](src/Service.Infrastructure/DependencyInjection.cs)

`ProductRepository` implement `IProductRepository` ที่อยู่ใน Application ทำให้ use case ไม่ต้องรู้ว่าเบื้องหลังใช้ EF Core และ PostgreSQL

## Database Migrations

Template นี้ใช้ Entity Framework Core Migrations เพื่อจัดการการเปลี่ยนแปลงโครงสร้าง database อย่างเป็นระบบ โดยทำให้ schema ของ database และ domain model อยู่ในเวอร์ชันเดียวกัน

### Architecture Pattern

Flow ของ data model:

```text
Service.Domain (Entities)
    ↓
Service.Infrastructure (AppDbContext, Migrations)
    ↓
Database (PostgreSQL, SQL Server, etc.)
```

เมื่อเพิ่มหรือแก้ entity ใน Domain ต้องสร้าง migration ใหม่เพื่อ sync กับ database

### Current Data Model

Template มี entities ต่อไปนี้:

#### Product

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary Key (UUID v7) |
| Name | string | Required, max 200 characters |
| Price | decimal | Required, must be > 0 |

#### Customer

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary Key (UUID v7) |
| Name | string | Required, max 200 characters |
| CreateDate | DateTimeOffset | Auto-set to UTC now |

#### Order

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary Key (UUID v7) |
| CustomerId | Guid | Foreign Key to Customer |
| CreateDate | DateTimeOffset | Auto-set to UTC now |
| Items | IReadOnlyCollection<OrderItem> | Navigation property |

#### OrderItem

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary Key (UUID v7) |
| OrderId | Guid | Foreign Key to Order (OnDelete: Cascade) |
| ProductId | Guid | Foreign Key to Product (OnDelete: Restrict) |
| Quantity | int | Required, must be > 0 |
| UnitPrice | decimal | Required, must be > 0 |
| TotalPrice | decimal | Computed (Quantity × UnitPrice), not persisted |

### How to Create a Migration

#### Step 1: Modify Domain Entity

Edit the entity in `Service.Domain`. For example, add Email to Customer:

```csharp
// src/Service.Domain/Customers/Customer.cs
namespace Service.Domain.Customers;

public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;  // ← New property
    public DateTimeOffset CreateDate { get; private set; } = DateTimeOffset.UtcNow;

    private Customer() { }

    public static Customer Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        return new Customer
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Email = email,
            CreateDate = DateTimeOffset.UtcNow
        };
    }
}
```

#### Step 2: Update Entity Configuration in AppDbContext

Edit `Service.Infrastructure/Persistence/AppDbContext.cs` in `OnModelCreating()`:

```csharp
modelBuilder.Entity<Customer>(entity =>
{
    entity.HasKey(customer => customer.Id);

    entity.Property(customer => customer.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(customer => customer.Email)  // ← Add this
        .IsRequired()
        .HasMaxLength(255);

    entity.Property(customer => customer.CreateDate)
        .IsRequired()
        .HasDefaultValueSql("CURRENT_TIMESTAMP");
});
```

#### Step 3: Create Migration

Open PowerShell and run:

```powershell
cd D:\MinimalTemplate\service-template-workspace

dotnet ef migrations add AddEmailToCustomer `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj `
    --output-dir Migrations
```

Output:
```
Done. To undo this action, use 'dotnet ef migrations remove'
```

This generates a new migration file like `20260505120000_AddEmailToCustomer.cs`

#### Step 4: Review Migration

Open the generated migration file and review the SQL:

```csharp
public partial class AddEmailToCustomer : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Email",
            table: "Customers",
            type: "character varying(255)",
            maxLength: 255,
            nullable: false,
            defaultValue: ""); // Review this default value
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Email",
            table: "Customers");
    }
}
```

#### Step 5: Apply Migration to Database

```powershell
dotnet ef database update `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

Output:
```
Build started...
Build succeeded.
Applying migration '20260505120000_AddEmailToCustomer'...
Done.
```

#### Step 6: Verify Changes

Check the database schema was updated:

```sql
-- For PostgreSQL
\d customers

-- For SQL Server
sp_help Customers
```

### Sample Migration Scenarios

#### Scenario 1: Add a New Column with Default Value

```powershell
# 1. Edit Product entity
# Add: public int StockQuantity { get; private set; }

# 2. Update DbContext
# entity.Property(p => p.StockQuantity).IsRequired().HasDefaultValue(0);

# 3. Create migration
dotnet ef migrations add AddStockQuantityToProduct `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

# 4. Apply migration
dotnet ef database update `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

#### Scenario 2: Create a New Entity and Table

Example: Add Category entity

```csharp
// 1. Create entity: src/Service.Domain/Categories/Category.cs
namespace Service.Domain.Categories;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private Category() { }

    public static Category Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new Category
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = description
        };
    }
}

// 2. Add to DbContext: src/Service.Infrastructure/Persistence/AppDbContext.cs
public DbSet<Category> Categories => Set<Category>();

// 3. Add configuration in OnModelCreating()
modelBuilder.Entity<Category>(entity =>
{
    entity.HasKey(c => c.Id);

    entity.Property(c => c.Name)
        .IsRequired()
        .HasMaxLength(100);

    entity.Property(c => c.Description)
        .HasMaxLength(500);
});

// 4. Create migration
dotnet ef migrations add AddCategoryTable `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

// 5. Apply migration
dotnet ef database update `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

#### Scenario 3: Add Foreign Key Relationship

Example: Link Product to Category

```csharp
// 1. Modify Product entity
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public Guid? CategoryId { get; private set; }  // ← New FK property
    public Category? Category { get; private set; } // ← Navigation
}

// 2. Update DbContext configuration
modelBuilder.Entity<Product>(entity =>
{
    // ... existing config

    entity.HasOne(p => p.Category)
        .WithMany()
        .HasForeignKey(p => p.CategoryId)
        .OnDelete(DeleteBehavior.SetNull);
});

// 3. Create and apply migration
dotnet ef migrations add AddCategoryForeignKeyToProduct `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

dotnet ef database update `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

#### Scenario 4: Make a Column Optional or Change Type

```csharp
// 1. Modify entity - change required property to optional
public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string? Notes { get; private set; }  // ← Made optional (nullable)
    public DateTimeOffset CreateDate { get; private set; }
}

// 2. Update DbContext
modelBuilder.Entity<Order>(entity =>
{
    entity.Property(o => o.Notes)
        .IsRequired(false)  // ← Make optional
        .HasMaxLength(1000);
});

// 3. Create and apply migration
dotnet ef migrations add MakeOrderNotesOptional `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

dotnet ef database update `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

### Useful Migration Commands

**List all migrations with their status:**

```powershell
dotnet ef migrations list `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

Output:
```
20260326133244_Init
20260428073737_AddCustomersAndOrders (Pending)
```

**Remove the last unapplied migration:**

```powershell
dotnet ef migrations remove `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

**Rollback database to a specific migration:**

```powershell
# Rollback to InitialCreate migration
dotnet ef database update 20260326133244_Init `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

**Generate SQL script without executing:**

```powershell
# Generate SQL for all pending migrations
dotnet ef migrations script `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

**Generate SQL script from specific migration range:**

```powershell
# Generate SQL from Init to AddCustomersAndOrders
dotnet ef migrations script 20260326133244_Init 20260428073737_AddCustomersAndOrders `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

**Check database status:**

```powershell
# Show which migrations are applied
dotnet ef migrations list `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

### Production Migration Best Practices

⚠️ **Before deploying to production:**

1. **Always review migrations** in your code review process
2. **Test migrations** on a staging/development database first
3. **Backup database** before applying migrations to production
4. **Use SQL scripts** instead of `dotnet ef database update` for production deployments:
   ```powershell
   dotnet ef migrations script > migration.sql
   # Review migration.sql
   # Execute on production database using your database admin tool
   ```
5. **Keep migrations immutable** - never edit applied migrations
6. **Verify rollback plan** - understand the `Down()` method for each migration

⚠️ **Breaking changes to avoid:**

- `DropColumn` causes permanent data loss
- `DropTable` loses all data
- When adding required columns to existing tables, provide a `defaultValue`:
  ```csharp
  entity.Property(p => p.Email)
      .IsRequired()
      .HasDefaultValue("no-email@example.com");
  ```

⚠️ **Team development tips:**

1. Always `git pull` latest migrations before creating new ones
2. Name migrations descriptively:
   - ✅ `AddEmailToCustomer`
   - ❌ `Update`
3. Commit migrations to source control with your code changes
4. Communicate pending migrations with team members
5. If conflicts occur in migration names, coordinate with team to resolve

### Complete Migration Workflow Example

```powershell
# 1. Pull latest from repository
git pull origin master

# 2. Restore packages
dotnet restore

# 3. Build to ensure no compilation errors
dotnet build .\Service.slnx

# 4. Modify your domain entity
# Edit src/Service.Domain/Products/Product.cs
# Add: public string Sku { get; private set; } = string.Empty;

# 5. Update DbContext configuration
# Edit src/Service.Infrastructure/Persistence/AppDbContext.cs
# Add: entity.Property(p => p.Sku).IsRequired().HasMaxLength(50);

# 6. Create migration
dotnet ef migrations add AddSkuToProduct `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

# 7. Review the generated migration
# Open src/Service.Infrastructure/Migrations/<timestamp>_AddSkuToProduct.cs
# Verify the SQL is correct

# 8. Apply migration to local database
dotnet ef database update `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

# 9. Run tests to ensure nothing broke
dotnet test .\Service.slnx

# 10. Commit and push
git add .
git commit -m "feat: add SKU to Product"
git push origin <your-branch>

# 11. Create pull request and wait for code review
```

## Tests

Template แยก test เป็น 2 project เพื่อให้เข้าใจเจตนาของ test ชัดเจน

### Service.UnitTests

ใช้ทดสอบ logic ขนาดเล็กที่ไม่ต้องพึ่ง database หรือ network

ตัวอย่าง test:

- Domain rules ใน `ProductTests`
- Validator rules ใน `ProductValidatorTests`
- Use case behavior ใน `ProductUseCaseTests`

Unit test ควรรันเร็ว ไม่ต้องต่อ database จริง และควรบอกได้ทันทีว่า business logic พังตรงไหน

### Service.IntegrationTests

ใช้ทดสอบการประกอบกันของหลายส่วน เช่น dependency injection wiring หรือ integration กับ infrastructure

ใน template ตอนนี้มี test ตรวจว่า:

- `AddApplication()` register use cases ได้
- `AddInfrastructure()` register repository ได้

ถ้าต้องการทดสอบ database จริงในอนาคต แนะนำให้ใช้ test database หรือ container แยก ไม่ควรใช้ database production หรือ shared database

## Sample CRUD Flow

ตัวอย่าง Products CRUD มี endpoints:

- `GET /products`
- `GET /products?page=1&pageSize=20&search=keyboard`
- `GET /products/{id}`
- `POST /products`
- `PUT /products/{id}`
- `DELETE /products/{id}`

ทุก endpoint ในกลุ่ม `/products` ต้องส่ง JWT แบบ Bearer token:

```http
Authorization: Bearer <access_token>
```

ขอ token ได้จาก:

```text
POST /auth/token
```

ตัวอย่าง token request:

```json
{
  "username": "demo",
  "password": "demo"
}
```

ตัวอย่าง token response:

```json
{
  "accessToken": "<jwt>",
  "refreshToken": "<refresh-token>",
  "tokenType": "Bearer",
  "expiresAt": "2026-04-28T12:00:00+00:00",
  "refreshTokenExpiresAt": "2026-05-05T12:00:00+00:00"
}
```

เมื่อ access token หมดอายุ สามารถขอ token ชุดใหม่ด้วย refresh token:

```text
POST /auth/refresh
```

```json
{
  "refreshToken": "<refresh-token>"
}
```

ระบบจะ rotate refresh token ทุกครั้งที่ refresh สำเร็จ แปลว่า refresh token เก่าจะใช้ซ้ำไม่ได้ ให้ client เก็บ refresh token ตัวใหม่จาก response ล่าสุดเสมอ

ถ้าต้องการ revoke refresh token:

```text
POST /auth/revoke
```

endpoint นี้ต้องส่ง access token ด้วย:

```http
Authorization: Bearer <access_token>
```

```json
{
  "refreshToken": "<refresh-token>"
}
```

หมายเหตุ: refresh token store ใน template นี้เป็น in-memory เพื่อให้ลอง flow ได้ง่าย เมื่อ restart service token ที่เคยออกไว้จะหายทั้งหมด ถ้าใช้จริงควรเปลี่ยนไปเก็บใน database หรือ Redis และควร hash refresh token ก่อนบันทึก

ตัวอย่าง create request:

```json
{
  "name": "Sample product",
  "price": 199.00
}
```

Flow ของ `POST /products`:

```text
Client
  -> Service.Api: POST /auth/token
  -> Service.Api: JWT validation
  -> Service.Api: ProductEndpoints
  -> Service.Application: CreateProductUseCase
  -> Service.Application: CreateProductValidator
  -> Service.Domain: Product.Create
  -> Service.Application: IProductRepository
  -> Service.Infrastructure: ProductRepository
  -> Database
```

จุดสำคัญคือ API ไม่เรียก database เอง และ Infrastructure ไม่เป็นคนตัดสิน business rule

## SOLID Principles ใน Template นี้

### S: Single Responsibility Principle

แต่ละ class มีหน้าที่เดียวชัดเจน

- `ProductEndpoints` ดูแล HTTP route
- `CreateProductUseCase` ดูแลการสร้าง product
- `CreateProductValidator` ดูแล validation ของ create command
- `Product` ดูแล rule ของ entity
- `ProductRepository` ดูแล persistence

ถ้า class เริ่มทำหลายหน้าที่เกินไป เช่น endpoint เริ่ม query database เอง หรือ repository เริ่ม validate business rule แปลว่าควรแยกความรับผิดชอบใหม่

### O: Open/Closed Principle

ระบบควรเปิดให้เพิ่ม behavior ใหม่โดยแก้ของเดิมให้น้อยที่สุด

เช่น ถ้าต้องเพิ่ม feature `Orders` ให้เพิ่ม folder ใหม่:

```text
Service.Api/Features/Orders
Service.Application/Orders
Service.Domain/Orders
Service.Infrastructure/Persistence/OrderRepository.cs
```

ไม่ควรยัดทุกอย่างลงใน Products เดิมหรือแก้ class เดิมจนใหญ่ขึ้นเรื่อย ๆ

### L: Liskov Substitution Principle

ถ้า code ใช้ interface เช่น `IProductRepository` implementation ใด ๆ ควรใช้แทนกันได้โดยไม่ทำให้ use case พัง

ตัวอย่าง:

- `ProductRepository` ใช้ EF Core จริง
- `FakeProductRepository` ใช้ใน unit test

ทั้งคู่ต้องทำตาม contract เดียวกัน

### I: Interface Segregation Principle

Interface ควรมี method เท่าที่ client ต้องใช้ ไม่ควรเป็น interface ใหญ่เกินจำเป็น

ใน template นี้ `IProductRepository` มีเฉพาะ operation ที่ Products use cases ใช้ ถ้าอนาคตมี read model ซับซ้อน หรือ query เฉพาะทาง อาจแยก interface เพิ่มแทนการยัดทุกอย่างไว้ที่เดียว

### D: Dependency Inversion Principle

ชั้นสูงไม่ควรพึ่งพาชั้นต่ำโดยตรง

`CreateProductUseCase` พึ่งพา `IProductRepository` ไม่ใช่ `ProductRepository`

```text
Application -> IProductRepository
Infrastructure -> ProductRepository : IProductRepository
```

ผลคือ Application test ได้ง่าย และเปลี่ยน database implementation ได้โดยไม่กระทบ use case

## วิธีเพิ่ม Feature ใหม่

ตัวอย่างถ้าต้องเพิ่ม `Customers`

1. เพิ่ม entity ใน `Service.Domain/Customers`
2. เพิ่ม repository interface ใน `Service.Application/Customers`
3. เพิ่ม command, validator, use cases ใน `Service.Application/Customers`
4. เพิ่ม endpoint และ request/response DTO ใน `Service.Api/Features/Customers`
5. เพิ่ม repository implementation ใน `Service.Infrastructure`
6. เพิ่ม `DbSet<Customer>` ใน `AppDbContext`
7. เพิ่ม migration
8. เพิ่ม unit tests และ integration tests

แนะนำให้ทำทีละ use case เช่นเริ่มจาก Create ก่อน แล้วค่อย List/Get/Update/Delete จะควบคุม scope ง่ายกว่า

## Configuration

ห้าม commit secret จริงลง source control เช่น database password, API key, JWT signing key

ค่า connection string ใน `appsettings.json` เป็น placeholder เท่านั้น:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=ServiceDb;Username=postgres;Password=CHANGE_ME"
  }
}
```

สำหรับ local development ให้ใช้ user secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=ServiceDb;Username=postgres;Password=your-password" --project .\src\Service.Api
```

JWT bearer authentication ใช้ config จาก section `Jwt`

```json
{
  "Jwt": {
    "Issuer": "service-template",
    "Audience": "service-template-clients",
    "SigningKey": "CHANGE_ME_TO_A_SECRET_KEY_WITH_AT_LEAST_32_CHARS",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

สำหรับ local development ให้เปลี่ยน signing key ผ่าน user secrets:

```powershell
dotnet user-secrets set "Jwt:SigningKey" "your-local-secret-key-with-at-least-32-characters" --project .\src\Service.Api
```

`/auth/token` ใน template ใช้ demo user จาก config เพื่อให้ลองระบบ JWT ได้ทันที:

```json
{
  "TokenUser": {
    "Username": "demo",
    "Password": "demo"
  }
}
```

สำหรับ local development ให้เปลี่ยน password ผ่าน user secrets:

```powershell
dotnet user-secrets set "TokenUser:Password" "your-local-demo-password" --project .\src\Service.Api
```

ใน production ไม่ควรใช้ `TokenUser` แบบนี้ ให้เปลี่ยนเป็น identity provider, user store, หรือ authentication service จริง เช่น Keycloak, Azure AD, Auth0, หรือระบบ user ภายในขององค์กร

## Useful Commands

Restore packages:

```powershell
dotnet restore
```

Build solution:

```powershell
dotnet build .\Service.slnx
```

Run tests:

```powershell
dotnet test .\Service.slnx
```

Run API:

```powershell
dotnet run --project .\src\Service.Api
```

OpenAPI และ Scalar:

- `/openapi/v1.json`
- `/scalar/v1`

## Team Guidelines

- Keep endpoint thin.
- Put use case logic in Application.
- Put business rules in Domain.
- Put database/external service code in Infrastructure.
- Test domain and use cases with unit tests.
- Do not commit real secrets.
- Prefer small classes with clear names.
- Add a new feature by following the same folder pattern as Products.

ถ้าสงสัยว่า code ควรอยู่ project ไหน ให้ถามว่า code นั้นเกี่ยวกับอะไร:

- เกี่ยวกับ HTTP หรือ response status? ไปที่ `Service.Api`
- เกี่ยวกับ use case ของระบบ? ไปที่ `Service.Application`
- เกี่ยวกับ business rule แท้ ๆ? ไปที่ `Service.Domain`
- เกี่ยวกับ database หรือ external system? ไปที่ `Service.Infrastructure`
