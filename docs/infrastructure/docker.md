# 🐳 Docker & Development Environment

คู่มือนี้อธิบายการใช้งาน **Docker** เพื่อจำลองสภาพแวดล้อมในการรันโปรเจกต์ โดยไม่ต้องติดตั้ง Database ในเครื่องตัวเอง

---

## 🚀 รันโปรเจกต์ด้วย Docker Compose

วิธีที่ง่ายที่สุดในการเริ่มงานคือการใช้ Docker Compose ซึ่งจะสร้างทั้งตัว API และ Database ให้พร้อมกัน:

### 1. คำสั่งรัน
เปิด Terminal ที่ Root ของโปรเจกต์แล้วรัน:
```powershell
docker-compose up -d
```

### 2. สิ่งที่จะเกิดขึ้น
- **Database (PostgreSQL)**: ถูกสร้างขึ้นและเปิดพอร์ต `5432`
- **API**: ถูก Build และรันบนพอร์ต `8080`
- **Migrations & Seeding**: ระบบจะทำการสร้างตารางและใส่ข้อมูลตัวอย่าง (Products/Customers) ให้โดยอัตโนมัติ

---

## 🛠️ รายละเอียดไฟล์ Docker

### 1. Dockerfile
เราใช้ **Multi-stage build** เพื่อให้ Image มีขนาดเล็กและปลอดภัย:
- **Build Stage**: ใช้ SDK 10.0 ในการคอมไพล์โค้ด
- **Final Stage**: ใช้เฉพาะ ASP.NET Runtime เพื่อรันแอป

### 2. docker-compose.yml
ประกอบด้วย 2 บริการหลัก:
- **api**: ตัวแอปพลิเคชันของเรา
- **db**: ฐานข้อมูล PostgreSQL (ใช้ image `npgsql/postgresql`)

---

## 🌱 Database Seeding (ข้อมูลตัวอย่าง)

เพื่อให้การพัฒนาทำได้ทันที เรามีระบบ **Seed Data** อัตโนมัติ:
- เมื่อรันแอปครั้งแรก ระบบจะตรวจสอบว่าตารางว่างเปล่าหรือไม่
- หากว่างเปล่า ระบบจะเพิ่มข้อมูลตัวอย่าง เช่น สินค้า (Keyboard, Mouse) และลูกค้า เข้าไปให้เอง
- **Code**: สามารถดูหรือแก้ไขข้อมูล Seed ได้ที่ `src/Service.Infrastructure/DbInitializer.cs`

---

## 💡 คำแนะนำสำหรับ Jr. Dev

1.  **ดู Logs**: หากรันด้วย Docker แล้วมีปัญหา สามารถดู log ได้ด้วยคำสั่ง `docker-compose logs -f api`
2.  **ลบข้อมูลทั้งหมด**: หากต้องการ Reset ข้อมูลใหม่ทั้งหมด ให้รัน `docker-compose down -v` (คำสั่ง `-v` จะลบ Volume ของ database ทิ้งด้วย)
3.  **Local vs Docker**: คุณสามารถรัน DB ใน Docker แต่รัน API ใน Visual Studio ได้ โดยการแก้ Connection String ใน `appsettings.json` ให้ชี้ไปที่ `localhost:5432`

---
*ศึกษาเพิ่มเติม: [Docker Documentation](https://docs.docker.com/)*
