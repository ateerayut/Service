using Service.Application.Products;

namespace Service.Api.Features.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products")
            .WithTags("Products");

        group.MapGet("/",
            async (
                ListProductsUseCase uc,
                CancellationToken ct) =>
            {
                var products = await uc.Execute(ct);

                return Results.Ok(products.Select(ProductResponse.FromDto));
            })
        .AllowAnonymous();

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
            })
        .AllowAnonymous();

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
            })
        .AllowAnonymous();

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
            })
        .AllowAnonymous();

        group.MapDelete("/{id:guid}",
            async (
                Guid id,
                DeleteProductUseCase uc,
                CancellationToken ct) =>
            {
                var deleted = await uc.Execute(id, ct);

                return deleted ? Results.NoContent() : Results.NotFound();
            })
        .AllowAnonymous();

        return app;
    }
}
