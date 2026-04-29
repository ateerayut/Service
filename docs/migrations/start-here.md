# 🗄️ Database Migrations Guide

คู่มือสำหรับการจัดการ Database Schema ด้วย Entity Framework Core Migrations ในโปรเจกต์นี้

---

## 🎯 QUICK START - เลือกเส้นทางของคุณ

### 👶 สำหรับมือใหม่ (เริ่มที่นี่)
1. อ่าน: [Migration Index](index.md) (5 นาที)
2. อ่าน: [README.md](../../README.md) ส่วน Database Migrations
3. ลองทำตาม: Pattern 1 ใน [Quick Reference](quick-reference.md)

### 🚀 สำหรับ Developer (ต้องการรันคำสั่งทันที)
1. ไปที่: [Quick Reference](quick-reference.md)
2. เลือก Pattern ที่ต้องการ (1-6)
3. Copy คำสั่งไปวางใน PowerShell

### 🏭 สำหรับการขึ้น Production
1. อ่าน: [README.md](../../README.md) -> Production Best Practices
2. ทำตาม: [Quick Reference](quick-reference.md) -> Production Workflow
3. ตรวจสอบ SQL: [SQL Examples](sql-examples.md)

---

## 📂 รายการเอกสารทั้งหมด

- **[index.md](index.md)**: ศูนย์รวมลิงก์และเส้นทางการเรียนรู้
- **[quick-reference.md](quick-reference.md)**: รวมคำสั่ง PowerShell ที่ใช้บ่อย (Copy-paste ได้เลย)
- **[sql-examples.md](sql-examples.md)**: ตัวอย่าง SQL ที่ EF Core สร้างขึ้นในแต่ละ Scenario
- **[summary.md](summary.md)**: สรุปสถานะ Database และ Entities ปัจจุบัน
- **[overview.md](overview.md)**: ภาพรวมโครงสร้างของ Suite เอกสารนี้

---

## ⚡ คำสั่งด่วน (Super Quick Commands)

```powershell
# 1. สร้าง Migration ใหม่ (เมื่อมีการแก้ Domain Entity)
dotnet ef migrations add MyMigrationName `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

# 2. Apply ลง Database (Local)
dotnet ef database update `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

---

## 🎯 ขั้นตอนมาตรฐาน (3-Step Process)

1. **Modify Entity**: แก้ไข Domain Entity ใน `src/Service.Domain`
2. **Update DbContext**: ตั้งค่าเพิ่มเติมใน `AppDbContext.cs` (ถ้าจำเป็น)
3. **Create & Apply**: รันคำสั่งสร้างและอัปเดต Database

---
*Last Updated: 2026-04-29*
