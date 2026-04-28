using FluentValidation;
using Service.Application.Common;

namespace Service.Application.Products;

public class ListProductsUseCase
{
    private readonly IProductRepository _repo;
    private readonly IValidator<ListProductsQuery> _validator;

    public ListProductsUseCase(
        IProductRepository repo,
        IValidator<ListProductsQuery> validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public async Task<OperationResult<PagedResult<ProductDto>>> Execute(
        ListProductsQuery query,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(query, ct);

        if (!validation.IsValid)
            return OperationResult<PagedResult<ProductDto>>.Invalid(validation);

        var products = await _repo.List(query, ct);

        return OperationResult<PagedResult<ProductDto>>.Success(products);
    }
}
