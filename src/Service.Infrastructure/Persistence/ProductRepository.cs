using Service.Application.Products;
using Service.Domain.Products;
using Service.Infrastructure.Persistence;

namespace Service.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task Add(Product product, CancellationToken ct)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
    }
}