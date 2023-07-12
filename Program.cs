using System.Reflection;
using Elastic.Apm.NetCoreAll;
using Serilog;
using Serilog.Context;
using Serilog.Sinks.Elasticsearch;
using Serilog.ThrowContext;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();

// Enable Serilog debugging, handy for Elasticsearch errors
// Serilog.Debugging.SelfLog.Enable(Console.WriteLine); 

builder.Host.UseAllElasticApm();
builder.Host.UseSerilog((hostContext, serviceProvider, configuration) =>
{
    var environmentName = hostContext.HostingEnvironment.EnvironmentName;
    configuration
        .ReadFrom.Configuration(hostContext.Configuration)
        .Enrich.With<ThrowContextEnricher>()
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProperty("Environment", environmentName)
        .Enrich.WithProperty("Version", Assembly.GetEntryAssembly().GetName().Version)
        .WriteTo.Console(/*new CompactJsonFormatter()*/)
        .WriteTo.Elasticsearch(ConfigureElasticSink(hostContext.Configuration, environmentName));
});


var app = builder.Build();
app.Use(async (httpContext, next) =>
    {
        using (LogContext.PushProperty("User-Agent", httpContext.Request.Headers["User-Agent"]))
            await next(httpContext);
    }
);
app.MapGet("/", () => $"Im in {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
app.MapGet("/oops", (IDiagnosticContext diagnosticContext) =>
{
    // Will be included in all of the logs in this scope (including the Exception log!)
    using var _ = LogContext.PushProperty("SomeProperty", "900");

    var s = "Een woord of iets";
    
    Log.Information("Sup dude");
    throw new NullReferenceException("Oops");
});

app.MapGet("/context-properties", (context) =>
{
    Log.ForContext("SomeId", "Products-1-A")
        .Information("Handled product");
    throw new NullReferenceException("Oops");
});

app.MapGet("/inline-properties", () =>
{
    string productId = "products-1-A";
    Log.Information("Handled product {productId}", productId);

    var product = new {name = "SomeProduct", price = 30.20};
    Log.Information("Handled product {@product}", product); // Also indexes name and price so they can be searched!
    throw new NullReferenceException("Oops");
});

app.MapGet("/log-spam-woop-woop", () =>
{
    // Create random logs
    var random = new Random(42) ;
    for (int i = 0; i < 100_000; i++)
    {
        var rand = random.Next(0, 4);
        if (rand == 0)
            Log.Information("Spam {i}", i);
        else if (rand == 1)
            Log.Warning("Spam {i}", i);
        else if (rand == 2)
            Log.Error("Spam {i}", i);
        else if (rand == 3)
            Log.Fatal("Spam {i}", i);
    }
    return "Done!";
});

app.Run();

static ElasticsearchSinkOptions ConfigureElasticSink(IConfiguration configuration, string environment) =>
    new(new Uri(configuration["ElasticConfiguration:Uri"]))
    {
        IndexFormat = $"serilogtest-{environment?.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
        AutoRegisterTemplate = true,
        // Prevent errors creating index with ElasticSearch 8, see https://github.com/serilog-contrib/serilog-sinks-elasticsearch/issues/375
        TypeName = null
    };
