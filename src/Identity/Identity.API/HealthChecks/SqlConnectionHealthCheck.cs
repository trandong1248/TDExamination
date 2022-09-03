using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data.Common;
using System.Data.SqlClient;

namespace Identity.API.HealthChecks
{
    public class SqlConnectionHealthCheck : IHealthCheck
    {
        private const string DefaultTestQuery = "Select 1";
        private string ConnectionString = "";

        public SqlConnectionHealthCheck(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    await connection.OpenAsync(cancellationToken);

                    var command = connection.CreateCommand();
                    command.CommandText = DefaultTestQuery;

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
                catch (DbException ex)
                {
                    // Send email here
                    return new HealthCheckResult(status: context.Registration.FailureStatus, exception: ex);
                }
            }

            return HealthCheckResult.Healthy();
        }
    }
}