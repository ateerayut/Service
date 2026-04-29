# Architecture & File Structure

## 🏗️ Clean Architecture Overview

The Customer and Order APIs follow the Clean Architecture pattern with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    API Layer (Service.Api)                   │
│  HTTP Endpoints, DTOs, Request/Response Handling             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ CustomerEndpoints | OrderEndpoints | ProductEndpoints│   │
│  │ CustomerResponse | OrderResponse | etc.              │   │
│  └──────────────────────────────────────────────────────┘   │
└──────────────────────┬──────────────────────────────────────┘
                       │ Depends on
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              Application Layer (Service.Application)          │
│  Business Logic, Use Cases, Validation, DTOs                 │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Use Cases: Create/Get/List/Update/Delete             │   │
│  │ Commands/Queries: Request objects for operations     │   │
│  │ Validators: FluentValidation rules                   │   │
│  │ Repositories (Interfaces): Data access contracts     │   │
│  └──────────────────────────────────────────────────────┘   │
└──────────────────────┬──────────────────────────────────────┘
                       │ Depends on
                       ▼
┌─────────────────────────────────────────────────────────────┐
│           Infrastructure Layer (Service.Infrastructure)      │
│  Concrete Implementations, EF Core, Database Access          │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Repositories (Implementation): EF Core queries        │   │
│  │ AppDbContext: Entity mappings and DbSets             │   │
│  │ Migrations: Database schema management               │   │
│  └──────────────────────────────────────────────────────┘   │
└──────────────────────┬──────────────────────────────────────┘
                       │ Depends on
                       ▼
┌─────────────────────────────────────────────────────────────┐
│               Domain Layer (Service.Domain)                   │
│  Business Entities, Validation Rules, Aggregates             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Customer Entity | Order Entity | OrderItem Entity    │   │
│  │ Business Rules: Create(), Update(), AddItem()        │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

**Key Principle**: Dependencies always point inward. The Domain layer has no dependencies on other layers.

---

## 📁 File Structure - Customer API

```
src/Service.Api/Features/Customers/
├── CustomerEndpoints.cs         ← HTTP endpoints (GET, POST, PUT, DELETE)
├── CustomerResponse.cs          ← DTO for API responses
├── CreateCustomerRequest.cs     ← DTO for POST request
├── UpdateCustomerRequest.cs     ← DTO for PUT request
└── CreateCustomerResponse.cs    ← DTO for create response

src/Service.Application/Customers/
├── CustomerDto.cs              ← DTO for data transfer
├── CreateCustomerCommand.cs    ← Command object for creation
├── UpdateCustomerCommand.cs    ← Command object for update
├── ListCustomersQuery.cs       ← Query object for listing
├── ICustomerRepository.cs      ← Repository interface
├── CreateCustomerUseCase.cs    ← Business logic for creation
├── GetCustomerByIdUseCase.cs   ← Business logic for GET by ID
├── ListCustomersUseCase.cs     ← Business logic for listing
├── UpdateCustomerUseCase.cs    ← Business logic for update
├── DeleteCustomerUseCase.cs    ← Business logic for delete
├── CreateCustomerCommandValidator.cs  ← Validation rules
├── UpdateCustomerCommandValidator.cs  ← Validation rules
└── ListCustomersQueryValidator.cs     ← Validation rules

src/Service.Infrastructure/Repositories/
└── CustomerRepository.cs       ← EF Core implementation

src/Service.Domain/Customers/
└── Customer.cs                 ← Domain entity with business logic
```

---

## 📁 File Structure - Order API

```
src/Service.Api/Features/Orders/
├── OrderEndpoints.cs            ← HTTP endpoints (GET, POST, DELETE, add items)
├── OrderResponse.cs             ← DTO for API responses (with nested items)
├── CreateOrderRequest.cs        ← DTO for POST request
├── AddOrderItemRequest.cs       ← DTO for adding items to order
└── CreateOrderResponse.cs       ← DTO for create response

src/Service.Application/Orders/
├── OrderDto.cs                 ← DTO for order and order item data
├── CreateOrderCommand.cs       ← Command object for creation
├── AddOrderItemCommand.cs      ← Command object for adding items
├── ListOrdersQuery.cs          ← Query object for listing
├── IOrderRepository.cs         ← Repository interface
├── CreateOrderUseCase.cs       ← Business logic for creation
├── GetOrderByIdUseCase.cs      ← Business logic for GET by ID
├── ListOrdersUseCase.cs        ← Business logic for listing
├── AddOrderItemUseCase.cs      ← Business logic for adding items
├── DeleteOrderUseCase.cs       ← Business logic for delete
├── CreateOrderCommandValidator.cs    ← Validation rules
├── AddOrderItemCommandValidator.cs   ← Validation rules
└── ListOrdersQueryValidator.cs       ← Validation rules

src/Service.Infrastructure/Repositories/
└── OrderRepository.cs          ← EF Core implementation

src/Service.Domain/Orders/
├── Order.cs                    ← Domain entity (aggregate root)
└── OrderItem.cs                ← Domain entity (value object)
```

