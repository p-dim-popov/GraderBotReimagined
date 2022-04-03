using System.Net;
using System.Text;
using Api.Helpers.Authorization;
using Api.Services;
using Api.Services.Abstractions;
using Data.DbContexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.IdentityModel.Tokens;

namespace Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        var jwtSection = services
            .BuildServiceProvider()
            .GetService<IConfiguration>()
            !.GetSection("Jwt");
        services.Configure<JwtSettings>(jwtSection);

        var jwtSettings = jwtSection.Get<JwtSettings>();

        services.AddScoped<IJwtUtils, JwtUtils>();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents {
                    OnMessageReceived = (context) => {
                        if (!context.Request.Query.TryGetValue("access_token", out var values)) {
                            return Task.CompletedTask;
                        }

                        if (values.Count > 1) {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Fail(
                                "Only one 'access_token' query string parameter can be defined. " +
                                $"However, {values.Count:N0} were included in the request."
                            );

                            return Task.CompletedTask;
                        }

                        var token = values.Single();

                        if (string.IsNullOrWhiteSpace(token)) {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Fail(
                                "The 'access_token' query string parameter was defined, " +
                                "but a value to represent the token was not included."
                            );

                            return Task.CompletedTask;
                        }

                        context.Token = token;

                        return Task.CompletedTask;
                    }
                };

                var secret = jwtSettings.Secret;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });

        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    public static IServiceCollection AddConvenientDbContext(this IServiceCollection services, bool isDevelopment)
        => isDevelopment
            ? services.AddDbContext<AppDbContext, LocalDbContext>()
            // TODO: add a real db at some point
            : services;
}

public static class WebApplicationExtensions
{
    public static IApplicationBuilder UseClientSideAppDevelopmentServerWhen(
        this IApplicationBuilder app,
        bool condition
    ) => condition ? app.UseClientSideAppDevelopmentServer() : app;

    public static IApplicationBuilder UseClientSideAppDevelopmentServer(this IApplicationBuilder app) =>
        app.MapWhen(x => x.Connection.LocalPort == 3003, app0 =>
        {
            app0.UseSpa(spa =>
            {
                spa.Options.PackageManagerCommand = "yarn";
                spa.Options.SourcePath = "ClientApp";
                spa.Options.DevServerPort = 3003;
                spa.UseReactDevelopmentServer("dev");
            });
        });

    public static IApplicationBuilder UseAuth(this IApplicationBuilder app) =>
        app.UseAuthentication().UseAuthorization();

    public static IApplicationBuilder UseSwaggerDocsWhen(this IApplicationBuilder app, bool condition)
        => condition
            ? app.UseSwagger().UseSwaggerUI()
            : app;
}
