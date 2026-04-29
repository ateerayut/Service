namespace Service.Application.Orders;

public class DeleteOrderUseCase
{
    private readonly IOrderRepository _repo;

    public DeleteOrderUseCase(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Execute(Guid id, CancellationToken ct)
    {
        var order = await _repo.GetEntityById(id, ct);

        if (order is null)
            return false;

        await _repo.Delete(order, ct);

        return true;
    }
}
