# 📝 Logging: Serilog Configuration

คู่มือนี้อธิบายการจัดการ Log ใน Template นี้โดยใช้ **Serilog** ซึ่งรองรับการเก็บ Log ได้หลายรูปแบบ (Sinks)

---

## 🚀 รูปแบบการเก็บ Log (Sinks)

ใน Template นี้เราติดตั้ง Sinks มาให้ 3 แบบหลัก ซึ่งสามารถเปิด/ปิดได้ผ่าน `appsettings.json`:

1.  **Console**: แสดง Log บนหน้าจอ Terminal (เหมาะสำหรับ Development)
2.  **File**: บันทึก Log ลงในไฟล์ text (เหมาะสำหรับ Local/On-premise)
3.  **PostgreSQL**: บันทึก Log ลงใน Database (เหมาะสำหรับ Production ที่ต้องการค้นหา Log ย้อนหลังได้ง่าย)

---

## ⚙️ การตั้งค่าใน `appsettings.json`

คุณสามารถควบคุมการไหลของ Log ได้จากส่วน `WriteTo`:

```json
"Serilog": {
  "WriteTo": [
    { "Name": "Console" },
    {
      "Name": "File",
      "Args": {
        "path": "logs/log-.txt",
        "rollingInterval": "Day"
      }
    },
    {
      "Name": "PostgreSQL",
      "Args": {
        "connectionString": "...",
        "tableName": "Logs",
        "needAutoCreateTable": true
      }
    }
  ]
}
```

### วิธีเปิด/ปิด Sink
หากไม่ต้องการเก็บ Log ลงที่ไหน (เช่น ไม่เอาลง DB ในช่วง Develop) ให้ทำการ **ลบ Object** ของ Sink นั้นออกจาก Array `WriteTo` หรือใช้ไฟล์ `appsettings.Development.json` เพื่อ Override ค่า

## 🗄️ การสร้างตารางใน Database

ใน Template นี้มีการตั้งค่า `"needAutoCreateTable": true` ซึ่งจะทำให้ Serilog สร้างตารางให้เองโดยอัตโนมัติเมื่อรันแอปครั้งแรก

อย่างไรก็ตาม ในระบบ **Production** ที่ User ของ Database อาจไม่มีสิทธิ์ในการสร้างตาราง (Create Table) แนะนำให้ใช้ Script SQL ที่เตรียมไว้ให้:
👉 **Script**: `docs/migrations/create_logs_table.sql`

ข้อดีของการใช้ Script เอง:
- สามารถกำหนด Index หรือ Partitioning เพิ่มเติมได้เอง
- ควบคุม Permission ได้ดีกว่า
- ตรวจสอบ Schema ได้ง่ายก่อนใช้งานจริง

---

## 💡 ส่วนประกอบสำคัญ

### 1. Structured Logging
Serilog ไม่ได้เก็บแค่ตัวอักษร แต่เก็บเป็น **Structured Data (JSON)** ทำให้เราสามารถค้นหา Log ตาม property ได้ เช่น ค้นหาตาม `CorrelationId` หรือ `ApplicationName`

### 2. Enrichment
เรามีการใส่ข้อมูลเพิ่มเข้าไปในทุกๆ Log โดยอัตโนมัติ:
- **CorrelationId**: ช่วยให้ไล่ลำดับเหตุการณ์ในหนึ่ง Request ได้ (แม้จะผ่านหลาย Service)
- **Application**: ชื่อ Service ที่เกิด Log นั้น

---

## ✅ Best Practices สำหรับ Jr. Dev

1.  **Log Level**:
    *   `Information`: งานทั่วไปที่เกิดขึ้นปกติ
    *   `Warning`: สิ่งที่ผิดปกติแต่ระบบยังทำงานได้
    *   `Error`: ระบบทำงานพลาด (มี Exception)
2.  **Don't Log Secrets**: ห้ามบันทึก Password, Token หรือข้อมูลส่วนตัว (PII) ลงใน Log เด็ดขาด
3.  **Use PostgreSQL for Prod**: ในระบบจริง แนะนำให้เปิด PostgreSQL sink เพื่อให้ทีมสามารถใช้ Tool อื่นๆ มา Query ดูความผิดปกติได้ง่าย

---
*ศึกษาเพิ่มเติม: [Serilog Documentation](https://serilog.net/)*
