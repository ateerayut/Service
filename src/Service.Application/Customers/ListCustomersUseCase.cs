using FluentValidation;
using Service.Application.Common;

namespace Service.Application.Customers;

public class ListCustomersUseCase
{
    private readonly ICustomerRepository _repo;
    private readonly IValidator<ListCustomersQuery> _validator;

    public ListCustomersUseCase(
        ICustomerRepository repo,
        IValidator<ListCustomersQuery> validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public async Task<OperationResult<Common.PagedResult<CustomerDto>>> Execute(
        ListCustomersQuery query,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(query, ct);

        if (!validation.IsValid)
            return OperationResult<Common.PagedResult<CustomerDto>>.Invalid(validation);

        var result = await _repo.List(query, ct);

        return OperationResult<Common.PagedResult<CustomerDto>>.Success(result);
    }
}
