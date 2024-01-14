
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Nop.Api.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddAuthenticationService(this IServiceCollection services, IConfiguration _config, IWebHostEnvironment _env)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["JWTSettings:Issuer"],
                    ValidAudience = _config["JWTSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWTSettings:Key"]))
                };
            });
        }
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

        public static void AddSwaggerExtension(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.IncludeXmlComments(string.Format(@"Nop.Api.xml"));
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "MediaOne.Partner.F5Seconds.WebApi",
                    Description = "This Api will be responsible for overall data distribution and authorization.",
                    Contact = new OpenApiContact
                    {
                        Name = "Le Anh Toan",
                        Email = "toanle@f5seconds.vn",
                        Url = new Uri("https://f5seconds.vn"),
                    }
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        }, new List<string>()
                    },
                });
            });
        }
    }
}
