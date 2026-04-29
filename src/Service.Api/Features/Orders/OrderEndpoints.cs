using Service.Application.Orders;
using Service.Application.Common;
using Service.Api.Common;

namespace Service.Api.Features.Orders;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapGet("/",
            async (
                int? page,
                int? pageSize,
                Guid? customerId,
                ListOrdersUseCase uc,
                CancellationToken ct) =>
            {
                var result = await uc.Execute(
                    new ListOrdersQuery(
                        page ?? 1,
                        pageSize ?? 20,
                        customerId),
                    ct);

                return result.Match<IResult>(
                    orders => Results.Ok(
                        PagedResponse<OrderResponse>.From(
                            orders,
                            OrderResponse.FromDto)),
                    validation => Results.ValidationProblem(validation.ToDictionary()));
            });

        group.MapGet("/{id:guid}",
            async (
                Guid id,
                GetOrderByIdUseCase uc,
                CancellationToken ct) =>
            {
                var order = await uc.Execute(id, ct);

                return order is null
                    ? Results.NotFound()
                    : Results.Ok(OrderResponse.FromDto(order));
            });

        group.MapPost("/",
            async (
                CreateOrderRequest request,
                CreateOrderUseCase uc,
                CancellationToken ct) =>
            {
                var result = await uc.Execute(
                    new CreateOrderCommand(request.CustomerId),
                    ct);

                return result.Match<IResult>(
                    id => Results.Created($"/orders/{id}", new CreateOrderResponse(id)),
                    validation => Results.ValidationProblem(validation.ToDictionary()));
            });

        group.MapPost("/{id:guid}/items",
            async (
                Guid id,
                AddOrderItemRequest request,
                AddOrderItemUseCase uc,
                CancellationToken ct) =>
            {
                var result = await uc.Execute(
                    new AddOrderItemCommand(id, request.ProductId, request.Quantity, request.UnitPrice),
                    ct);

                return result.Match<IResult>(
                    success => success ? Results.NoContent() : Results.NotFound(),
                    validation => Results.ValidationProblem(validation.ToDictionary()));
            });

        group.MapDelete("/{id:guid}",
            async (
                Guid id,
                DeleteOrderUseCase uc,
                CancellationToken ct) =>
            {
                var deleted = await uc.Execute(id, ct);

                return deleted ? Results.NoContent() : Results.NotFound();
            });

        return app;
    }
}
