namespace Examination.API.Installers
{
    public class SystemInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .SetIsOriginAllowed((host) => true)
                        .AllowAnyMethod()
                        .WithExposedHeaders("Content-Disposition")
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddControllers();
        }
    }
}
