using Datum.Domain.Entities;
using Datum.Infrastructure.Repositories;
using Datum.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace Datum.Tests.Repositories;

/// <summary>
/// Testes do UserRepository usando EF Core InMemory.
/// Cada teste recebe um contexto isolado com banco próprio.
/// </summary>
public class UserRepositoryTests
{
	// ── GetByIdAsync ───────────────────────────────────────────────────────────

	[Fact]
	public async Task GetByIdAsync_ComIdExistente_DeveRetornarUsuario()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var user = new User { Username = "joao", Email = "joao@datum.com", PasswordHash = "hash" };
		context.Users.Add(user);
		await context.SaveChangesAsync();

		var repo = new UserRepository(context);

		// Act
		var result = await repo.GetByIdAsync(user.Id);

		// Assert
		result.Should().NotBeNull();
		result!.Email.Should().Be("joao@datum.com");
	}

	[Fact]
	public async Task GetByIdAsync_ComIdInexistente_DeveRetornarNull()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var repo = new UserRepository(context);

		// Act
		var result = await repo.GetByIdAsync(999);

		// Assert
		result.Should().BeNull();
	}

	// ── GetByEmailAsync ────────────────────────────────────────────────────────

	[Fact]
	public async Task GetByEmailAsync_ComEmailExistente_DeveRetornarUsuario()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var user = new User { Username = "maria", Email = "maria@datum.com", PasswordHash = "hash" };
		context.Users.Add(user);
		await context.SaveChangesAsync();

		var repo = new UserRepository(context);

		// Act
		var result = await repo.GetByEmailAsync("maria@datum.com");

		// Assert
		result.Should().NotBeNull();
		result!.Username.Should().Be("maria");
	}

	[Fact]
	public async Task GetByEmailAsync_ComEmailInexistente_DeveRetornarNull()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var repo = new UserRepository(context);

		// Act
		var result = await repo.GetByEmailAsync("naoexiste@datum.com");

		// Assert
		result.Should().BeNull();
	}

	// ── ExistsByEmailAsync ─────────────────────────────────────────────────────

	[Fact]
	public async Task ExistsByEmailAsync_ComEmailCadastrado_DeveRetornarTrue()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		context.Users.Add(new User { Username = "user1", Email = "user1@datum.com", PasswordHash = "h" });
		await context.SaveChangesAsync();

		var repo = new UserRepository(context);

		// Act
		var exists = await repo.ExistsByEmailAsync("user1@datum.com");

		// Assert
		exists.Should().BeTrue();
	}

	[Fact]
	public async Task ExistsByEmailAsync_ComEmailNaoCadastrado_DeveRetornarFalse()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var repo = new UserRepository(context);

		// Act
		var exists = await repo.ExistsByEmailAsync("fantasma@datum.com");

		// Assert
		exists.Should().BeFalse();
	}

	// ── ExistsByUsernameAsync ──────────────────────────────────────────────────

	[Fact]
	public async Task ExistsByUsernameAsync_ComUsernameExistente_DeveRetornarTrue()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		context.Users.Add(new User { Username = "admin", Email = "admin@datum.com", PasswordHash = "h" });
		await context.SaveChangesAsync();

		var repo = new UserRepository(context);

		// Act
		var exists = await repo.ExistsByUsernameAsync("admin");

		// Assert
		exists.Should().BeTrue();
	}

	[Fact]
	public async Task ExistsByUsernameAsync_ComUsernameInexistente_DeveRetornarFalse()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var repo = new UserRepository(context);

		// Act
		var exists = await repo.ExistsByUsernameAsync("naoexiste");

		// Assert
		exists.Should().BeFalse();
	}

	// ── AddAsync + SaveChangesAsync ────────────────────────────────────────────

	[Fact]
	public async Task AddAsync_DeveInserirUsuarioNoBanco()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var repo = new UserRepository(context);
		var user = new User { Username = "novo", Email = "novo@datum.com", PasswordHash = "hash" };

		// Act
		await repo.AddAsync(user);
		await repo.SaveChangesAsync();

		// Assert
		var saved = await repo.GetByEmailAsync("novo@datum.com");
		saved.Should().NotBeNull();
		saved!.Username.Should().Be("novo");
	}

	[Fact]
	public async Task AddAsync_DeveAtribuirId_AposInserir()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var repo = new UserRepository(context);
		var user = new User { Username = "comId", Email = "comid@datum.com", PasswordHash = "hash" };

		// Act
		await repo.AddAsync(user);
		await repo.SaveChangesAsync();

		// Assert
		user.Id.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task AddAsync_DeveRetornarOMesmoUsuario()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var repo = new UserRepository(context);
		var user = new User { Username = "ret", Email = "ret@datum.com", PasswordHash = "hash" };

		// Act
		var returned = await repo.AddAsync(user);

		// Assert
		returned.Should().BeSameAs(user);
	}
}
