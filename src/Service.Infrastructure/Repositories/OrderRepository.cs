using Microsoft.EntityFrameworkCore;
using Service.Application.Common;
using Service.Application.Orders;
using Service.Domain.Orders;
using Service.Infrastructure.Persistence;

namespace Service.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<OrderDto>> List(
        ListOrdersQuery query,
        CancellationToken ct)
    {
        var ordersQuery = _db.Orders
            .AsNoTracking();

        if (query.CustomerId.HasValue && query.CustomerId.Value != Guid.Empty)
        {
            ordersQuery = ordersQuery
                .Where(order => order.CustomerId == query.CustomerId.Value);
        }

        var totalItems = await ordersQuery.CountAsync(ct);

        var items = await ordersQuery
            .OrderByDescending(order => order.CreateDate)
            .ThenBy(order => order.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(order => order.Items)
            .Select(order => new OrderDto(
                order.Id,
                order.CustomerId,
                order.CreateDate,
                order.Items.Select(item => new OrderItemDto(
                    item.Id,
                    item.ProductId,
                    item.Quantity,
                    item.UnitPrice,
                    item.TotalPrice)).ToList()))
            .ToListAsync(ct);

        return new PagedResult<OrderDto>(
            items,
            query.Page,
            query.PageSize,
            totalItems);
    }

    public async Task<OrderDto?> GetById(Guid id, CancellationToken ct)
    {
        return await _db.Orders
            .AsNoTracking()
            .Where(order => order.Id == id)
            .Include(order => order.Items)
            .Select(order => new OrderDto(
                order.Id,
                order.CustomerId,
                order.CreateDate,
                order.Items.Select(item => new OrderItemDto(
                    item.Id,
                    item.ProductId,
                    item.Quantity,
                    item.UnitPrice,
                    item.TotalPrice)).ToList()))
            .SingleOrDefaultAsync(ct);
    }

    public Task<Order?> GetEntityById(Guid id, CancellationToken ct)
    {
        return _db.Orders
            .Include(order => order.Items)
            .SingleOrDefaultAsync(order => order.Id == id, ct);
    }

    public async Task Add(Order order, CancellationToken ct)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
    }

    public Task Update(Order order, CancellationToken ct)
    {
        _db.Orders.Update(order);
        return _db.SaveChangesAsync(ct);
    }

    public Task Delete(Order order, CancellationToken ct)
    {
        _db.Orders.Remove(order);
        return _db.SaveChangesAsync(ct);
    }
}
