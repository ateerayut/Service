namespace Service.Application.Products;

public class ListProductsUseCase
{
    private readonly IProductRepository _repo;

    public ListProductsUseCase(IProductRepository repo)
    {
        _repo = repo;
    }

    public Task<IReadOnlyList<ProductDto>> Execute(CancellationToken ct) =>
        _repo.List(ct);
}
