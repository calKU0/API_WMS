using APIWMS.Data;
using APIWMS.Helpers;
using APIWMS.Interfaces;
using APIWMS.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

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
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevelopmentSQLConnection")));
builder.Services.AddControllers();

// App Services
builder.Services.Configure<XlApiSettings>(builder.Configuration.GetSection("XlApiSettings"));
builder.Services.AddSingleton<IXlApiService, XlApiService>();
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddHostedService<LoginService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
