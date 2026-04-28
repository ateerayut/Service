# Migration Documentation Index

This workspace contains comprehensive documentation for database migrations using Entity Framework Core. Below is a guide to all migration-related documents and resources.

## 📚 Documentation Files

### 1. **README.md** (Main Project Documentation)
**Location**: `README.md`

Contains the main project documentation including:
- Full architecture overview
- Database Migrations section (lines 514-900+)
- Step-by-step migration creation guide
- Complete workflow examples
- Best practices for production deployments

**Read this for**: Comprehensive understanding of migrations in the context of the full project.

---

### 2. **MIGRATION_SUMMARY.md** (Applied Migration Details)
**Location**: `MIGRATION_SUMMARY.md`

Details about the migration that has already been applied to your database:
- Migration: `AddCustomersAndOrders` (20260428073737)
- Current database schema
- Relationship diagrams
- File locations and configurations

**Read this for**: Understanding what's currently in your database and the applied migration history.

---

### 3. **MIGRATION_QUICK_REFERENCE.md** (Copy-Paste Scripts)
**Location**: `MIGRATION_QUICK_REFERENCE.md`

Quick reference guide with copy-paste PowerShell commands:
- All common migration commands
- 6 migration patterns with complete scripts
- Production deployment workflow
- Troubleshooting guide

**Read this for**: Quick copy-paste commands when creating migrations.

---

### 4. **MIGRATION_SQL_EXAMPLES.md** (SQL Reference)
**Location**: `MIGRATION_SQL_EXAMPLES.md`

SQL examples showing what gets generated for each migration scenario:
- Current database schema
- 10 example migrations with up/down SQL
- SQL validation commands
- Tips and best practices

**Read this for**: Understanding the SQL being generated and validating migrations.

---

### 5. **This Document** (Navigation Guide)
**Location**: `MIGRATION_DOCUMENTATION_INDEX.md`

Navigation guide for all migration documentation.

---

## 🚀 Quick Start by Scenario

### I want to understand migrations from scratch
1. Read: `README.md` → Database Migrations section
2. Skim: `MIGRATION_SUMMARY.md` → Current Data Model
3. Reference: `MIGRATION_SQL_EXAMPLES.md` → Current Database Schema

### I need to add a column to an existing table
1. Go to: `MIGRATION_QUICK_REFERENCE.md` → Pattern 1
2. Execute the commands provided
3. Reference: `MIGRATION_SQL_EXAMPLES.md` → Example 1 for expected SQL

### I need to create a brand new entity/table
1. Go to: `MIGRATION_QUICK_REFERENCE.md` → Pattern 2
2. Execute the commands provided
3. Reference: `MIGRATION_SQL_EXAMPLES.md` → Example 3 for expected SQL

### I need to add relationships between entities
1. Go to: `MIGRATION_QUICK_REFERENCE.md` → Pattern 3
2. Execute the commands provided
3. Reference: `MIGRATION_SQL_EXAMPLES.md` → Example 4 for expected SQL

### I'm deploying to production
1. Read: `README.md` → Production Migration Best Practices
2. Follow: `MIGRATION_QUICK_REFERENCE.md` → Production Deployment Workflow
3. Reference: `MIGRATION_SQL_EXAMPLES.md` → SQL Validation Commands

### Something went wrong with my migration
1. Consult: `MIGRATION_QUICK_REFERENCE.md` → Troubleshooting
2. Reference: `README.md` → Useful Migration Commands
3. Check: `MIGRATION_SQL_EXAMPLES.md` → for expected behavior

---

## 📋 Current Project Structure

```
src/
├── Service.Api/                      # API layer
├── Service.Application/              # Business logic (use cases, validators)
├── Service.Domain/                   # Core domain entities
│   ├── Customers/
│   │   └── Customer.cs               # ← Domain entity
│   ├── Orders/
│   │   ├── Order.cs                  # ← Domain entity
│   │   └── OrderItem.cs              # ← Domain entity
│   └── Products/
│       └── Product.cs                # ← Domain entity
└── Service.Infrastructure/           # Database & persistence
    └── Persistence/
        ├── AppDbContext.cs           # ← Entity configuration (DbContext)
        └── *Repository.cs            # ← Repository implementations
    └── Migrations/
        ├── 20260326133244_Init.cs
        ├── 20260428073737_AddCustomersAndOrders.cs  ← Latest applied
        └── <future migrations>

tests/
├── Service.UnitTests/
└── Service.IntegrationTests/
```

---

## 🔄 Migration Workflow Overview

```
┌─────────────────────────────────────────────────────────┐
│ 1. Modify Domain Entity                                 │
│    (src/Service.Domain/*/Entity.cs)                     │
└────────────────┬────────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────────────┐
│ 2. Update DbContext Configuration                       │
│    (src/Service.Infrastructure/Persistence/             │
│     AppDbContext.cs - OnModelCreating method)           │
└────────────────┬────────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────────────┐
│ 3. Create Migration                                     │
│    $ dotnet ef migrations add <MigrationName>           │
│    → Generates: src/Service.Infrastructure/Migrations/  │
│                 <timestamp>_<MigrationName>.cs          │
└────────────────┬────────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────────────┐
│ 4. Review Generated Migration                           │
│    Check Up() and Down() methods in generated file      │
└────────────────┬────────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────────────┐
│ 5. Apply Migration to Database                          │
│    $ dotnet ef database update                          │
│    → Executes migration and updates __EFMigrationsHistory
└────────────────┬────────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────────────┐
│ 6. Verify Changes                                       │
│    Check database schema with SQL commands              │
└────────────────┬────────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────────────┐
│ 7. Commit to Git                                        │
│    $ git add .                                          │
│    $ git commit -m "feat: your migration description"   │
│    $ git push origin <branch>                           │
└─────────────────────────────────────────────────────────┘
```

