namespace Service.Application.Customers;

public class DeleteCustomerUseCase
{
    private readonly ICustomerRepository _repo;

    public DeleteCustomerUseCase(ICustomerRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Execute(Guid id, CancellationToken ct)
    {
        var customer = await _repo.GetEntityById(id, ct);

        if (customer is null)
            return false;

        await _repo.Delete(customer, ct);

        return true;
    }
}
