# 📂 เจาะลึกไฟล์ Program.cs

ไฟล์ `Program.cs` เปรียบเสมือน **"สมองกลาง"** ของแอปพลิเคชัน ทำหน้าที่ตั้งค่าเริ่มต้น ลงทะเบียนบริการต่างๆ และกำหนดลำดับการทำงานของ Request (Middleware) คู่มือนี้จะอธิบายโค้ดแต่ละส่วนเพื่อให้ Jr. Dev เข้าใจภาพรวมได้ง่ายขึ้น

---

## 🏗️ 1. การสร้าง Builder (`WebApplication.CreateBuilder`)

```csharp
var builder = WebApplication.CreateBuilder(args);
```
บรรทัดแรกสุดคือการสร้าง `builder` ซึ่งเป็นตัวรวบรวม Configuration (จาก `appsettings.json`) และ Environment ต่างๆ ของระบบ

---

## 🛠️ 2. การลงทะเบียน Services (Dependency Injection)

ส่วนนี้ใช้ `builder.Services` เพื่อบอกแอปพลิเคชันว่ามี "เครื่องมือ" อะไรให้ใช้บ้าง:

### Logging (Serilog)
```csharp
builder.Host.UseSerilog((context, services, loggerConfiguration) => { ... });
```
เปลี่ยนจาก Logger มาตรฐานเป็น **Serilog** เพื่อให้เก็บ Log ลงไฟล์หรือ Database ได้ตามที่เราคอนฟิกไว้

### OpenAPI & Documentation
```csharp
builder.Services.AddOpenApi(options => { ... });
```
เปิดใช้งานระบบสร้างเอกสาร API อัตโนมัติ (Swagger/Scalar) พร้อมตั้งค่าให้รองรับการใส่ Bearer Token เพื่อทดสอบ API ที่ต้อง Login

### Authentication & Authorization (ความปลอดภัย)
```csharp
builder.Services.AddAuthentication(...).AddJwtBearer(...);
builder.Services.AddAuthorization();
```
- **Authentication**: ตรวจสอบว่า "คุณคือใคร" (ใช้ JWT Token)
- **Authorization**: ตรวจสอบว่า "คุณมีสิทธิ์ทำอะไร"

### CORS & Rate Limiting
```csharp
builder.Services.AddCors(...);        // อนุญาตให้ Frontend Domain อื่นเรียก API ได้
builder.Services.AddRateLimiter(...); // จำกัดจำนวนครั้งในการเรียก API เพื่อป้องกันการโดน Spam
```

### การเชื่อมต่อ Layer ต่างๆ
```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
```
ลงทะเบียน Use Cases จากชั้น **Application** และ Repositories/DbContext จากชั้น **Infrastructure**

---

## 🧱 3. การ Build แอปพลิเคชัน

```csharp
var app = builder.Build();
```
เมื่อลงทะเบียนของทุกอย่างเสร็จแล้ว เราจะสั่ง `Build()` เพื่อเปลี่ยนจาก "ตัวเตรียมของ" ให้กลายเป็น "ตัวแอปพลิเคชัน" จริงๆ

---

## 🌱 4. Database Seeding (เตรียมข้อมูลตัวอย่าง)

```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.SeedAsync(context);
}
```
ส่วนนี้จะรันตอนเปิดแอปฯ เสมอ เพื่อตรวจสอบว่า Database พร้อมใช้งานไหม และใส่ข้อมูลตัวอย่าง (Products/Customers) ให้เราอัตโนมัติ

---

## 🔄 5. Middleware Pipeline (ลำดับการประมวลผล)

ส่วนที่ขึ้นต้นด้วย `app.Use...` คือการวางลำดับว่าเมื่อมี Request เข้ามา จะต้องผ่านด่านอะไรบ้าง **(ลำดับมีความสำคัญมาก!)**:

1.  **GlobalExceptionHandling**: ดักจับ Error ทั้งหมดในระบบ
2.  **CorrelationId**: ใส่ ID พิเศษให้ทุก Request เพื่อให้ไล่ Log ได้ง่าย
3.  **SerilogRequestLogging**: บันทึกรายละเอียดการเรียก API ลง Log
4.  **CORS & RateLimiter**: ตรวจสอบสิทธิ์ Domain และจำกัดจำนวนครั้ง
5.  **Authentication & Authorization**: ตรวจสอบตัวตนและสิทธิ์การเข้าถึง

---

## 📍 6. Endpoint Mapping (เส้นทาง API)

ส่วนสุดท้ายคือการบอกว่า URL ไหนจะวิ่งไปที่โค้ดส่วนไหน:

```csharp
app.MapOpenApi();           // เปิดไฟล์ json ของเอกสาร API
app.MapScalarApiReference(); // เปิดหน้าเว็บเอกสาร API สวยๆ
app.MapGet("/health/live"); // เช็คว่าแอปยังรันอยู่ไหม
app.MapAuthEndpoints();     // API สำหรับ Login/Refresh Token
app.MapProductEndpoints();  // API จัดการสินค้า
// ... อื่นๆ
```

---

## 🚀 7. การเริ่มทำงาน (`app.Run`)

```csharp
app.Run($"http://0.0.0.0:{port}");
```
คำสั่งสุดท้ายเพื่อสั่งให้แอปพลิเคชันเริ่มรับ Request จากโลกภายนอกผ่านพอร์ตที่กำหนด (ค่าเริ่มต้นคือ 8080)

---
*💡 ข้อแนะนำ: Jr. Dev ไม่ควรแก้ไขลำดับในส่วนของ Middleware (app.Use...) หากไม่มั่นใจ เพราะอาจทำให้ระบบความปลอดภัยทำงานผิดพลาดได้*
