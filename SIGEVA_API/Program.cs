
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SIGEVA_API.Data;
using SIGEVA_API.Interfaces;
using SIGEVA_API.Middleware;
using SIGEVA_API.Models;
using SIGEVA_API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Inyección de dependencias
// ==========================================

// Usuarios
builder.Services.AddScoped<IUsuarioData, UsuarioData>();

// Equipos y mantenimiento
builder.Services.AddScoped<IEquipoData, EquipoData>();
builder.Services.AddScoped<IMantenimientoData, MantenimientoData>();

// Historial clínico
builder.Services.AddScoped<IHistorialClinicoData, HistorialClinicoData>();

// ==========================================
// Configuración de correo electrónico
// ==========================================

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IEmailService, EmailService>();

// ==========================================
// Servicios de seguridad y autenticación
// ==========================================

// Generación de tokens JWT
builder.Services.AddScoped<JwtService>();

// HU-AUT-004: Autenticación de doble factor
builder.Services.AddScoped<DobleFactorService>();

// ==========================================
// Configuración de autenticación JWT
// ==========================================

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = builder.Configuration["Jwt:Issuer"],

                ValidAudience = builder.Configuration["Jwt:Audience"],

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"]!
                    )
                )
            };
    });

// Autorización
builder.Services.AddAuthorization();

// ==========================================
// Configuración de controladores
// ==========================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ==========================================
// Configuración de Swagger
// ==========================================

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
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

// ==========================================
// Construcción de la aplicación
// ==========================================

var app = builder.Build();

// ==========================================
// Middleware de manejo de excepciones
// ==========================================

// Debe ejecutarse primero
app.UseMiddleware<ErrorHandlingMiddleware>();

// ==========================================
// Swagger en entorno de desarrollo
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ==========================================
// Configuración HTTP y seguridad
// ==========================================

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// ==========================================
// Rutas de controladores
// ==========================================

app.MapControllers();

// ==========================================
// Ejecutar aplicación
// ==========================================

app.Run();
