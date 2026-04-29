# 🚀 Service Template (.NET Minimal API)

Template นี้เป็น **Master Template** สำหรับเริ่มต้นพัฒนา Microservices หรือ Web API ใหม่ด้วย .NET 10 โดยใช้แนวคิด **Clean Architecture** และ **Minimal API**

> 🆕 **มาใหม่?** เริ่มต้นที่นี่เลย: [**Zero to Hero: คู่มือสำหรับมือใหม่**](docs/zero-to-hero.md)

เป้าหมายของ Template นี้คือเพื่อให้ Jr.Dev หรือทีมพัฒนาสามารถเริ่มโปรเจกต์ใหม่ได้ทันที (Ready-to-use) โดยมีโครงสร้างมาตรฐานที่รองรับการขยายตัว (Scalable) และทดสอบง่าย (Testable)

---

## ⚡ Quick Start (5 นาที)

สำหรับผู้ที่ต้องการรันโปรเจกต์ทันที ให้ทำตามขั้นตอนดังนี้:

### 🚀 รันผ่าน Docker (แนะนำที่สุด)
วิธีนี้จะรันทั้ง API, Database พร้อมข้อมูลตัวอย่าง (Seeding) ให้ทันที:
```powershell
# รันทุกอย่าง
docker-compose up -d
```

### 💻 รันแบบ Manual (Local)
1. **Database Setup**: ตั้งค่า Connection String และ JWT Key ผ่าน **User Secrets**:
```powershell
cd src/Service.Api
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=ServiceDb;Username=postgres;Password=your-password"
dotnet user-secrets set "Jwt:SigningKey" "a-very-secret-key-that-is-at-least-32-chars-long"
```
2. **Run**:
```powershell
dotnet run --project src/Service.Api
```

