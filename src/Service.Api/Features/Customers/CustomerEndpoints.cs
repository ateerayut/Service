using Service.Application.Customers;
using Service.Application.Common;
using Service.Api.Common;

namespace Service.Api.Features.Customers;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/customers")
            .WithTags("Customers")
            .RequireAuthorization();

        group.MapGet("/",
            async (
                int? page,
                int? pageSize,
                string? search,
                ListCustomersUseCase uc,
                CancellationToken ct) =>
            {
                var result = await uc.Execute(
                    new ListCustomersQuery(
                        page ?? 1,
                        pageSize ?? 20,
                        search),
                    ct);

                return result.Match<IResult>(
                    customers => Results.Ok(
                        PagedResponse<CustomerResponse>.From(
                            customers,
                            CustomerResponse.FromDto)),
                    validation => Results.ValidationProblem(validation.ToDictionary()));
            });

        group.MapGet("/{id:guid}",
            async (
                Guid id,
                GetCustomerByIdUseCase uc,
                CancellationToken ct) =>
            {
                var customer = await uc.Execute(id, ct);

                return customer is null
                    ? Results.NotFound()
                    : Results.Ok(CustomerResponse.FromDto(customer));
            });

        group.MapPost("/",
            async (
                CreateCustomerRequest request,
                CreateCustomerUseCase uc,
                CancellationToken ct) =>
            {
                var result = await uc.Execute(
                    new CreateCustomerCommand(request.Name),
                    ct);

                return result.Match<IResult>(
                    id => Results.Created($"/customers/{id}", new CreateCustomerResponse(id)),
                    validation => Results.ValidationProblem(validation.ToDictionary()));
            });

        group.MapPut("/{id:guid}",
            async (
                Guid id,
                UpdateCustomerRequest request,
                UpdateCustomerUseCase uc,
                CancellationToken ct) =>
            {
                var result = await uc.Execute(
                    new UpdateCustomerCommand(id, request.Name),
                    ct);

                return result.Match<IResult>(
                    updated => updated ? Results.NoContent() : Results.NotFound(),
                    validation => Results.ValidationProblem(validation.ToDictionary()));
            });

        group.MapDelete("/{id:guid}",
            async (
                Guid id,
                DeleteCustomerUseCase uc,
                CancellationToken ct) =>
            {
                var deleted = await uc.Execute(id, ct);

                return deleted ? Results.NoContent() : Results.NotFound();
            });

        return app;
    }
}
