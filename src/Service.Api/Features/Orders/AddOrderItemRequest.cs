namespace Service.Api.Features.Orders;

public record AddOrderItemRequest(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
);
