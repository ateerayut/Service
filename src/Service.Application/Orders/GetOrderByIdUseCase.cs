namespace Service.Application.Orders;

public class GetOrderByIdUseCase
{
    private readonly IOrderRepository _repo;

    public GetOrderByIdUseCase(IOrderRepository repo)
    {
        _repo = repo;
    }

    public Task<OrderDto?> Execute(Guid id, CancellationToken ct)
    {
        return _repo.GetById(id, ct);
    }
}
