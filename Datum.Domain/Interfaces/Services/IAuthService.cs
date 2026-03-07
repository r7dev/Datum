using Datum.Domain.DTOs;

namespace Datum.Domain.Interfaces.Services;

public interface IAuthService
{
	Task<AuthResponse> RegisterAsync(RegisterRequest request);
	Task<AuthResponse> LoginAsync(LoginRequest request);
}
