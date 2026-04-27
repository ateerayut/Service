using Service.Domain.Products;

namespace Service.Application.Products;

public class CreateProductUseCase
{
    private readonly IProductRepository _repo;

    public CreateProductUseCase(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> Execute(
        string name,
        decimal price,
        CancellationToken ct)
    {
        var product = Product.Create(name, price);

        await _repo.Add(product, ct);

        return product.Id;
    }
}