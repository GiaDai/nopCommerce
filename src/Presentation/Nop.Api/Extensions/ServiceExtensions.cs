
using System.Reflection;
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
            string? jWT_KEY = _env.IsProduction() ? Environment.GetEnvironmentVariable("JWT_KEY") : _config["JWTSettings:Key"];
            string? vALID_AUDIENCE = _env.IsProduction() ? Environment.GetEnvironmentVariable("VALID_AUDIENCE") : _config["JWTSettings:Audience"];
            string? vALID_ISSUER = _env.IsProduction() ? Environment.GetEnvironmentVariable("VALID_ISSUER") : _config["JWTSettings:Issuer"];
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = vALID_ISSUER,
                    ValidAudience = vALID_AUDIENCE,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jWT_KEY))
                };
            });
        }
        public static void AddJwtBererService(this IServiceCollection services, IConfiguration _config, IWebHostEnvironment _env)
        {
            //IdentityServerConfig identityServerConfig = new IdentityServerConfig();
            //Configuration.Bind("IdentityServerConfig", identityServerConfig);

            string? jWT_KEY = _env.IsProduction() ? Environment.GetEnvironmentVariable("JWT_KEY") : _config["JWTSettings:Key"];
            string? vALID_AUDIENCE = _env.IsProduction() ? Environment.GetEnvironmentVariable("VALID_AUDIENCE") : _config["JWTSettings:Audience"];
            string? vALID_ISSUER = _env.IsProduction() ? Environment.GetEnvironmentVariable("VALID_ISSUER") : _config["JWTSettings:Issuer"];
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jWT_KEY, o =>
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
                    ValidIssuer = vALID_ISSUER,
                    ValidAudience = vALID_AUDIENCE,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jWT_KEY))
                };
            });
        }

        public static void AddSwaggerExtension(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "NopCommerce Wallet",
                    Description = "An ASP.NET Core Web API for managing NopCommerce Wallet items",
                    TermsOfService = new Uri("https://nop-wallet.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Nop Wallet Contact",
                        Url = new Uri("https://nop-wallet.com/contact")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Nop Wallet License",
                        Url = new Uri("https://nop-wallet.com/license")
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
                // using System.Reflection;
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });
        }

        public static void AddCorsExtension(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        public static void AddApiVersioningExtension(this IServiceCollection services)
        {
            services.AddApiVersioning(config =>
            {
                // Specify the default API Version as 1.0
                config.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                // If the client hasn't specified the API version in the request, use the default API version number 
                config.AssumeDefaultVersionWhenUnspecified = true;
                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
            });
        }
    }
}
