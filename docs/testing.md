# 🧪 Testing Guide

คู่มือนี้อธิบายแนวทางการเขียนและรัน Test ใน Template นี้เพื่อให้มั่นใจว่าโค้ดที่เขียนทำงานได้ถูกต้องและไม่เกิด Regression

---

## 🏗️ โครงสร้างของ Test

เราแบ่ง Test ออกเป็น 2 โปรเจกต์หลัก:

### 1. Service.UnitTests
เน้นทดสอบ Logic ภายใน Class ใด Class หนึ่งแบบแยกส่วน (Isolation):
- **Domain Tests**: ทดสอบ Business Rules ใน Entity (เช่น การคำนวณ TotalPrice)
- **UseCase Tests**: ทดสอบ flow ของ Use Case โดยใช้ **Fake Repository** แทนการต่อ Database จริง
- **Validator Tests**: ทดสอบเงื่อนไขการตรวจสอบข้อมูล (FluentValidation)

### 2. Service.IntegrationTests
เน้นทดสอบการทำงานร่วมกันของหลายๆ ส่วน:
- **Dependency Injection (DI)**: ตรวจสอบว่า Class ต่างๆ ถูกลงทะเบียนใน Service Provider ครบถ้วน
- **Infrastructure**: ทดสอบการติดต่อกับระบบภายนอก (หากมีการ Mock หรือใช้ DB จริง)
- **Auth**: ทดสอบระบบ Token และ Refresh Token

---

## 🚀 วิธีการรัน Test

คุณสามารถรัน Test ทั้งหมดผ่าน Terminal ด้วยคำสั่ง:

```powershell
dotnet test .\Service.slnx
```

หรือใช้ **Test Explorer** ใน Visual Studio / VS Code เพื่อรันและ Debug เฉพาะ Case ที่สนใจ

---

## 💡 แนวทางการเพิ่ม Test Case สำหรับ Jr. Dev

เมื่อคุณสร้าง Feature ใหม่ ให้เดินตามรอยเดิมดังนี้:

1.  **Validator Tests**: เพิ่ม Test เพื่อเช็คว่าถ้าส่งข้อมูลผิด (เช่น ชื่อว่าง, ราคาติดลบ) ระบบจะต้อง Error
2.  **UseCase Success Path**: ทดสอบว่าถ้าข้อมูลครบ ระบบต้องบันทึกข้อมูลได้สำเร็จ
3.  **UseCase Edge Cases**: ทดสอบกรณีพิเศษ เช่น
    *   การค้นหาแล้วไม่เจอข้อมูล (NotFound)
    *   การลบข้อมูลที่ไม่มีอยู่จริง
    *   การส่ง Page ที่เกินจำนวนที่มีอยู่จริง (ต้องได้รายการว่าง ไม่ใช่ Error)

---

## ✅ Best Practices

- **Naming**: ตั้งชื่อ Test ให้สื่อสารชัดเจน เช่น `[MethodName]_[Scenario]_[ExpectedResult]`
- **Arrange-Act-Assert (AAA)**: แบ่งโค้ดใน Test เป็น 3 ส่วนให้ชัดเจน
- **Fakes vs Mocks**: ใน Template นี้เราเลือกใช้ **Fake Classes** (เช่น `FakeProductRepository`) แทน Mocking Framework เพื่อให้ Test อ่านง่ายและรันได้เร็วขึ้น
- **Independent Tests**: Test แต่ละ Case ต้องทำงานแยกจากกัน ไม่ควรมีลำดับก่อน-หลัง

---
*ศึกษาเพิ่มเติม: [xUnit Documentation](https://xunit.net/)*
