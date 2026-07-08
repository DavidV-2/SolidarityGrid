namespace SolidarityGrid.Application;

public static class DependencyInjection
{

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPaymentApplicationService, PaymentApplicationService>();
        return services;
    }
}
