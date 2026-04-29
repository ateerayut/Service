using Service.Application.Common;
using Service.Domain.Orders;

namespace Service.Application.Orders;

public interface IOrderRepository
{
    Task<PagedResult<OrderDto>> List(ListOrdersQuery query, CancellationToken ct);
    Task<OrderDto?> GetById(Guid id, CancellationToken ct);
    Task<Order?> GetEntityById(Guid id, CancellationToken ct);
    Task Add(Order order, CancellationToken ct);
    Task Update(Order order, CancellationToken ct);
    Task Delete(Order order, CancellationToken ct);
}
