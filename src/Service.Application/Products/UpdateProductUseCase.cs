using FluentValidation;

namespace Service.Application.Products;

public class UpdateProductUseCase
{
    private readonly IProductRepository _repo;
    private readonly IValidator<UpdateProductCommand> _validator;

    public UpdateProductUseCase(
        IProductRepository repo,
        IValidator<UpdateProductCommand> validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public async Task<OperationResult<bool>> Execute(
        UpdateProductCommand command,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(command, ct);

        if (!validation.IsValid)
            return OperationResult<bool>.Invalid(validation);

        var product = await _repo.GetEntityById(command.Id, ct);

        if (product is null)
            return OperationResult<bool>.Success(false);

        product.Update(command.Name, command.Price);
        await _repo.Update(product, ct);

        return OperationResult<bool>.Success(true);
    }
}
