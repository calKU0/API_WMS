using APIWMS.Data;
using APIWMS.Helpers;
using APIWMS.Interfaces;
using APIWMS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Newtonsoft;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: Path.Combine(Directory.GetCurrentDirectory(), "Logs/log-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}"
    ).CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DevelopmentSQLConnection")));
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
    });
builder.Services.AddSwaggerGenNewtonsoftSupport();

// App Services
builder.Services.Configure<XlApiSettings>(builder.Configuration.GetSection("XlApiSettings"));
builder.Services.AddSingleton<IXlApiService, XlApiService>();
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddHostedService<LoginService>();

builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "APIWMS", Version = "v1" });

    // Custom order
    c.DocumentFilter<CustomTagOrderFilter>();
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
