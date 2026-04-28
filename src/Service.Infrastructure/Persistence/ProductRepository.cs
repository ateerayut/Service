using Microsoft.EntityFrameworkCore;
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

    public async Task<IReadOnlyList<ProductDto>> List(CancellationToken ct)
    {
        return await _db.Products
            .AsNoTracking()
            .OrderBy(product => product.Name)
            .Select(product => new ProductDto(
                product.Id,
                product.Name,
                product.Price))
            .ToListAsync(ct);
    }

    public async Task<ProductDto?> GetById(Guid id, CancellationToken ct)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(product => product.Id == id)
            .Select(product => new ProductDto(
                product.Id,
                product.Name,
                product.Price))
            .SingleOrDefaultAsync(ct);
    }

    public Task<Product?> GetEntityById(Guid id, CancellationToken ct)
    {
        return _db.Products
            .SingleOrDefaultAsync(product => product.Id == id, ct);
    }

    public async Task Add(Product product, CancellationToken ct)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
    }

    public Task Update(Product product, CancellationToken ct)
    {
        _db.Products.Update(product);
        return _db.SaveChangesAsync(ct);
    }

    public Task Delete(Product product, CancellationToken ct)
    {
        _db.Products.Remove(product);
        return _db.SaveChangesAsync(ct);
    }
}
