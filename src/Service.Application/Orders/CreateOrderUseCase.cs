using FluentValidation;
using Service.Application.Common;
using Service.Domain.Orders;

namespace Service.Application.Orders;

public class CreateOrderUseCase
{
    private readonly IOrderRepository _repo;
    private readonly IValidator<CreateOrderCommand> _validator;

    public CreateOrderUseCase(
        IOrderRepository repo,
        IValidator<CreateOrderCommand> validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public async Task<OperationResult<Guid>> Execute(
        CreateOrderCommand command,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(command, ct);

        if (!validation.IsValid)
            return OperationResult<Guid>.Invalid(validation);

        var order = Order.Create(command.CustomerId);

        await _repo.Add(order, ct);

        return OperationResult<Guid>.Success(order.Id);
    }
}
