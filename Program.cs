using APIWMS.Data;
using APIWMS.Helpers;
using APIWMS.Interfaces;
using APIWMS.Middleware;
using APIWMS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Newtonsoft;
using System.Reflection;

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
builder.Services.Configure<BasicAuthSettings>(builder.Configuration.GetSection("BasicAuth"));
builder.Services.Configure<List<string>>(builder.Configuration.GetSection("AllowedIPs"));
builder.Services.AddSingleton<IXlApiService, XlApiService>();
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddHostedService<LoginService>();

builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "APIWMS", Version = "v1" });

    // Schema filters
    options.DocumentFilter<CustomTagOrderFilter>(); // Custom order

    options.EnableAnnotations();
    // XML comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // Authorization
    options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "Enter your username and password as 'username:password' in base64 format."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Basic"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<BasicAuthHandler>("Test");
app.UseMiddleware<IpWhitelistMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
