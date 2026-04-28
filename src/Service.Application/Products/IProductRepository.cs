using Service.Domain.Products;

namespace Service.Application.Products;

public interface IProductRepository
{
    Task<IReadOnlyList<ProductDto>> List(CancellationToken ct);
    Task<ProductDto?> GetById(Guid id, CancellationToken ct);
    Task<Product?> GetEntityById(Guid id, CancellationToken ct);
    Task Add(Product product, CancellationToken ct);
    Task Update(Product product, CancellationToken ct);
    Task Delete(Product product, CancellationToken ct);
}