เมื่อรันสำเร็จ เข้าไปที่:
- **API UI**: [http://localhost:8080/scalar/v1](http://localhost:8080/scalar/v1)
- **Health Check**: [http://localhost:8080/health/live](http://localhost:8080/health/live)

---

## 🏗️ Architecture: Clean Architecture

โปรเจกต์นี้แบ่งออกเป็น 4 ชั้นหลัก เพื่อแยกหน้าที่ให้ชัดเจน (Separation of Concerns):

1.  **Service.Domain**: หัวใจของระบบ เก็บ Business Logic แท้ๆ (Entities, Value Objects) **ห้าม** พึ่งพาชั้นอื่นเลย
2.  **Service.Application**: เก็บ Use Cases (สั่งให้ระบบทำอะไร) และ Interfaces (Interfaces ของ Repository)
3.  **Service.Infrastructure**: ส่วนติดต่อกับโลกภายนอก (Database, EF Core, External APIs)
4.  **Service.Api**: ส่วนที่รับ HTTP Request และส่ง Response กลับ (Minimal API Endpoints, DTOs)

> **💡 กฎสำคัญ:** Dependency ต้องไหลเข้าด้านในเสมอ (API -> Application -> Domain) และ (Infrastructure -> Application)

---

## 🛠️ วิธีใช้เป็น Master Template

หากต้องการนำโปรเจกต์นี้ไปใช้เป็นจุดเริ่มต้นโปรเจกต์ใหม่:

### 1. เปลี่ยนชื่อโปรเจกต์ (Rename)
แนะนำให้ใช้เครื่องมือค้นหาและแทนที่ (Find & Replace) คำว่า `Service` เป็นชื่อโปรเจกต์ของคุณ (เช่น `OrderService`, `IdentityService`) ในไฟล์ดังนี้:
- ชื่อโฟลเดอร์และไฟล์ `.csproj`
- ชื่อไฟล์ Solution `.slnx`
- Namespace ในโค้ดทุกไฟล์

### 2. การเพิ่ม Feature ใหม่ (The Pattern)
เมื่อต้องการเพิ่ม Feature ใหม่ (เช่น `Orders`) ให้เดินตามรอยเดิมของ `Products`:
- **Domain**: สร้าง Entity ใน `src/Service.Domain/Orders`
- **Application**: สร้าง Use Case, Command, Validator ใน `src/Service.Application/Orders`
- **Infrastructure**: Implement Repository ใน `src/Service.Infrastructure/Repositories`
- **Api**: สร้าง Endpoints ใน `src/Service.Api/Features/Orders`

---

## 📚 แผนผังเอกสาร (Documentation Map)

ถ้าต้องการรายละเอียดเจาะลึก สามารถอ่านต่อได้ที่โฟลเดอร์ `docs/`:

| หัวข้อ | ลิงก์เอกสาร | คำอธิบาย |
| :--- | :--- | :--- |
| **ภาพรวม API** | [docs/api/quick-start.md](docs/api/quick-start.md) | วิธีเรียกใช้ Endpoint ต่างๆ |
| **Minimal API** | [docs/api/minimal-api.md](docs/api/minimal-api.md) | พื้นฐานและการใช้งาน Minimal API ในโปรเจกต์ |
| **ความปลอดภัย** | [docs/api/security.md](docs/api/security.md) | การตั้งค่า CORS และ Rate Limiting |
| **Docker** | [docs/infrastructure/docker.md](docs/infrastructure/docker.md) | การรันแอปด้วย Docker และ Seed Data |
| **รายละเอียด API** | [docs/api/customer-order.md](docs/api/customer-order.md) | Spec ของ Customer & Order API |
| **สถาปัตยกรรม** | [docs/architecture.md](docs/architecture.md) | เจาะลึกการวางโครงสร้าง Clean Architecture |
| **Program.cs** | [docs/architecture/program-cs.md](docs/architecture/program-cs.md) | อธิบายโค้ดเริ่มต้นแอปพลิเคชันอย่างละเอียด |
| **Database** | [docs/migrations/start-here.md](docs/migrations/start-here.md) | คู่มือการทำ Migration และจัดการ Schema |
| **การติดตั้ง** | [docs/api/implementation-details.md](docs/api/implementation-details.md) | รายละเอียดสิ่งที่ถูกสร้างไว้ใน Template นี้ |

---

## ✨ Key Features ที่มีให้แล้ว

- ✅ **Authentication**: ระบบ JWT Bearer Token (Login, Refresh, Revoke)
- ✅ **Global Exception Handling**: จัดการ Error และส่งกลับเป็น Problem Details (RFC 7807)
- ✅ **Pagination**: ระบบแบ่งหน้าสำหรับ List Endpoints
- ✅ **Structured Logging**: ใช้ Serilog พร้อม Correlation ID (ตาม Request ได้ง่าย)
- ✅ **Validation**: ใช้ FluentValidation ตรวจสอบข้อมูลขาเข้า
- ✅ **Health Checks**: Endpoint เช็คความพร้อมของ Service และ DB
- ✅ **OpenAPI (Scalar)**: เอกสาร API แบบ Interactive สวยงาม

---

## 🧪 Testing

เราแบ่ง Test ออกเป็น 2 ส่วนหลัก:
- **Unit Tests**: ทดสอบ Logic ใน Domain และ Application (ไม่ต้องต่อ DB)
- **Integration Tests**: ทดสอบการต่อกันของแต่ละ Layer และ DI Wiring

รัน Test ทั้งหมดด้วยคำสั่ง:
```powershell
dotnet test .\Service.slnx
```

---

## 📞 Team Guidelines

- **Keep Endpoints Thin**: Endpoint ควรทำหน้าที่แค่รับ-ส่ง ไม่ควรมี logic ซับซ้อน
- **Logic in Application**: Business logic ของระบบควรอยู่ที่ Use Case
- **No Secrets in Git**: ห้าม commit password หรือ API Key ลง GitHub (ใช้ User Secrets/Environment Variables)
- **Small Classes**: แยกไฟล์ให้เล็กและตั้งชื่อให้ชัดเจนตามหน้าที่

---
*Last Updated: 2026-04-29*
