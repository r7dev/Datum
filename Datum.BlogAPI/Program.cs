using Datum.BlogAPI.Middleware;
using Datum.Domain.Interfaces.Repositories;
using Datum.Domain.Interfaces.Services;
using Datum.Infrastructure.Data;
using Datum.Infrastructure.Repositories;
using Datum.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers & API Explorer ───────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger com suporte a JWT ─────────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title       = "Datum Blog API",
		Version     = "v1",
		Description = "API de Blog com autenticação JWT e notificações em tempo real via WebSocket."
	});

	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = "Autenticação JWT. Informe: Bearer {token}",
		Name        = "Authorization",
		In          = ParameterLocation.Header,
		Type        = SecuritySchemeType.ApiKey,
		Scheme      = "Bearer"
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

// ── Entity Framework — SQL Server (Database First, sem migrations) ────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("DefaultConnection"),
		sqlOptions => sqlOptions.EnableRetryOnFailure(3)
	)
);

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey   = jwtSettings["SecretKey"]
	?? throw new InvalidOperationException("JwtSettings:SecretKey não configurado.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer           = true,
			ValidateAudience         = true,
			ValidateLifetime         = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer              = jwtSettings["Issuer"]   ?? "Datum",
			ValidAudience            = jwtSettings["Audience"] ?? "DatumUsers",
			IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
		};
	});

builder.Services.AddAuthorization();

// ── Injeção de Dependência — DIP (SOLID) ──────────────────────────────────────
// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IJwtService,  JwtService>();
builder.Services.AddSingleton<INotificationService, NotificationService>(); // Singleton: mantém conexões WS ativas

// ── Pipeline ──────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "Datum Blog API v1");
		c.RoutePrefix = string.Empty; // Swagger na raiz: http://localhost:5000
	});
}

app.UseHttpsRedirection();

app.UseWebSockets(new WebSocketOptions
{
	KeepAliveInterval = TimeSpan.FromMinutes(2)
});

app.UseMiddleware<WebSocketMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
