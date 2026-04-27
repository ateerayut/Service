using Microsoft.EntityFrameworkCore;
using Service.Domain.Products;

namespace Service.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}