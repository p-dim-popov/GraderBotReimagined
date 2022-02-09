using Api;
using Api.Services;
using Api.Services.Abstractions;
using Data.DbContexts;
using Data.Models;
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
    .AddGradableAppProvider()
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

    var roles = context.Roles.ToList();
    if (!roles.Any())
    {
        roles.Add(new Role {Name = "Admin"});
        roles.Add(new Role {Name = "Moderator"});
        context.Roles.AddRange(roles);
    }
    context.SaveChanges();

    if (!context.Users.Any())
    {
        context.Users.AddRange(
            new User
            {
                Email = "admin@local.host",
                Password = BCrypt.Net.BCrypt.HashPassword("123"),
                Roles = roles.Select(x => new UserRole { Role = x }).ToList(),
            },
            new User
            {
                Email = "moderator@local.host",
                Password = BCrypt.Net.BCrypt.HashPassword("123"),
                Roles = roles
                    .Where(x => x.Name == "Moderator")
                    .Select(x => new UserRole { Role = x })
                    .ToList(),
            },
            new User
            {
                Email = "not-admin@local.host",
                Password = BCrypt.Net.BCrypt.HashPassword("123"),
            }
        );
    }
    context.SaveChanges();
}

app.MapControllers();
app.Run();
