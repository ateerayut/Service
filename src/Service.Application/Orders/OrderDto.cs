namespace Service.Application.Orders;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    DateTimeOffset CreateDate,
    List<OrderItemDto> Items);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
