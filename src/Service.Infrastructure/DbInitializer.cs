using Microsoft.EntityFrameworkCore;
using Service.Domain.Customers;
using Service.Domain.Products;
using Service.Infrastructure.Persistence;

namespace Service.Infrastructure;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // 1. Ensure database is created and migrations are applied
        await context.Database.MigrateAsync();

        // 2. Seed Products if empty
        if (!await context.Products.AnyAsync())
        {
            var products = new List<Product>
            {
                Product.Create("Mechanical Keyboard", 2500),
                Product.Create("Wireless Mouse", 1200),
                Product.Create("UltraWide Monitor", 15000),
                Product.Create("USB-C Hub", 850),
                Product.Create("Webcam 4K", 3500)
            };

            context.Products.AddRange(products);
        }

        // 3. Seed Customers if empty
        if (!await context.Customers.AnyAsync())
        {
            var customers = new List<Customer>
            {
                Customer.Create("John Doe"),
                Customer.Create("Jane Smith"),
                Customer.Create("Somsak Rakdee")
            };

            context.Customers.AddRange(customers);
        }

        await context.SaveChangesAsync();
    }
}
