namespace SolidarityGrid.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<SolidarityGridDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(SolidarityGridDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<SolidarityGridDbContext>());

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.Configure<NodeConfig>(configuration.GetSection("Node"));

        services.AddSingleton<NodeRegistry>();

        services.AddHttpClient();

        services.AddHostedService<PaymentProcessorHostedService>();
        services.AddHostedService<NodeHeartbeatService>();

        return services;
    }
}