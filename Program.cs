using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Reflection;
using Serilog.Formatting.Json;
using Serilog.Sinks.File;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
// Enable Serilog debugging, handy for Elasticsearch errors
// Serilog.Debugging.SelfLog.Enable(Console.WriteLine); 

builder.Host.UseSerilog((hostContext, services, configuration) =>
{
    var environmentName = hostContext.HostingEnvironment.EnvironmentName;
    configuration
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(ConfigureElasticSink(hostContext.Configuration, environmentName))
        .Enrich.WithProperty("Environment", environmentName)
        .ReadFrom.Configuration(hostContext.Configuration);
});

var app = builder.Build();
app.MapGet("/", () => $"Im in {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
app.MapGet("/oops", () =>
{
    throw new NullReferenceException("Oops");
});

app.Run();

static ElasticsearchSinkOptions ConfigureElasticSink(IConfiguration configuration, string environment) =>
    new(new Uri(configuration["ElasticConfiguration:Uri"]))
    {
        IndexFormat = $"serilogtest-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
        AutoRegisterTemplate = true,
        // Prevent errors creating index with ElasticSearch 8, see https://github.com/serilog-contrib/serilog-sinks-elasticsearch/issues/375
        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
        TypeName = null // Prevent 
    };
