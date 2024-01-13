
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Nop.Api.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddJwtBererService(this IServiceCollection services, IConfiguration _config, IWebHostEnvironment _env)
        {
            //IdentityServerConfig identityServerConfig = new IdentityServerConfig();
            //Configuration.Bind("IdentityServerConfig", identityServerConfig);

            string JWT_KEY = _env.IsProduction() ? Environment.GetEnvironmentVariable("JWT_KEY") : _config["JWTSettings:Key"];
            string VALID_AUDIENCE = _env.IsProduction() ? Environment.GetEnvironmentVariable("VALID_AUDIENCE") : _config["JWTSettings:Audience"];
            string VALID_ISSUER = _env.IsProduction() ? Environment.GetEnvironmentVariable("VALID_ISSUER") : _config["JWTSettings:Issuer"];
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JWT_KEY, o =>
            {
                o.RequireHttpsMetadata = false;
                o.SaveToken = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = VALID_ISSUER,
                    ValidAudience = VALID_AUDIENCE,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT_KEY))
                };
            });
        }
    }
}
