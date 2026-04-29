using FluentValidation;
using Service.Application.Common;

namespace Service.Application.Orders;

public class ListOrdersUseCase
{
    private readonly IOrderRepository _repo;
    private readonly IValidator<ListOrdersQuery> _validator;

    public ListOrdersUseCase(
        IOrderRepository repo,
        IValidator<ListOrdersQuery> validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public async Task<OperationResult<Common.PagedResult<OrderDto>>> Execute(
        ListOrdersQuery query,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(query, ct);

        if (!validation.IsValid)
            return OperationResult<Common.PagedResult<OrderDto>>.Invalid(validation);

        var result = await _repo.List(query, ct);

        return OperationResult<Common.PagedResult<OrderDto>>.Success(result);
    }
}
