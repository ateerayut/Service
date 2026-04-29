using FluentValidation;
using Service.Application.Common;
using Service.Domain.Customers;

namespace Service.Application.Customers;

public class CreateCustomerUseCase
{
    private readonly ICustomerRepository _repo;
    private readonly IValidator<CreateCustomerCommand> _validator;

    public CreateCustomerUseCase(
        ICustomerRepository repo,
        IValidator<CreateCustomerCommand> validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public async Task<OperationResult<Guid>> Execute(
        CreateCustomerCommand command,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(command, ct);

        if (!validation.IsValid)
            return OperationResult<Guid>.Invalid(validation);

        var customer = Customer.Create(command.Name);

        await _repo.Add(customer, ct);

        return OperationResult<Guid>.Success(customer.Id);
    }
}
