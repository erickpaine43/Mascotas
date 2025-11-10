using Mascotas.Data;
using Mascotas.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OfficeOpenXml;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<MascotaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PetStore API",
        Version = "v1",
        Description = "API para Tienda de Mascotas"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingresa el token JWT en el formato: Bearer {token}",
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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "PetStoreSecretKey2024!";

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
        ValidIssuer = jwtSettings["Issuer"] ?? "PetStoreAPI",
        ValidAudience = jwtSettings["Audience"] ?? "PetStoreUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Services
builder.Services.AddScoped<IJwtServices, JwtService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IOrdenService, OrdenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IReviewReminderService, ReviewReminderService>();
builder.Services.AddScoped<IFiltroGuardadoService, FiltroGuardadoService>();
builder.Services.AddScoped<IAlertaPrecioService, AlertaPrecioService>();
builder.Services.AddScoped<IBusquedaAvanzadaService, BusquedaAvanzadaService>();
builder.Services.AddScoped<IMonitorBusquedasService, MonitorBusquedasService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddHostedService<MonitorBackgroundService>();

// Background Services
builder.Services.AddHostedService<ReservaCleanupService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Logging
builder.Services.AddLogging();


ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("ReactFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();