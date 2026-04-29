namespace Service.Application.Customers;

public class GetCustomerByIdUseCase
{
    private readonly ICustomerRepository _repo;

    public GetCustomerByIdUseCase(ICustomerRepository repo)
    {
        _repo = repo;
    }

    public Task<CustomerDto?> Execute(Guid id, CancellationToken ct)
    {
        return _repo.GetById(id, ct);
    }
}
