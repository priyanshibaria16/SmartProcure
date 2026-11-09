using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartProcure.Application.Common.Interfaces;
using SmartProcure.Application.Interfaces;
using SmartProcure.Application.Services;
using SmartProcure.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration with Resilient Auto-Detection
var config = builder.Configuration;
var dbProvider = config["DatabaseProvider"] ?? "SqlServer";
var sqlServerConn = config.GetConnectionString("SqlServerConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=SmartProcureDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
var sqliteConn = config.GetConnectionString("SqliteConnection") ?? "Data Source=smartprocure.db";

bool useSqlServer = dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);

if (useSqlServer)
{
    try
    {
        var testOptions = new DbContextOptionsBuilder<SmartProcureDbContext>()
            .UseSqlServer(sqlServerConn, sql => sql.CommandTimeout(2))
            .Options;
        using var testContext = new SmartProcureDbContext(testOptions);
        if (!testContext.Database.CanConnect())
        {
            useSqlServer = false;
        }
    }
    catch
    {
        useSqlServer = false;
    }
}

builder.Services.AddDbContext<SmartProcureDbContext>(options =>
{
    if (useSqlServer)
    {
        options.UseSqlServer(sqlServerConn);
    }
    else
    {
        options.UseSqlite(sqliteConn);
    }
});

builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<SmartProcureDbContext>());

// 2. Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<IProcurementService, ProcurementService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();

// 3. JWT Authentication & Authorization
var jwtSecret = config["Jwt:SecretKey"] ?? "SmartProcure_Enterprise_Super_Secret_Key_2026_Secure_JWT_Token_Generator_Key!";
var issuer = config["Jwt:Issuer"] ?? "SmartProcure.API";
var audience = config["Jwt:Audience"] ?? "SmartProcure.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 4. Controllers & JSON Formatting
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 5. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 6. Swagger / OpenAPI Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartProcure Enterprise ERP API",
        Version = "v1",
        Description = "Enterprise Procurement & Inventory Management Platform (SAP MM / P2P Inspired)"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 7. Database Initialization & Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<SmartProcureDbContext>();

    try
    {
        logger.LogInformation("Initializing database with provider: {Provider}", useSqlServer ? "Microsoft SQL Server" : "SQLite");
        await context.Database.EnsureCreatedAsync();
        await DatabaseSeeder.SeedAsync(context);
        logger.LogInformation("Database initialized and demo data seeded successfully for Apex Manufacturing Industries Pvt. Ltd.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database initialization.");
    }
}

// 8. Pipeline Configuration
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartProcure API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Partial program class for WebApplicationFactory integration testing
public partial class Program { }
