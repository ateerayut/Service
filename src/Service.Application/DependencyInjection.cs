using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Service.Application.Products;

namespace Service.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly);

        // ⭐ IMPORTANT
        services.AddScoped<CreateProductUseCase>();

        return services;
    }
}