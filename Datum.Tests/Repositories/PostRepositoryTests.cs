using Datum.Domain.Entities;
using Datum.Infrastructure.Repositories;
using Datum.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace Datum.Tests.Repositories;

/// <summary>
/// Testes do PostRepository usando EF Core InMemory.
/// </summary>
public class PostRepositoryTests
{
	private static async Task<(User user, int userId)> SeedUserAsync(
		Datum.Infrastructure.Data.ApplicationDbContext context,
		string username = "autor",
		string email = "autor@datum.com")
	{
		var user = new User { Username = username, Email = email, PasswordHash = "hash" };
		context.Users.Add(user);
		await context.SaveChangesAsync();
		return (user, user.Id);
	}

	// ── GetAllAsync ────────────────────────────────────────────────────────────

	[Fact]
	public async Task GetAllAsync_ComPostsInseridos_DeveRetornarTodosOsPosts()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var (_, userId) = await SeedUserAsync(context);

		context.Posts.AddRange(
			new Post { Title = "Post 1", Content = "C1", UserId = userId },
			new Post { Title = "Post 2", Content = "C2", UserId = userId }
		);
		await context.SaveChangesAsync();

		var repo = new PostRepository(context);

		// Act
		var result = (await repo.GetAllAsync()).ToList();

		// Assert
		result.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetAllAsync_DeveIncluirAutor_EmCadaPost()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var (_, userId) = await SeedUserAsync(context, "escritor");

		context.Posts.Add(new Post { Title = "Post com Author", Content = "C", UserId = userId });
		await context.SaveChangesAsync();

		var repo = new PostRepository(context);

		// Act
		var result = (await repo.GetAllAsync()).ToList();

		// Assert
		result.First().Author.Should().NotBeNull();
		result.First().Author.Username.Should().Be("escritor");
	}

	[Fact]
	public async Task GetAllAsync_DeveRetornarOrdenado_PorCreatedAtDescendente()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var (_, userId) = await SeedUserAsync(context);

		context.Posts.AddRange(
			new Post { Title = "Antigo", Content = "C", UserId = userId, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
			new Post { Title = "Recente", Content = "C", UserId = userId, CreatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc) }
		);
		await context.SaveChangesAsync();

		var repo = new PostRepository(context);

		// Act
		var result = (await repo.GetAllAsync()).ToList();

		// Assert
		result.First().Title.Should().Be("Recente");
		result.Last().Title.Should().Be("Antigo");
	}

	[Fact]
	public async Task GetAllAsync_SemPosts_DeveRetornarListaVazia()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var repo = new PostRepository(context);

		// Act
		var result = await repo.GetAllAsync();

		// Assert
		result.Should().BeEmpty();
	}

	// ── GetByIdAsync ───────────────────────────────────────────────────────────

	[Fact]
	public async Task GetByIdAsync_ComIdExistente_DeveRetornarPost()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var (_, userId) = await SeedUserAsync(context);

		var post = new Post { Title = "Meu Post", Content = "Conteúdo", UserId = userId };
		context.Posts.Add(post);
		await context.SaveChangesAsync();

		var repo = new PostRepository(context);

		// Act
		var result = await repo.GetByIdAsync(post.Id);

		// Assert
		result.Should().NotBeNull();
		result!.Title.Should().Be("Meu Post");
	}

	[Fact]
	public async Task GetByIdAsync_ComIdInexistente_DeveRetornarNull()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var repo = new PostRepository(context);

		// Act
		var result = await repo.GetByIdAsync(999);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task GetByIdAsync_DeveIncluirAutor()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var (_, userId) = await SeedUserAsync(context, "autorNomeado");

		var post = new Post { Title = "Post com autor", Content = "C", UserId = userId };
		context.Posts.Add(post);
		await context.SaveChangesAsync();

		var repo = new PostRepository(context);

		// Act
		var result = await repo.GetByIdAsync(post.Id);

		// Assert
		result!.Author.Should().NotBeNull();
		result.Author.Username.Should().Be("autorNomeado");
	}

	// ── AddAsync ───────────────────────────────────────────────────────────────

	[Fact]
	public async Task AddAsync_DeveInserirPost_EAtribuirId()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var (_, userId) = await SeedUserAsync(context);

		var repo = new PostRepository(context);
		var post = new Post { Title = "Novo", Content = "Conteúdo", UserId = userId };

		// Act
		await repo.AddAsync(post);
		await repo.SaveChangesAsync();

		// Assert
		post.Id.Should().BeGreaterThan(0);
		context.Posts.Should().ContainSingle(p => p.Title == "Novo");
	}

	// ── UpdateAsync ────────────────────────────────────────────────────────────

	[Fact]
	public async Task UpdateAsync_DeveAtualizarCampos_NoBanco()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var (_, userId) = await SeedUserAsync(context);

		var post = new Post { Title = "Original", Content = "Conteúdo Original", UserId = userId };
		context.Posts.Add(post);
		await context.SaveChangesAsync();

		var repo = new PostRepository(context);

		// Act
		post.Title   = "Atualizado";
		post.Content = "Conteúdo Atualizado";
		await repo.UpdateAsync(post);
		await repo.SaveChangesAsync();

		// Assert
		var updated = await repo.GetByIdAsync(post.Id);
		updated!.Title.Should().Be("Atualizado");
		updated.Content.Should().Be("Conteúdo Atualizado");
	}

	// ── DeleteAsync ────────────────────────────────────────────────────────────

	[Fact]
	public async Task DeleteAsync_DeveRemoverPost_DoBanco()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var (_, userId) = await SeedUserAsync(context);

		var post = new Post { Title = "Para Excluir", Content = "C", UserId = userId };
		context.Posts.Add(post);
		await context.SaveChangesAsync();

		var postId = post.Id;
		var repo = new PostRepository(context);

		// Act
		await repo.DeleteAsync(post);
		await repo.SaveChangesAsync();

		// Assert
		var deleted = await repo.GetByIdAsync(postId);
		deleted.Should().BeNull();
	}

	[Fact]
	public async Task DeleteAsync_NaoDeveAfetar_OutrosPosts()
	{
		// Arrange
		using var context = DbContextFactory.Create();
		var (_, userId) = await SeedUserAsync(context);

		var post1 = new Post { Title = "Manter", Content = "C", UserId = userId };
		var post2 = new Post { Title = "Excluir", Content = "C", UserId = userId };
		context.Posts.AddRange(post1, post2);
		await context.SaveChangesAsync();

		var repo = new PostRepository(context);

		// Act
		await repo.DeleteAsync(post2);
		await repo.SaveChangesAsync();

		// Assert
		var remaining = (await repo.GetAllAsync()).ToList();
		remaining.Should().HaveCount(1);
		remaining.First().Title.Should().Be("Manter");
	}
}
