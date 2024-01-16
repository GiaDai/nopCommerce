using Autofac.Extensions.DependencyInjection;
using Nop.Api.Extensions;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Infrastructure.Extensions;
var builder = WebApplication.CreateBuilder(args);
var _services = builder.Services;
var _config = builder.Configuration;
var _env = builder.Environment;
// Add services to the container.
_services.AddSwaggerExtension();
_services.AddControllers();

builder.Configuration.AddJsonFile(NopConfigurationDefaults.AppSettingsFilePath, true, true);
if (!string.IsNullOrEmpty(builder.Environment?.EnvironmentName))
{
    var path = string.Format(NopConfigurationDefaults.AppSettingsEnvironmentFilePath, builder.Environment.EnvironmentName);
    builder.Configuration.AddJsonFile(path, true, true);
}

builder.Configuration.AddEnvironmentVariables();
//load application settings
_services.ConfigureApplicationSettings(builder);
_services.AddApiVersioningExtension();
var appSettings = Singleton<AppSettings>.Instance;
var useAutofac = appSettings.Get<CommonConfig>().UseAutofac;

if (useAutofac)
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
else
    builder.Host.UseDefaultServiceProvider(options =>
    {
        //we don't validate the scopes, since at the app start and the initial configuration we need 
        //to resolve some services (registered as "scoped") through the root container
        options.ValidateScopes = false;
        options.ValidateOnBuild = true;
    });
_services.AddAuthenticationService(_config,_env);
//add services to the application and configure service provider
_services.ConfigureApplicationServices(builder);
_services.AddJwtBererService(_config, _env);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
_services.AddEndpointsApiExplorer();
_services.AddCorsExtension();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExtension();
}
//configure the application HTTP request pipeline
app.ConfigureRequestPipeline();
app.StartEngine();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();
app.UseCrossOrigin();
app.Run();
