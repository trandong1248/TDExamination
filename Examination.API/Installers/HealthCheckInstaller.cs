using Examination.API.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Examination.API.Installers
{
    public class HealthCheckInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            //Health check
            services.AddHealthChecks()
                    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "Exam API" })
                    .AddCheck<MongoHealthCheck>("MongoDBConnectionCheck", null, new[] { "MongoDb", "Mongo" });

            services.AddHealthChecksUI(opt =>
            {
                opt.SetEvaluationTimeInSeconds(15); //time in seconds between check
                opt.MaximumHistoryEntriesPerEndpoint(60); //mry of caximum histohecks
                opt.SetApiMaxActiveRequests(1); //api requests concurrency

                opt.AddHealthCheckEndpoint("Exam API", "/hc"); //map health check api, default /healthcheck-ui
            })
            .AddInMemoryStorage();
        }
    }
}