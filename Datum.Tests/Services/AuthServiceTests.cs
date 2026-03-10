using Datum.Domain.DTOs;
using Datum.Domain.Entities;
using Datum.Domain.Interfaces.Repositories;
using Datum.Domain.Interfaces.Services;
using Datum.Infrastructure.Services;
using Datum.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Datum.Tests.Services;

public class AuthServiceTests
{
	private readonly Mock<IUserRepository> _userRepositoryMock;
	private readonly Mock<IJwtService> _jwtServiceMock;
	private readonly AuthService _sut; // System Under Test

	public AuthServiceTests()
	{
		_userRepositoryMock = new Mock<IUserRepository>();
		_jwtServiceMock     = new Mock<IJwtService>();
		_sut                = new AuthService(_userRepositoryMock.Object, _jwtServiceMock.Object);
	}

	// ── RegisterAsync ──────────────────────────────────────────────────────────

	[Fact]
	public async Task RegisterAsync_ComDadosValidos_DeveRetornarAuthResponse()
	{
		// Arrange
		var request = new RegisterRequest("novousuario", "novo@datum.com", "Senha@123");

		_userRepositoryMock.Setup(r => r.ExistsByEmailAsync(request.Email))
						   .ReturnsAsync(false);
		_userRepositoryMock.Setup(r => r.ExistsByUsernameAsync(request.Username))
						   .ReturnsAsync(false);
		_userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
						   .ReturnsAsync((User u) => u);
		_userRepositoryMock.Setup(r => r.SaveChangesAsync())
						   .Returns(Task.CompletedTask);
		_jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>()))
					   .Returns("jwt.token.gerado");

		// Act
		var result = await _sut.RegisterAsync(request);

		// Assert
		result.Should().NotBeNull();
		result.Token.Should().Be("jwt.token.gerado");
		result.Username.Should().Be(request.Username);
		result.Email.Should().Be(request.Email);
	}

	[Fact]
	public async Task RegisterAsync_ComEmailDuplicado_DeveLancarInvalidOperationException()
	{
		// Arrange
		var request = new RegisterRequest("usuario", "existente@datum.com", "Senha@123");

		_userRepositoryMock.Setup(r => r.ExistsByEmailAsync(request.Email))
						   .ReturnsAsync(true);

		// Act
		var act = () => _sut.RegisterAsync(request);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
				 .WithMessage("E-mail já está em uso.");
	}

	[Fact]
	public async Task RegisterAsync_ComUsernameDuplicado_DeveLancarInvalidOperationException()
	{
		// Arrange
		var request = new RegisterRequest("userexistente", "novo@datum.com", "Senha@123");

		_userRepositoryMock.Setup(r => r.ExistsByEmailAsync(request.Email))
						   .ReturnsAsync(false);
		_userRepositoryMock.Setup(r => r.ExistsByUsernameAsync(request.Username))
						   .ReturnsAsync(true);

		// Act
		var act = () => _sut.RegisterAsync(request);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
				 .WithMessage("Nome de usuário já está em uso.");
	}

	[Fact]
	public async Task RegisterAsync_DeveChamarAddAsyncESaveChanges_UmaVezCadaUm()
	{
		// Arrange
		var request = new RegisterRequest("usuario", "email@datum.com", "Senha@123");

		_userRepositoryMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
		_userRepositoryMock.Setup(r => r.ExistsByUsernameAsync(It.IsAny<string>())).ReturnsAsync(false);
		_userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
		_userRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
		_jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("token");

		// Act
		await _sut.RegisterAsync(request);

		// Assert
		_userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
		_userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task RegisterAsync_DeveSalvarSenhaComoHash_NaoEmTextoPlano()
	{
		// Arrange
		const string senhaPlana = "Senha@123";
		User? usuarioSalvo = null;

		_userRepositoryMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
		_userRepositoryMock.Setup(r => r.ExistsByUsernameAsync(It.IsAny<string>())).ReturnsAsync(false);
		_userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
						   .Callback<User>(u => usuarioSalvo = u)
						   .ReturnsAsync((User u) => u);
		_userRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
		_jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("token");

		// Act
		await _sut.RegisterAsync(new RegisterRequest("user", "u@datum.com", senhaPlana));

		// Assert
		usuarioSalvo.Should().NotBeNull();
		usuarioSalvo!.PasswordHash.Should().NotBe(senhaPlana);
		BCrypt.Net.BCrypt.Verify(senhaPlana, usuarioSalvo.PasswordHash).Should().BeTrue();
	}

	// ── LoginAsync ─────────────────────────────────────────────────────────────

	[Fact]
	public async Task LoginAsync_ComCredenciaisValidas_DeveRetornarAuthResponse()
	{
		// Arrange
		const string senhaPlana = "Senha@123";
		var user = EntityBuilder.BuildUser(passwordHash: BCrypt.Net.BCrypt.HashPassword(senhaPlana));
		var request = new LoginRequest(user.Email, senhaPlana);

		_userRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email))
						   .ReturnsAsync(user);
		_jwtServiceMock.Setup(j => j.GenerateToken(user))
					   .Returns("jwt.valido");

		// Act
		var result = await _sut.LoginAsync(request);

		// Assert
		result.Should().NotBeNull();
		result.Token.Should().Be("jwt.valido");
		result.Username.Should().Be(user.Username);
		result.Email.Should().Be(user.Email);
	}

	[Fact]
	public async Task LoginAsync_ComEmailInexistente_DeveLancarUnauthorizedAccessException()
	{
		// Arrange
		_userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
						   .ReturnsAsync((User?)null);

		// Act
		var act = () => _sut.LoginAsync(new LoginRequest("naoexiste@datum.com", "qualquer"));

		// Assert
		await act.Should().ThrowAsync<UnauthorizedAccessException>()
				 .WithMessage("Credenciais inválidas.");
	}

	[Fact]
	public async Task LoginAsync_ComSenhaErrada_DeveLancarUnauthorizedAccessException()
	{
		// Arrange
		var user = EntityBuilder.BuildUser(passwordHash: BCrypt.Net.BCrypt.HashPassword("SenhaCorreta@123"));

		_userRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email))
						   .ReturnsAsync(user);

		// Act
		var act = () => _sut.LoginAsync(new LoginRequest(user.Email, "SenhaErrada@999"));

		// Assert
		await act.Should().ThrowAsync<UnauthorizedAccessException>()
				 .WithMessage("Credenciais inválidas.");
	}

	[Fact]
	public async Task LoginAsync_DeveGerarToken_UsandoOUsuarioCorreto()
	{
		// Arrange
		const string senhaPlana = "Senha@123";
		var user = EntityBuilder.BuildUser(passwordHash: BCrypt.Net.BCrypt.HashPassword(senhaPlana));

		_userRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
		_jwtServiceMock.Setup(j => j.GenerateToken(user)).Returns("token");

		// Act
		await _sut.LoginAsync(new LoginRequest(user.Email, senhaPlana));

		// Assert
		_jwtServiceMock.Verify(j => j.GenerateToken(user), Times.Once);
	}
}
