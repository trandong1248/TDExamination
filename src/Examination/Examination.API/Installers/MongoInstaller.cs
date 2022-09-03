using MongoDB.Driver;

namespace Examination.API.Installers
{
    public class MongoInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // mongoDb Singleton
            services.AddSingleton<IMongoClient>(c =>
            {
                var user = configuration.GetValue<string>("DatabaseSettings:User");
                var password = configuration.GetValue<string>("DatabaseSettings:Password");
                var server = configuration.GetValue<string>("DatabaseSettings:Server");
                var databaseName = configuration.GetValue<string>("DatabaseSettings:DatabaseName");
                var mongodbConnectionString = "mongodb://" + user + ":" + password + "@" + server + "/" + databaseName + "?authSource=admin";
                
                return new MongoClient(mongodbConnectionString);
            });
        }
    }
}
