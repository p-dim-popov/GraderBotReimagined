using Contracts;
using Data.DbContexts;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

    string? GetProblemType()
    {
        return routeValues?["problemType"]?.ToString()?.ToLower();
    }

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();