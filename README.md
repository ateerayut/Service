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

## Service.Application

หน้าที่ของ project นี้คืออธิบายว่า application ทำ use case อะไรได้บ้าง เช่น create product, update product, delete product

สิ่งที่ควรอยู่ใน `Service.Application`:

- Use cases เช่น `CreateProductUseCase`
- Commands เช่น `CreateProductCommand`
- Validators เช่น `CreateProductValidator`
- Repository interfaces เช่น `IProductRepository`
- DTO ที่ใช้ส่งข้อมูลระหว่าง Application กับชั้นอื่น เช่น `ProductDto`
- Application-level result เช่น `OperationResult<T>`

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
- [IProductRepository.cs](src/Service.Application/Products/IProductRepository.cs)

เหตุผลที่ `IProductRepository` อยู่ใน Application เพราะ use case ต้องบอกว่า “ฉันต้องการเก็บ/อ่าน Product” แต่ไม่ควรรู้ว่าใช้ PostgreSQL, SQL Server, MongoDB, หรือ external API

นี่คือหลัก Dependency Inversion: ชั้น Application พึ่งพา abstraction ส่วน Infrastructure เป็นคน implement abstraction นั้น

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

`Product` เป็นคนดูแล rule ของตัวเอง เช่น name ห้ามว่าง และ price ต้องมากกว่า 0 ต่อให้ Application validator หลุดไป Domain ก็ยังป้องกันข้อมูลผิดพลาดอยู่

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
- `GET /products/{id}`
- `POST /products`
- `PUT /products/{id}`
- `DELETE /products/{id}`

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

JWT bearer authentication จะเปิดใช้เมื่อ config ครบทั้ง `Authentication:Authority` และ `Authentication:Audience`

```powershell
dotnet user-secrets set "Authentication:Authority" "https://issuer.example.com" --project .\src\Service.Api
dotnet user-secrets set "Authentication:Audience" "service-api" --project .\src\Service.Api
```

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
