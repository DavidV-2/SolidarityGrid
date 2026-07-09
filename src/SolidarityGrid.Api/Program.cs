var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApi()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();

var dbContext = scope.ServiceProvider.GetRequiredService<SolidarityGridDbContext>();

await dbContext.Database.MigrateAsync();

app.UseApi();

app.Run();