namespace Service.Application.Products;

public class GetProductByIdUseCase
{
    private readonly IProductRepository _repo;

    public GetProductByIdUseCase(IProductRepository repo)
    {
        _repo = repo;
    }

    public Task<ProductDto?> Execute(Guid id, CancellationToken ct) =>
        _repo.GetById(id, ct);
}
