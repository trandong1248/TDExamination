using Identity.API.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Identity.API.Installers
{
    public class HealthCheckInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            //Health check
            services.AddHealthChecks()
                    .AddCheck(name:"self", () => HealthCheckResult.Healthy(), tags: new[] { "Identity API" })
                    .AddCheck<SqlConnectionHealthCheck>(name:"SqlDBConnectionCheck", tags: new[] { "sqlServer", "sql" });

            services.AddHealthChecksUI(opt =>
            {
                opt.SetHeaderText("Overview - Health Checks Status");
                opt.SetEvaluationTimeInSeconds(15); //time in seconds between check
                opt.MaximumHistoryEntriesPerEndpoint(60); //mry of caximum histohecks
                opt.SetApiMaxActiveRequests(2); //api requests concurrency, default 3

                opt.AddHealthCheckEndpoint("Identity API", "/hc"); //map health check api, default /healthcheck-ui
            })
            .AddInMemoryStorage();
        }
    }
}