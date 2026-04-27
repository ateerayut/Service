using Service.Domain.Products;

namespace Service.Application.Products;

public interface IProductRepository
{
    Task Add(Product product, CancellationToken ct);
}