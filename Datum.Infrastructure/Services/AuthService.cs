using Datum.Domain.DTOs;
using Datum.Domain.Entities;
using Datum.Domain.Interfaces.Repositories;
using Datum.Domain.Interfaces.Services;

namespace Datum.Infrastructure.Services;

public class AuthService(IUserRepository userRepository, IJwtService jwtService) : IAuthService
{
	private readonly IUserRepository _userRepository = userRepository;
	private readonly IJwtService _jwtService = jwtService;

	public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
	{
		if (await _userRepository.ExistsByEmailAsync(request.Email))
			throw new InvalidOperationException("E-mail já está em uso.");

		if (await _userRepository.ExistsByUsernameAsync(request.Username))
			throw new InvalidOperationException("Nome de usuário já está em uso.");

		var user = new User
		{
			Username = request.Username,
			Email    = request.Email,
			PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
		};

		await _userRepository.AddAsync(user);
		await _userRepository.SaveChangesAsync();

		var token = _jwtService.GenerateToken(user);
		return new AuthResponse(token, user.Username, user.Email);
	}

	public async Task<AuthResponse> LoginAsync(LoginRequest request)
	{
		var user = await _userRepository.GetByEmailAsync(request.Email);

		if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
			throw new UnauthorizedAccessException("Credenciais inválidas.");

		var token = _jwtService.GenerateToken(user);
		return new AuthResponse(token, user.Username, user.Email);
	}
}
