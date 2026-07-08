namespace SolidarityGrid.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddDbContext<SolidarityGridDbContext>(options =>
        {
            options.UseSqlServer(
               configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IPaymentRepository, PaymentRepository>();


        return services;
    }
}
