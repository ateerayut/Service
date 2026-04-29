# ⚡ Minimal API Guide

คู่มือนี้อธิบายแนวคิดและการใช้งาน **Minimal API** ใน .NET 10 ซึ่งเป็นหัวใจหลักของ API Layer ใน Template นี้

---

## ❓ Minimal API คืออะไร?

Minimal API คือแนวทางการสร้าง HTTP API ของ ASP.NET Core ที่ออกแบบมาให้มี **Boilerplate code (โค้ดส่วนเกิน) น้อยที่สุด** โดยเน้นความเรียบง่ายและประสิทธิภาพสูงสุด (Performance) 

ในสมัยก่อน (Controller-based) เราต้องสร้าง Class Controller, สืบทอดจาก `ControllerBase`, และใช้ Attributes มากมาย แต่ Minimal API อนุญาตให้เราเขียน Endpoint ได้โดยตรงใน `Program.cs` หรือแยกไฟล์ออกไปโดยใช้ Lambda expressions

---

## 🎯 ทำไม Template นี้ถึงใช้ Minimal API?

1.  **Performance**: มีความเร็วในการทำงานสูงกว่า Controller-based เล็กน้อยเนื่องจากข้ามขั้นตอนการทำ Reflection ที่ซับซ้อน
2.  **Simplicity**: โค้ดอ่านง่าย เห็นภาพรวมของ Endpoint ได้ชัดเจนในจุดเดียว
3.  **Modern standard**: เป็นแนวทางหลักที่ Microsoft แนะนำสำหรับการสร้าง Microservices ในปัจจุบัน

---

## 🏗️ การจัดโครงสร้างใน Template นี้

แม้ Minimal API จะอนุญาตให้เขียนทุกอย่างใน `Program.cs` แต่เพื่อให้โปรเจกต์ขยายตัวได้ (Scalable) เราจึงจัดโครงสร้างแบบ **Feature-based** ดังนี้:

### 1. การ Register Endpoints
ใน `src/Service.Api/Program.cs` เราจะเห็นการเรียกใช้ method เพื่อลงทะเบียน endpoint ของแต่ละ feature:

```csharp
// Program.cs
app.MapAuthEndpoints();
app.MapProductEndpoints();
app.MapCustomerEndpoints();
app.MapOrderEndpoints();
```

### 2. การสร้างไฟล์ Endpoint (Feature-based)
ตัวอย่างใน `src/Service.Api/Features/Products/ProductEndpoints.cs`:

```csharp
public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products")
                       .WithTags("Products")
                       .RequireAuthorization(); // บังคับ Auth ทั้งกลุ่ม

        group.MapGet("/", ListProducts);
        group.MapGet("/{id}", GetProduct);
        group.MapPost("/", CreateProduct);
    }

    // Handler logic...
}
```

---

## 💡 ส่วนประกอบสำคัญที่ Jr. Dev ควรรู้

### 1. Route Grouping (`MapGroup`)
ใช้สำหรับจัดกลุ่ม API ที่มีเส้นทาง (Path) ขึ้นต้นเหมือนกัน ช่วยให้เราสามารถใส่ Middleware หรือ Authorization ให้กับทั้งกลุ่มได้ในครั้งเดียว

### 2. Parameter Binding
Minimal API ฉลาดพอที่จะรู้ว่าข้อมูลมาจากไหนโดยอัตโนมัติ:
- `/{id}` -> อ่านจาก **Route**
- `[AsParameters] ListProductsQuery query` -> อ่านจาก **Query String**
- `CreateProductRequest request` -> อ่านจาก **Request Body** (JSON)
- `IProductRepository repo` -> อ่านจาก **Dependency Injection (DI)**

### 3. IResult (The Response)
เราใช้ `Results` หรือ `TypedResults` สำหรับส่งค่ากลับไปให้ Client:
- `TypedResults.Ok(data)` -> HTTP 200
- `TypedResults.Created($"/products/{id}", resp)` -> HTTP 201
- `TypedResults.NotFound()` -> HTTP 404
- `TypedResults.ValidationProblem(errors)` -> HTTP 400

### 4. TypedResults vs Results
ใน Template นี้แนะนำให้ใช้ `TypedResults` เพราะจะช่วยให้ **OpenAPI (Scalar)** แสดงผล Type ของ Response ได้ถูกต้องโดยไม่ต้องใส่ Attributes เพิ่มเติม

---

## 📞 เปรียบเทียบกับ Controller (สำหรับคนที่คุ้นเคยแบบเดิม)

| เรื่อง | Controller-based | Minimal API (ใน Template นี้) |
| :--- | :--- | :--- |
| **ที่เก็บโค้ด** | `Controllers/*.cs` | `Features/*/*Endpoints.cs` |
| **การเข้าถึง DI** | ผ่าน Constructor | ผ่าน Parameter ของ Method ได้เลย |
| **การกำหนด Route** | `[Route("api/[controller]")]` | `app.MapGroup("/path")` |
| **ความซับซ้อน** | มี Base Class และกฎเยอะ | เป็น Static Method เรียบง่าย |

---

## ✅ Best Practices สำหรับ Jr. Dev

1.  **Keep it Thin**: อย่าเขียน Business Logic ในไฟล์ Endpoints ให้เรียกใช้ Use Case จากชั้น Application แทน
2.  **Use Feature Folders**: เก็บ Endpoints ไว้กับ DTOs ของ feature นั้นๆ เพื่อให้หาโค้ดง่าย
3.  **Explicit Binding**: หาก Parameter เยอะเกินไป ให้ใช้ `[AsParameters]` กับ record เพื่อความเป็นระเบียบ
4.  **Meaningful Tags**: ใช้ `.WithTags("Name")` เพื่อให้ใน Scalar/Swagger แยกกลุ่ม API ให้สวยงาม

---
*ศึกษาเพิ่มเติมได้ที่: [Official Microsoft Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)*