---

## 📊 Current Database State

### Tables
- ✅ **Products** - Created in `20260326133244_Init`
- ✅ **Customers** - Created in `20260428073737_AddCustomersAndOrders`
- ✅ **Orders** - Created in `20260428073737_AddCustomersAndOrders`
- ✅ **OrderItems** - Created in `20260428073737_AddCustomersAndOrders`

### Relationships
- `Customers` (1) ↔ (Many) `Orders` → OnDelete: Restrict
- `Orders` (1) ↔ (Many) `OrderItems` → OnDelete: Cascade
- `OrderItems` (Many) ↔ (1) `Products` → OnDelete: Restrict

### Indexes
- `IX_Orders_CustomerId`
- `IX_OrderItems_OrderId`
- `IX_OrderItems_ProductId`

---

## 🛠️ Essential Commands Cheat Sheet

```powershell
# Change to project directory
cd D:\MinimalTemplate\service-template-workspace

# CREATE a new migration
dotnet ef migrations add MigrationName `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

# APPLY migrations to database
dotnet ef database update `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

# ROLLBACK to specific migration
dotnet ef database update MigrationName `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

# LIST all migrations
dotnet ef migrations list `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

# REMOVE last unapplied migration
dotnet ef migrations remove `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

# GENERATE SQL script
dotnet ef migrations script `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj > migration.sql
```

---

## 📖 Key Concepts

### Domain Entity
- Located in: `src/Service.Domain/<Feature>/`
- Pure C# class representing business concept
- Encapsulates business rules and validations
- **Must not** depend on EF Core or database

### DbContext
- Located in: `src/Service.Infrastructure/Persistence/AppDbContext.cs`
- Configures how entities map to database tables
- Defines relationships and constraints
- Entry point for EF Core

### Migration
- Located in: `src/Service.Infrastructure/Migrations/`
- Generated C# code representing schema changes
- Contains `Up()` (apply) and `Down()` (rollback) methods
- **Should never be edited** after applied

### Migration History
- Tracked in: `__EFMigrationsHistory` table
- Records which migrations have been applied
- Used to determine which migrations need to run

---

## ⚠️ Important Rules

1. **Don't edit applied migrations** - Create new migration to fix
2. **Always review generated SQL** - Especially before production
3. **Provide defaults for required columns** - When adding to existing tables with data
4. **Test on staging first** - Before applying to production
5. **Backup production database** - Before running any migrations
6. **Commit migrations to Git** - Keep in version control
7. **Use meaningful names** - `AddEmailToCustomer` not `Update`

---

## 🎓 Learning Path

**Beginner**
1. Read: `README.md` → Database Migrations (Architecture Pattern)
2. Understand: `MIGRATION_SUMMARY.md` → Current Data Model
3. Try: Add a simple column to existing table (Pattern 1 in Quick Reference)

**Intermediate**
1. Read: `README.md` → How to Create a Migration (full section)
2. Try: Create new entity and table (Pattern 2 in Quick Reference)
3. Understand: `MIGRATION_SQL_EXAMPLES.md` → What SQL is being generated

**Advanced**
1. Read: `README.md` → Production Migration Best Practices
2. Study: `MIGRATION_SQL_EXAMPLES.md` → Complex scenarios
3. Reference: `MIGRATION_QUICK_REFERENCE.md` → Production Deployment Workflow

---

## 🔍 File References

| File | Purpose | Read Time |
|------|---------|-----------|
| README.md | Main documentation | 15-20 min |
| MIGRATION_SUMMARY.md | Applied migration details | 5 min |
| MIGRATION_QUICK_REFERENCE.md | Copy-paste scripts | 10 min |
| MIGRATION_SQL_EXAMPLES.md | SQL reference | 10 min |
| MIGRATION_DOCUMENTATION_INDEX.md | This file | 5 min |

---

## 🤔 FAQ

**Q: I created an entity but don't know how to migrate it**
A: See Quick Reference → Pattern 2: Create New Entity and Table

**Q: I added a column but the migration failed**
A: See Quick Reference → Troubleshooting section

**Q: How do I rollback a migration?**
A: See Cheat Sheet → ROLLBACK command or Pattern 1 in examples

**Q: Can I edit a migration I haven't applied yet?**
A: Yes, edit the file directly or use `dotnet ef migrations remove` to delete and recreate

**Q: Can I edit an already-applied migration?**
A: No. Create a new migration to fix or rollback then fix

**Q: How do I deploy migrations to production?**
A: Follow Production Deployment Workflow in Quick Reference

---

## 📞 Support Resources

- **EF Core Official Documentation**: https://docs.microsoft.com/en-us/ef/core/
- **EF Core Migrations Reference**: https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/
- **PostgreSQL Documentation**: https://www.postgresql.org/docs/
- **SQL Server Documentation**: https://learn.microsoft.com/en-us/sql/

---

## 📝 Document History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-04-28 | Initial migration documentation suite created |

---

## Next Steps

1. ✅ **Understand Current State** → Read MIGRATION_SUMMARY.md
2. ✅ **Learn Migration Process** → Read README.md Database Migrations section
3. 👉 **Try Your First Migration** → Use MIGRATION_QUICK_REFERENCE.md Pattern 1
4. 📚 **Reference SQL Examples** → Consult MIGRATION_SQL_EXAMPLES.md when needed

---

## Contributors

Documentation maintained in: `D:\MinimalTemplate\service-template-workspace`

Repository: https://github.com/ateerayut/Service

---

**Last Updated**: 2026-04-28  
**Status**: ✅ Complete and tested
