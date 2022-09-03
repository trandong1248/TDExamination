using Identity.API.Configuration;
using Identity.API.Services;
using IdentityServer4.Services;

namespace Identity.API.Installers
{
    public class ServiceInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // =================================== Add services to the container. ======================================
            // =========================================================================================================

            services.Configure<AppSettings>(configuration); // Load data to AppSetting class from file appsetting.json
            services.AddTransient<IProfileService, ProfileService>();


        }
    }
}
