var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

app.Run();
