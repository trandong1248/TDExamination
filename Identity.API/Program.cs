using HealthChecks.UI.Client;
using Identity.API.Configuration;
using Identity.API.Database;
using Identity.API.Extensions;
using Identity.API.Installers;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Serilog;
using System.Net.Mime;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ===>>> Configuration Setting for Application
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

// ===>>> Configure logger of serilog for application
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .Enrich.WithProperty("ApplicationContext", typeof(Program).Assembly.FullName)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(@"logs/log-.txt", rollingInterval: RollingInterval.Day, shared: true)
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Seeding data 'PersistedGrantDbContext' & 'ApplicationDbContext' & 'ConfigurationDbContext'
// First DB:  PersistedGrantDbContext: =>> DbContext for the IdentityServer operational data.
// Second DB: ApplicationDbContext: =>> DbContext for IdenityUser
// Third DB: ConfigurationDbContext: =>> DbContext for SSO information: ClientId, Client, IdentityResource, ApiResource, ApiScope ...

var isDatabaseInitialized = builder.Configuration.GetValue<bool>("DatabaseInitialization");
if (!isDatabaseInitialized)
{
    try
    {
        Log.Information("Applying migrations ({ApplicationContext})...", typeof(Program).Assembly.FullName);
        // =>>> Apply DB
        app.MigrateDbContext<PersistedGrantDbContext>((_, __) => { })
            .MigrateDbContext<ApplicationDbContext>((context, services) =>
            {
                var env = services.GetService<IWebHostEnvironment>();
                var logger = services.GetService<ILogger<ApplicationDbContextSeed>>();
                var settings = services.GetService<IOptions<AppSettings>>();

                new ApplicationDbContextSeed()
                    .SeedAsync(context, env, logger, settings)
                    .Wait();
            })
            .MigrateDbContext<ConfigurationDbContext>((context, services) =>
            {
                new ConfigurationDbContextSeed()
                    .SeedAsync(context, builder.Configuration)
                    .Wait();
            });
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Migration DB has been occupied error ({ApplicationContext})!", typeof(Program).Assembly.FullName);
    }
    finally
    {
        Log.CloseAndFlush();
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity.API v1"));
}



app.UseHttpsRedirection();

app.UseAuthentication();
app.UseRouting();
app.UseStaticFiles();
app.UseIdentityServer();
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

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
        );
});
try
{
    Log.Information("Starting web host ({ApplicationContext})...", typeof(Program).Assembly.FullName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", typeof(Program).Assembly.FullName);
    throw;
}
finally
{
    Log.CloseAndFlush();
}