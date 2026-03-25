using Microsoft.Extensions.DependencyInjection;

namespace Service.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // register usecases / validators later

        return services;
    }
}