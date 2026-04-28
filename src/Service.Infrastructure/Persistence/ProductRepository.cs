using Microsoft.EntityFrameworkCore;
using Service.Application.Common;
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

    public async Task<PagedResult<ProductDto>> List(
        ListProductsQuery query,
        CancellationToken ct)
    {
        var productsQuery = _db.Products
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            productsQuery = productsQuery
                .Where(product => product.Name.Contains(search));
        }

        var totalItems = await productsQuery.CountAsync(ct);

        var items = await productsQuery
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(product => new ProductDto(
                product.Id,
                product.Name,
                product.Price))
            .ToListAsync(ct);

        return new PagedResult<ProductDto>(
            items,
            query.Page,
            query.PageSize,
            totalItems);
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
