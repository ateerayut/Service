using FluentValidation;
using Service.Application.Common;

namespace Service.Application.Orders;

public class AddOrderItemUseCase
{
    private readonly IOrderRepository _repo;
    private readonly IValidator<AddOrderItemCommand> _validator;

    public AddOrderItemUseCase(
        IOrderRepository repo,
        IValidator<AddOrderItemCommand> validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public async Task<OperationResult<bool>> Execute(
        AddOrderItemCommand command,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(command, ct);

        if (!validation.IsValid)
            return OperationResult<bool>.Invalid(validation);

        var order = await _repo.GetEntityById(command.OrderId, ct);

        if (order is null)
            return OperationResult<bool>.Success(false);

        order.AddItem(command.ProductId, command.Quantity, command.UnitPrice);

        await _repo.Update(order, ct);

        return OperationResult<bool>.Success(true);
    }
}
