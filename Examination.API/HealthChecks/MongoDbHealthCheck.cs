using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Examination.API.HealthChecks
{
    public class MongoHealthCheck : IHealthCheck
    {
        private IMongoDatabase _db { get; set; }
        public MongoClient _mongoClient { get; set; }

        public MongoHealthCheck(IConfiguration configuration)
        {
            var user = configuration.GetValue<string>("DatabaseSettings:User");
            var password = configuration.GetValue<string>("DatabaseSettings:Password");
            var server = configuration.GetValue<string>("DatabaseSettings:Server");
            var databaseName = configuration.GetValue<string>("DatabaseSettings:DatabaseName");
            var mongodbConnectionString = "mongodb://" + user + ":" + password + "@" + server + "/" + databaseName + "?authSource=admin";

            _mongoClient = new MongoClient(mongodbConnectionString);
            _db = _mongoClient.GetDatabase(databaseName);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var healthCheckResultHealthy = await CheckMongoDBConnectionAsync();

            if (healthCheckResultHealthy)
            {
                return HealthCheckResult.Healthy("MongoDB health check success");
            }
            else
            {
                // Send email to administrator here to check mongodb service
            }

            return HealthCheckResult.Unhealthy("MongoDB health check failure"); ;
        }

        private async Task<bool> CheckMongoDBConnectionAsync()
        {
            try
            {
                await _db.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}