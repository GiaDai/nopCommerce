using Nop.Api.Factories;
using Nop.Core.Infrastructure;

namespace Nop.Api.Infrastructure
{
    /// <summary>
    /// Represents the registration services on the application startup
    /// </summary>
    public partial class NopStartup : INopStartup
    {
        public int Order => 2004;

        public void Configure(IApplicationBuilder application)
        {
            
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICustomerModelFactory, CustomerModelFactory>();
            services.AddScoped<IAddressModelFactory, AddressModelFactory>();
            services.AddScoped<ICountryModelFactory, CountryModelFactory>();
            services.AddScoped<IOrderModelFactory, OrderModelFactory>();
        }
    }
}