---

## 🔄 Data Flow Example - Creating a Customer

```
HTTP Request
    │
    ▼
┌─────────────────────────────────────────┐
│ CustomerEndpoints (API Layer)           │
│ Receives: POST /customers               │
│ Body: { "name": "John" }                │
└─────────────────┬───────────────────────┘
                  │ Deserializes to
                  ▼
        CreateCustomerRequest
                  │
                  ▼ Injects
┌─────────────────────────────────────────┐
│ CreateCustomerUseCase (Application)     │
│ 1. Validates input with validator       │
│ 2. Creates Customer domain entity       │
│ 3. Calls repository.Add()               │
└─────────────────┬───────────────────────┘
                  │ Uses injected repo
                  ▼
┌─────────────────────────────────────────┐
│ CustomerRepository (Infrastructure)     │
│ 1. DbContext.Customers.Add(customer)    │
│ 2. DbContext.SaveChangesAsync()         │
└─────────────────┬───────────────────────┘
                  │ Executes
                  ▼
            Database (PostgreSQL)
                  │
                  ▼ Returns
        Customer entity saved
                  │
                  ▼ Maps to
        CustomerResponse (ID)
                  │
                  ▼ Returns
    201 Created with Location header
                  │
                  ▼
            HTTP Response
```

---

## 🔌 Dependency Injection Flow

### How Dependencies Are Registered

1. **API Layer** (`Program.cs`):
   ```csharp
   builder.Services.AddApplication();      // Register Application layer
   builder.Services.AddInfrastructure();   // Register Infrastructure layer
   ```

2. **Application Layer** (`DependencyInjection.cs`):
   ```csharp
   services.AddValidatorsFromAssembly();   // Auto-register validators
   services.AddScoped<CreateCustomerUseCase>();
   services.AddScoped<ListCustomersUseCase>();
   // ... etc
   ```

3. **Infrastructure Layer** (`DependencyInjection.cs`):
   ```csharp
   services.AddDbContext<AppDbContext>();
   services.AddScoped<ICustomerRepository, CustomerRepository>();
   services.AddScoped<IOrderRepository, OrderRepository>();
   ```

### How Injection Works at Endpoint

```csharp
// In CustomerEndpoints.cs
group.MapPost("/",
    async (
        CreateCustomerRequest request,
        CreateCustomerUseCase uc,           // ← Injected from DI container
        CancellationToken ct) =>
    {
        // CreateCustomerUseCase constructor receives:
        // - ICustomerRepository (registered as CustomerRepository)
        // - IValidator<CreateCustomerCommand> (auto-discovered)
    }
);
```

---

## 📊 Database Schema Relationships

```
Customers Table
├── Id (GUID, Primary Key)
├── Name (VARCHAR)
├── CreateDate (TIMESTAMP)
└── INDICES: Id

Orders Table
├── Id (GUID, Primary Key)
├── CustomerId (GUID, Foreign Key → Customers.Id)
├── CreateDate (TIMESTAMP)
└── INDICES: Id, CustomerId

OrderItems Table
├── Id (GUID, Primary Key)
├── OrderId (GUID, Foreign Key → Orders.Id)
├── ProductId (GUID, Foreign Key → Products.Id)
├── Quantity (INT)
├── UnitPrice (DECIMAL)
└── INDICES: Id, OrderId, ProductId
```

**Foreign Key Constraints:**
- `Orders.CustomerId` → `Customers.Id` (OnDelete: Restrict)
- `OrderItems.OrderId` → `Orders.Id` (OnDelete: Cascade)
- `OrderItems.ProductId` → `Products.Id` (OnDelete: Restrict)

---

## 🔀 Request/Response Flow - Add Order Item

