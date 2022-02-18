using Api;
using Api.Data.Seeding;
using Api.Services;
using Api.Services.Abstractions;
using Data.DbContexts;
using Data.Seeding;
using Data.Seeding.Abstractions;
using Helpers;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddDefaultPolicy(cors => cors.WithOrigins("*")));
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
    .AddScoped<IProblemsService, ProblemsService>()
    .AddScoped<ISolutionsService, SolutionsService>()
    .AddScoped<ITestableAppFactory, TestableAppFactory>()
    .AddAuth();

var app = builder.Build();
app.UseSwaggerDocsWhen(app.Environment.IsDevelopment())
    .UseClientSideAppDevelopmentServerWhen(app.Environment.IsDevelopment())
    // .UseHttpsRedirection()
    .UseAuth()
    .UseCors();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetService<AppDbContext>()!;

    new AppDbSeeder(new ISeeder[]
        {
            new RolesSeeder(),
            new UsersSeeder(),
            new ProblemsSeeder(),
            new SolutionsSeeder(),
        })
        .SeedAsync(context, scope.ServiceProvider)
        .GetAwaiter()
        .GetResult();
}

app.MapControllers();
app.Run();
