-- SQL Script for creating Serilog Logs table in PostgreSQL
-- This script matches the default configuration used in the Serilog.Sinks.PostgreSQL

CREATE TABLE IF NOT EXISTS "Logs" (
    "Id" SERIAL PRIMARY KEY,
    "Message" TEXT,
    "MessageTemplate" TEXT,
    "Level" VARCHAR(128),
    "Timestamp" TIMESTAMP WITH TIME ZONE,
    "Exception" TEXT,
    "Properties" JSONB, -- เก็บ Structured Data ในรูปแบบ JSONB เพื่อให้ Search ได้เร็ว
    "LogEvent" JSONB
);

-- เพิ่ม Index สำหรับ Timestamp เพื่อให้ Query ดู Log ย้อนหลังได้รวดเร็ว
CREATE INDEX IF NOT EXISTS "IX_Logs_Timestamp" ON "Logs" ("Timestamp");

-- เพิ่ม Index สำหรับ Level เพื่อให้กรองเฉพาะ Error/Warning ได้เร็ว
CREATE INDEX IF NOT EXISTS "IX_Logs_Level" ON "Logs" ("Level");
