using System;
using System.Reflection;
using Identity.Admin.EntityFramework.Configuration.Configuration;
using SqlMigrationAssembly = Identity.Admin.EntityFramework.SqlServer.Helpers.MigrationAssembly;

namespace Identity.Admin.Configuration.Database
{
    public static class MigrationAssemblyConfiguration
    {
        public static string GetMigrationAssemblyByProvider(DatabaseProviderConfiguration databaseProvider)
        {
            return databaseProvider.ProviderType switch
            {
                DatabaseProviderType.SqlServer => typeof(SqlMigrationAssembly).GetTypeInfo().Assembly.GetName().Name
            };
        }
    }
}