using Service.Application.Products;

namespace Service.Api.Features.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products")
            .WithTags("Products")
            .RequireAuthorization();

        group.MapGet("/",
            async (
                int? page,
                int? pageSize,
                string? search,
                ListProductsUseCase uc,
                CancellationToken ct) =>
            {
                var result = await uc.Execute(
                    new ListProductsQuery(
                        page ?? 1,
                        pageSize ?? 20,
                        search),
                    ct);

                return result.Match<IResult>(
                    products => Results.Ok(
                        PagedResponse<ProductResponse>.From(
                            products,
                            ProductResponse.FromDto)),
                    validation => Results.ValidationProblem(validation.ToDictionary()));
            });

        group.MapGet("/{id:guid}",
            async (
                Guid id,
                GetProductByIdUseCase uc,
                CancellationToken ct) =>
            {
                var product = await uc.Execute(id, ct);

                return product is null
                    ? Results.NotFound()
                    : Results.Ok(ProductResponse.FromDto(product));
            });

        group.MapPost("/",
            async (
                CreateProductRequest request,
                CreateProductUseCase uc,
                CancellationToken ct) =>
            {
                var result = await uc.Execute(
                    new CreateProductCommand(request.Name, request.Price),
                    ct);

                return result.Match<IResult>(
                    id => Results.Created($"/products/{id}", new CreateProductResponse(id)),
                    validation => Results.ValidationProblem(validation.ToDictionary()));
            });

        group.MapPut("/{id:guid}",
            async (
                Guid id,
                UpdateProductRequest request,
                UpdateProductUseCase uc,
                CancellationToken ct) =>
            {
                var result = await uc.Execute(
                    new UpdateProductCommand(id, request.Name, request.Price),
                    ct);

                return result.Match<IResult>(
                    updated => updated ? Results.NoContent() : Results.NotFound(),
                    validation => Results.ValidationProblem(validation.ToDictionary()));
            });

        group.MapDelete("/{id:guid}",
            async (
                Guid id,
                DeleteProductUseCase uc,
                CancellationToken ct) =>
            {
                var deleted = await uc.Execute(id, ct);

                return deleted ? Results.NoContent() : Results.NotFound();
            });

        return app;
    }
}
