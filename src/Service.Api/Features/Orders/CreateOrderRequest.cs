namespace Service.Api.Features.Orders;

public record CreateOrderRequest(
    Guid CustomerId
);
