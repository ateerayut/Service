using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Service.Application.Customers;
using Service.Application.Orders;
using Service.Application.Products;

namespace Service.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly);

        // Product use cases
        services.AddScoped<CreateProductUseCase>();
        services.AddScoped<ListProductsUseCase>();
        services.AddScoped<GetProductByIdUseCase>();
        services.AddScoped<UpdateProductUseCase>();
        services.AddScoped<DeleteProductUseCase>();

        // Customer use cases
        services.AddScoped<CreateCustomerUseCase>();
        services.AddScoped<ListCustomersUseCase>();
        services.AddScoped<GetCustomerByIdUseCase>();
        services.AddScoped<UpdateCustomerUseCase>();
        services.AddScoped<DeleteCustomerUseCase>();

        // Order use cases
        services.AddScoped<CreateOrderUseCase>();
        services.AddScoped<ListOrdersUseCase>();
        services.AddScoped<GetOrderByIdUseCase>();
        services.AddScoped<AddOrderItemUseCase>();
        services.AddScoped<DeleteOrderUseCase>();

        return services;
    }
}
