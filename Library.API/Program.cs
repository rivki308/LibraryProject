using System.Text;
using Library.Core.Interfaces;
using Library.Core.Settings;
using Library.Data;
using Library.Data.Repositories;
using Library.Service.Implementations;
using Library.Service.Mappings;
using Library.Service.Security;
using Library.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using Library.API.Middleware;
using Microsoft.EntityFrameworkCore;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Configuration: JWT settings
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
        ?? throw new InvalidOperationException("Jwt configuration section is missing");
    builder.Services.AddSingleton(jwtSettings);

    // Database
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is not configured");

    //builder.Services.AddDbContext<AppDbContext>(options =>
    //    options.UseSqlServer(connectionString), ServiceLifetime.Scoped);
    builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Repositories
    builder.Services.AddScoped<IBookRepository, BookRepository>();
    builder.Services.AddScoped<IMemberRepository, MemberRepository>();
    builder.Services.AddScoped<ILoanRepository, LoanRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

    // Services
    builder.Services.AddScoped<IBookService, BookService>();
    builder.Services.AddScoped<IMemberService, MemberService>();
    builder.Services.AddScoped<ILoanService, LoanService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

    // AutoMapper
    builder.Services.AddAutoMapper(cfg => cfg.AddProfile<LibraryMappingProfile>());

    // Controllers
    builder.Services.AddControllers();

    // CORS - allow the React client to call the API during development
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ReactClient", policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    // Authentication - JWT Bearer
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
            };
        });

    // Authorization policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    });

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Library API", Version = "v1" });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "הזן: Bearer {הטוקן שהתקבל מ-login}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    //var app = builder.Build();

    //using (var scope = app.Services.CreateScope())
    //{
    //    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //    await DbSeeder.SeedAsync(dbContext);
    //}

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowClient", policy =>
        {
            policy.WithOrigins("https://libraryproject-client.onrender.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
    var app = builder.Build();
    app.UseCors("AllowClient");
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbSeeder.SeedAsync(dbContext);
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "An error occurred while initializing the database.");
    }

    // Middleware pipeline order matters:
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseCors("ReactClient");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}
