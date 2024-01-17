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
            services.AddScoped<IProductModelFactory, ProductModelFactory>();
            services.AddScoped<IShoppingCartModelFactory, ShoppingCartModelFactory>();
            services.AddScoped<ICatalogModelFactory, CatalogModelFactory>();
            services.AddScoped<IBlogModelFactory, BlogModelFactory>();
            services.AddScoped<ICheckoutModelFactory, CheckoutModelFactory>();
            services.AddScoped<ICommonModelFactory, CommonModelFactory>();
            services.AddScoped<IForumModelFactory, ForumModelFactory>();
            services.AddScoped<INewsletterModelFactory, NewsletterModelFactory>();
            services.AddScoped<INewsModelFactory, NewsModelFactory>();
            services.AddScoped<IPollModelFactory, PollModelFactory>();
            services.AddScoped<IPrivateMessagesModelFactory, PrivateMessagesModelFactory>();
            services.AddScoped<IProfileModelFactory, ProfileModelFactory>();
            services.AddScoped<IReturnRequestModelFactory, ReturnRequestModelFactory>();
            services.AddScoped<ISitemapModelFactory, SitemapModelFactory>();
            services.AddScoped<ISitemapModelFactory, SitemapModelFactory>();
            services.AddScoped<IVendorModelFactory, VendorModelFactory>();
        }
    }
}
