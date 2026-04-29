using FluentValidation;
using Service.Application.Common;

namespace Service.Application.Customers;

public class UpdateCustomerUseCase
{
    private readonly ICustomerRepository _repo;
    private readonly IValidator<UpdateCustomerCommand> _validator;

    public UpdateCustomerUseCase(
        ICustomerRepository repo,
        IValidator<UpdateCustomerCommand> validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public async Task<OperationResult<bool>> Execute(
        UpdateCustomerCommand command,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(command, ct);

        if (!validation.IsValid)
            return OperationResult<bool>.Invalid(validation);

        var customer = await _repo.GetEntityById(command.Id, ct);

        if (customer is null)
            return OperationResult<bool>.Success(false);

        customer.Update(command.Name);

        await _repo.Update(customer, ct);

        return OperationResult<bool>.Success(true);
    }
}
