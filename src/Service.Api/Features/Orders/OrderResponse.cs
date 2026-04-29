using Service.Application.Orders;

namespace Service.Api.Features.Orders;

public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    DateTimeOffset CreateDate,
    List<OrderItemResponse> Items)
{
    public static OrderResponse FromDto(OrderDto order) =>
        new(
            order.Id,
            order.CustomerId,
            order.CreateDate,
            order.Items.Select(i => OrderItemResponse.FromDto(i)).ToList());
}

public record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice)
{
    public static OrderItemResponse FromDto(OrderItemDto item) =>
        new(item.Id, item.ProductId, item.Quantity, item.UnitPrice, item.TotalPrice);
}
