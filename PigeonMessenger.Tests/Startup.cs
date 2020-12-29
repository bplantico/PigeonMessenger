using Microsoft.Extensions.DependencyInjection;

namespace PigeonMessenger.Tests
{
    class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDbService, SnowflakeDbService>();
        }
    }
}
