using Datum.Domain.Entities;
using Datum.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Datum.Infrastructure.Services;

public class JwtService(IConfiguration configuration) : IJwtService
{
	private readonly IConfiguration _configuration = configuration;

	public string GenerateToken(User user)
	{
		var jwtSettings = _configuration.GetSection("JwtSettings");
		var secretKey = jwtSettings["SecretKey"]
			?? throw new InvalidOperationException("JwtSettings:SecretKey not configured.");

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new Claim(ClaimTypes.Email, user.Email),
			new Claim(ClaimTypes.Name, user.Username)
		};

		var token = new JwtSecurityToken(
			issuer: jwtSettings["Issuer"] ?? "Datum",
			audience: jwtSettings["Audience"] ?? "DatumUsers",
			claims: claims,
			expires: DateTime.UtcNow.AddHours(24),
			signingCredentials: credentials
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public int? GetUserIdFromToken(string token)
	{
		var handler = new JwtSecurityTokenHandler();
		if (!handler.CanReadToken(token)) return null;

		var jwtToken = handler.ReadJwtToken(token);
		var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

		return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
	}
}
