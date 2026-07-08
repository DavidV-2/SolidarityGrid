var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApi()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseApi();

app.Run();