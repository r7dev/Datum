using Datum.Domain.Entities;

namespace Datum.Domain.Interfaces.Services;

public interface IJwtService
{
	string GenerateToken(User user);
	int? GetUserIdFromToken(string token);
}
