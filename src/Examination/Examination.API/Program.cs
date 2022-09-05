using Examination.API.Installers;
using Examination.Infrastructure;
using Examination.Infrastructure.SeedWork;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Serilog;
using System.Net.Mime;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ===>>> Configuration Setting for Application
var appName = typeof(Program).Assembly.FullName;
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    // Read from your appsettings.json.
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    // Read from your secrets.
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

// Add installers
builder.Services.InstallServicesInAssembly(builder.Configuration);

// Serilog
Log.Logger = new LoggerConfiguration().CreateLogger();
builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Examination.API v1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "Examination.API v2");
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Map healthcheck ui endpoint - default is /healthchecks-ui/
    endpoints.MapHealthChecksUI(options => options.UIPath = "/hc-ui");

    endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
    {
        Predicate = r => r.Name.Contains("self")
    });

    endpoints.MapHealthChecks("/hc-details",
        new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                var result = JsonSerializer.Serialize(
                    new
                    {
                        status = report.Status.ToString(),
                        monitors = report.Entries.Select(e => new { key = e.Key, value = Enum.GetName(typeof(HealthStatus), e.Value.Status) })
                    });
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(result);
            }
        }
    );

    endpoints.MapControllers();
});

try
{
    Log.Information("Starting service");

    // Seeding data into mongoDB
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ExamMongoDbSeeding>>();
        var settings = services.GetRequiredService<IOptions<ExamSettings>>();
        var mongoClient = services.GetRequiredService<IMongoClient>();
        new ExamMongoDbSeeding()
            .SeedAsync(mongoClient, settings, logger)
            .Wait();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}