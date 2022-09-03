namespace Identity.API.Installers
{
    public class SystemInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddControllersWithViews();
            services.AddRazorPages();
        }
    }
}
