using Api;
using Helpers;
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

builder.Services
    .AddConvenientDbContext(builder.Environment.IsDevelopment())
    .AddTransient<IProcessStarter, ProcessStarter>()
    .AddGradableAppProvider()
    .AddAuth();

var app = builder.Build();
app.UseSwaggerDocsWhen(app.Environment.IsDevelopment())
    .UseClientSideAppDevelopmentServerWhen(app.Environment.IsDevelopment())
    .UseHttpsRedirection()
    .UseAuth();

app.MapControllers();
app.Run();
