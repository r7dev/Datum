using Datum.Domain.DTOs;
using Datum.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Datum.BlogAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
	private readonly IAuthService _authService = authService;

	/// <summary>Registra um novo usuário</summary>
	[HttpPost("register")]
	[ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request)
	{
		try
		{
			var response = await _authService.RegisterAsync(request);
			return StatusCode(StatusCodes.Status201Created, response);
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>Realiza login e retorna o token JWT</summary>
	[HttpPost("login")]
	[ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Login([FromBody] LoginRequest request)
	{
		try
		{
			var response = await _authService.LoginAsync(request);
			return Ok(response);
		}
		catch (UnauthorizedAccessException ex)
		{
			return Unauthorized(new { message = ex.Message });
		}
	}
}