```
┌──────────────────────────────────────────────────────────────┐
│ 1. HTTP Request                                              │
│    POST /orders/{orderId}/items                              │
│    { "productId": "...", "quantity": 2, "unitPrice": 29.99 } │
└──────────────┬───────────────────────────────────────────────┘
               │
               ▼
┌──────────────────────────────────────────────────────────────┐
│ 2. OrderEndpoints (API)                                      │
│    - Deserializes AddOrderItemRequest                        │
│    - Injects AddOrderItemUseCase                             │
│    - Calls uc.Execute(AddOrderItemCommand, ct)              │
└──────────────┬───────────────────────────────────────────────┘
               │
               ▼
┌──────────────────────────────────────────────────────────────┐
│ 3. AddOrderItemUseCase (Application)                         │
│    - Validates AddOrderItemCommand                           │
│    - Gets Order from repository                              │
│    - Calls order.AddItem(productId, qty, price)             │
│    - Updates repository                                      │
└──────────────┬───────────────────────────────────────────────┘
               │
               ▼
┌──────────────────────────────────────────────────────────────┐
│ 4. Order.AddItem() (Domain)                                  │
│    - Validates all parameters                                │
│    - Creates OrderItem via OrderItem.Create()               │
│    - Adds to internal _items collection                     │
└──────────────┬───────────────────────────────────────────────┘
               │
               ▼
┌──────────────────────────────────────────────────────────────┐
│ 5. OrderRepository.Update() (Infrastructure)                 │
│    - DbContext.Orders.Update(order)                         │
│    - SaveChangesAsync() - EF Core saves order + items       │
└──────────────┬───────────────────────────────────────────────┘
               │
               ▼
┌──────────────────────────────────────────────────────────────┐
│ 6. HTTP Response                                             │
│    204 No Content                                            │
│    (Item successfully added to order)                        │
└──────────────────────────────────────────────────────────────┘
```

---

## ✅ Validation Chain

```
Customer Creation Request
│
├─1. API Layer Binding
│   └─ ASP.NET Core binds JSON to CreateCustomerRequest DTO
│
├─2. Endpoint Handler
│   └─ Creates CreateCustomerCommand from request
│
├─3. Use Case Validation
│   ├─ Injects IValidator<CreateCustomerCommand>
│   └─ Calls validator.ValidateAsync(command)
│       └─ Validates: Name required, max 200 chars
│
├─4. Domain Entity Creation
│   └─ Customer.Create(name)
│       └─ Domain validation: Name is required
│           (Exception if validation fails)
│
└─5. Database Persistence
    └─ Save to database with constraints
```

---

## 🔐 Security Layers

```
┌────────────────────────────────────┐
│ 1. HTTP Authentication             │
│    JWT Bearer Token validation      │
└────────────────┬───────────────────┘
                 │
┌────────────────▼───────────────────┐
│ 2. Endpoint Authorization           │
│    [Authorize] / RequireAuthorization()│
└────────────────┬───────────────────┘
                 │
┌────────────────▼───────────────────┐
│ 3. Input Validation                 │
│    - API DTO binding                │
│    - FluentValidation rules         │
│    - Domain entity validation       │
└────────────────┬───────────────────┘
                 │
┌────────────────▼───────────────────┐
│ 4. Database Constraints             │
│    - Foreign key constraints        │
│    - Unique constraints             │
│    - Type constraints               │
└────────────────────────────────────┘
```

---

## 📈 Scalability Considerations

### Current Features Supporting Growth:
- ✅ Pagination for list endpoints (supports large datasets)
- ✅ Database indices on FK columns (fast lookups)
- ✅ Async/await throughout (non-blocking I/O)
- ✅ Repository pattern (allows caching layer)
- ✅ Dependency injection (enables mocking, swapping implementations)

### Future Enhancements:
- Add caching layer (Redis) in repository implementations
- Implement soft deletes for audit trail
- Add event sourcing for order state tracking
- Implement bulk operations for performance
- Add filtering options beyond search
- Implement ordering/sorting on list endpoints

---

## 🧪 Testing Structure Recommendation

```
tests/Service.UnitTests/
├── Customers/
│   ├── CreateCustomerCommandValidatorTests.cs
│   ├── UpdateCustomerCommandValidatorTests.cs
│   ├── CustomerRepositoryTests.cs
│   └── CustomerUseCaseTests.cs
├── Orders/
│   ├── AddOrderItemCommandValidatorTests.cs
│   ├── OrderRepositoryTests.cs
│   └── OrderUseCaseTests.cs

tests/Service.IntegrationTests/
├── Customers/
│   ├── CustomerEndpointTests.cs
│   └── CustomerWorkflowTests.cs
├── Orders/
│   ├── OrderEndpointTests.cs
│   └── OrderWorkflowTests.cs
```

---

This architecture ensures:
- **Maintainability**: Clear separation of concerns
- **Testability**: Dependencies are injected and mockable
- **Scalability**: Each layer can be enhanced independently
- **Reusability**: Common patterns across all entities
- **Security**: Multiple validation and authorization layers
