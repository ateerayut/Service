using FluentValidation;
using Service.Domain.Products;

namespace Service.Application.Products;

public class CreateProductUseCase
{
    private readonly IProductRepository _repo;
    private readonly IValidator<CreateProductCommand> _validator;

    public CreateProductUseCase(
        IProductRepository repo,
        IValidator<CreateProductCommand> validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public async Task<OperationResult<Guid>> Execute(
        CreateProductCommand command,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(command, ct);

        if (!validation.IsValid)
            return OperationResult<Guid>.Invalid(validation);

        var product = Product.Create(command.Name, command.Price);

        await _repo.Add(product, ct);

        return OperationResult<Guid>.Success(product.Id);
    }
}
