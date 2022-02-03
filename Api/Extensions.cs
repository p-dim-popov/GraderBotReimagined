using System.Text;
using Api.Helpers;
using Api.Helpers.Authorization;
using Api.Services;
using Api.Services.Abstractions;
using Data.DbContexts;
using Data.Models.Enums;
using Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.IdentityModel.Tokens;
using Runners;
using Runners.Abstractions;

namespace Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGradableAppProvider(this IServiceCollection services) =>
        services
            .AddHttpContextAccessor()
            .AddScoped<IApp>(serviceProvider =>
            {
                var context = serviceProvider
                    .GetRequiredService<IHttpContextAccessor>()
                    .HttpContext;
                var routeValues = context!.Request.RouteValues;

                var language = routeValues["programmingLanguage"]?.ToString()?.ToLower();
                var type = routeValues["solutionType"]?.ToString()?.ToLower();
                var processStarter = serviceProvider.GetService<IProcessStarter>()!;
                var problemType = ProblemTypeResolver.Resolve(language, type);

                return problemType switch
                {
                    ProblemType.JavaScriptSingleFileConsoleApp => new JavaScriptSingleFileConsoleApp(processStarter),
                    _ => new NotSupportedApp {Language = language, Type = type},
                };
            });

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
