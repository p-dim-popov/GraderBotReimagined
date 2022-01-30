using System.Text;
using Api.Helpers.Authorization;
using Api.Services;
using Data.DbContexts;
using Helpers;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Runners;
using Runners.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swaggerGenOptions =>
{
    var bearerScheme = new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter into field the word 'Bearer' following by space and JWT",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
    };
    swaggerGenOptions.AddSecurityDefinition("Bearer", bearerScheme);
    swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, new List<string>()
        },
    });
});

// TODO: add a real db at some point
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<AppDbContext, LocalDbContext>();
}

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<IProcessStarter, ProcessStarter>();
builder.Services.AddScoped<IApp>(serviceProvider =>
{
    var routeValues = serviceProvider.GetRequiredService<IHttpContextAccessor>()
        .HttpContext
        !.Request
        .RouteValues;

    string? GetProblemType() => routeValues["problemType"]?.ToString()?.ToLower();

    var processStarter = serviceProvider.GetService<IProcessStarter>()!;
    return routeValues["programmingLanguage"]?.ToString()?.ToLower() switch
    {
        "javascript" => GetProblemType() switch
        {
            "single-file" => new JavaScriptSingleFileConsoleApp(processStarter),
            { } type => new NotSupportedApp { Type = type },
            _ => new NotSupportedApp(),
        },
        { } language => new NotSupportedApp { Language = language },
        _ => new NotSupportedApp(),
    };
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<IJwtUtils, JwtUtils>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var secret = builder.Configuration.GetSection("Jwt").Get<JwtSettings>().Secret;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

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
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
