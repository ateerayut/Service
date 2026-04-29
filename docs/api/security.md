# 🛡️ Security: CORS & Rate Limiting

คู่มือนี้อธิบายการตั้งค่าความปลอดภัยพื้นฐานใน Template นี้ ได้แก่ **CORS** และ **Rate Limiting**

---

## 🌐 CORS (Cross-Origin Resource Sharing)

CORS คือระบบความปลอดภัยที่ Browser ใช้เพื่อป้องกันไม่ให้ Website หนึ่งไปดึงข้อมูลจาก API ของอีก Domain หนึ่งโดยไม่ได้รับอนุญาต

### การตั้งค่าใน Template
เราตั้งค่า CORS ไว้ใน `appsettings.json` เพื่อให้ง่ายต่อการเปลี่ยนตาม Environment:

```json
{
  "AllowedOrigins": [
    "http://localhost:3000",
    "http://localhost:5173"
  ]
}
```

*   **Local Development**: ใส่ URL ของ Frontend (เช่น React, Vue) ที่รันอยู่ในเครื่อง
*   **Production**: ใส่ URL จริงของ Frontend ที่ Deploy แล้ว

### วิธีทำงานในโค้ด
ใน `Program.cs` เราดึงค่าจาก config มาสร้าง Default Policy:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

---

## 🚦 Rate Limiting

Rate Limiting คือการจำกัดจำนวน Request ที่ Client สามารถเรียกใช้ API ได้ในช่วงเวลาหนึ่ง เพื่อป้องกันการโดน Spam (DoS) หรือการเดา Password (Brute Force)

### การตั้งค่าใน Template
เราใช้กลยุทธ์ **Fixed Window** ซึ่งตั้งค่าผ่าน `appsettings.json`:

```json
{
  "RateLimit": {
    "PermitLimit": 100,
    "WindowMinutes": 1,
    "QueueLimit": 0
  }
}
```

*   **PermitLimit**: จำนวน Request สูงสุดที่ยอมให้เรียกได้
*   **WindowMinutes**: ช่วงเวลา (นาที) ที่ใช้ในการ Reset จำนวน
*   **QueueLimit**: จำนวน Request ที่ยอมให้รอคิว (แนะนำเป็น 0 เพื่อตัดการทำงานทันที)

### การนำไปใช้งาน (Usage)
เราสร้าง Policy ชื่อว่า `"fixed"` และนำไปใช้ใน Endpoint Groups:

```csharp
var group = app.MapGroup("/products")
               .RequireRateLimiting("fixed");
```

เมื่อ Client เรียกเกินกำหนด ระบบจะส่ง HTTP Status **429 Too Many Requests** กลับไปอัตโนมัติ

---

## ✅ Best Practices สำหรับ Jr. Dev

1.  **Strict Origins**: ใน Production **ห้าม** ใช้ `*` (Allow All) ให้ระบุ Domain ที่อนุญาตจริงๆ เท่านั้น
2.  **Appropriate Limits**: ตั้งค่า Rate Limit ให้เหมาะสมกับลักษณะของ Endpoint (เช่น `/auth/token` ควรตั้งค่าให้ต่ำกว่า `/products`)
3.  **Client Handling**: บอกให้ฝั่ง Frontend เตรียมจัดการ Error 429 เพื่อแสดงข้อความเตือนผู้ใช้เมื่อกดปุ่มเร็วเกินไป

---
*ศึกษาเพิ่มเติม: [Microsoft Docs - Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)*
