using Service.Application.Products;

namespace Service.Api.Features.Products;

public static class CreateProductEndpoint
{
    public static IEndpointRouteBuilder MapCreateProduct(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/products",
            async (
                CreateProductRequest request,
                CreateProductUseCase uc,
                CancellationToken ct) =>
            {
                var id = await uc.Execute(
                    request.Name,
                    request.Price,
                    ct);

                return Results.Ok(
                    new CreateProductResponse(id));
            })
        .AllowAnonymous();
        return app;
    }
}
//    .RequireAuthorization();