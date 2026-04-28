namespace Service.Application.Products;

public class DeleteProductUseCase
{
    private readonly IProductRepository _repo;

    public DeleteProductUseCase(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Execute(Guid id, CancellationToken ct)
    {
        var product = await _repo.GetEntityById(id, ct);

        if (product is null)
            return false;

        await _repo.Delete(product, ct);

        return true;
    }
}
