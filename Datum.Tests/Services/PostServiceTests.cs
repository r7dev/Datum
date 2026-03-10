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

public class PostServiceTests
{
	private readonly Mock<IPostRepository> _postRepositoryMock;
	private readonly Mock<INotificationService> _notificationServiceMock;
	private readonly PostService _sut;

	public PostServiceTests()
	{
		_postRepositoryMock     = new Mock<IPostRepository>();
		_notificationServiceMock = new Mock<INotificationService>();
		_sut = new PostService(_postRepositoryMock.Object, _notificationServiceMock.Object);
	}

	// ── GetAllPostsAsync ───────────────────────────────────────────────────────

	[Fact]
	public async Task GetAllPostsAsync_ComPostsExistentes_DeveRetornarListaMapeada()
	{
		// Arrange
		var posts = new List<Post>
		{
			EntityBuilder.BuildPost(id: 1, title: "Post A"),
			EntityBuilder.BuildPost(id: 2, title: "Post B", userId: 2, author: EntityBuilder.BuildUser(id: 2, username: "outro"))
		};

		_postRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(posts);

		// Act
		var result = (await _sut.GetAllPostsAsync()).ToList();

		// Assert
		result.Should().HaveCount(2);
		result[0].Title.Should().Be("Post A");
		result[1].Title.Should().Be("Post B");
		result[1].AuthorUsername.Should().Be("outro");
	}

	[Fact]
	public async Task GetAllPostsAsync_SemPosts_DeveRetornarListaVazia()
	{
		// Arrange
		_postRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

		// Act
		var result = await _sut.GetAllPostsAsync();

		// Assert
		result.Should().BeEmpty();
	}

	// ── GetPostByIdAsync ───────────────────────────────────────────────────────

	[Fact]
	public async Task GetPostByIdAsync_ComIdExistente_DeveRetornarPostResponse()
	{
		// Arrange
		var post = EntityBuilder.BuildPost(id: 1, title: "Meu Post");
		_postRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(post);

		// Act
		var result = await _sut.GetPostByIdAsync(1);

		// Assert
		result.Should().NotBeNull();
		result!.Id.Should().Be(1);
		result.Title.Should().Be("Meu Post");
	}

	[Fact]
	public async Task GetPostByIdAsync_ComIdInexistente_DeveRetornarNull()
	{
		// Arrange
		_postRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Post?)null);

		// Act
		var result = await _sut.GetPostByIdAsync(99);

		// Assert
		result.Should().BeNull();
	}

	// ── CreatePostAsync ────────────────────────────────────────────────────────

	[Fact]
	public async Task CreatePostAsync_ComDadosValidos_DeveRetornarPostResponse()
	{
		// Arrange
		var request = new CreatePostRequest("Novo Post", "Conteúdo do post");
		const int userId = 1;
		var author = EntityBuilder.BuildUser(id: userId);
		var savedPost = EntityBuilder.BuildPost(id: 10, title: request.Title, content: request.Content, userId: userId, author: author);

		_postRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Post>()))
						   .ReturnsAsync((Post p) => p);
		_postRepositoryMock.Setup(r => r.SaveChangesAsync())
						   .Returns(Task.CompletedTask);
		_postRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
						   .ReturnsAsync(savedPost);
		_notificationServiceMock.Setup(n => n.NotifyNewPostAsync(It.IsAny<PostNotification>()))
								.Returns(Task.CompletedTask);

		// Act
		var result = await _sut.CreatePostAsync(request, userId);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().Be(request.Title);
		result.Content.Should().Be(request.Content);
		result.AuthorId.Should().Be(userId);
	}

	[Fact]
	public async Task CreatePostAsync_DeveChamarNotifyNewPostAsync_AposCriar()
	{
		// Arrange
		var request  = new CreatePostRequest("Post Notificado", "Conteúdo");
		var author   = EntityBuilder.BuildUser(id: 1);
		var savedPost = EntityBuilder.BuildPost(id: 5, title: request.Title, userId: 1, author: author);

		_postRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Post>())).ReturnsAsync((Post p) => p);
		_postRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
		_postRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(savedPost);
		_notificationServiceMock.Setup(n => n.NotifyNewPostAsync(It.IsAny<PostNotification>()))
								.Returns(Task.CompletedTask);

		// Act
		await _sut.CreatePostAsync(request, 1);

		// Assert
		_notificationServiceMock.Verify(
			n => n.NotifyNewPostAsync(It.Is<PostNotification>(p => p.Title == request.Title)),
			Times.Once
		);
	}

	[Fact]
	public async Task CreatePostAsync_QuandoGetByIdRetornaNull_DeveLancarInvalidOperationException()
	{
		// Arrange
		_postRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Post>())).ReturnsAsync((Post p) => p);
		_postRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
		_postRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Post?)null);

		// Act
		var act = () => _sut.CreatePostAsync(new CreatePostRequest("T", "C"), 1);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
				 .WithMessage("Erro ao recuperar post após criação.");
	}

	// ── UpdatePostAsync ────────────────────────────────────────────────────────

	[Fact]
	public async Task UpdatePostAsync_ComAutorCorreto_DeveAtualizarERetornarPost()
	{
		// Arrange
		var post    = EntityBuilder.BuildPost(id: 1, title: "Título Antigo", userId: 1);
		var request = new UpdatePostRequest("Título Novo", "Conteúdo Novo");

		_postRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(post);
		_postRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);
		_postRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		// Act
		var result = await _sut.UpdatePostAsync(1, request, userId: 1);

		// Assert
		result.Title.Should().Be("Título Novo");
		result.Content.Should().Be("Conteúdo Novo");
	}

	[Fact]
	public async Task UpdatePostAsync_ComPostInexistente_DeveLancarKeyNotFoundException()
	{
		// Arrange
		_postRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Post?)null);

		// Act
		var act = () => _sut.UpdatePostAsync(99, new UpdatePostRequest("T", "C"), userId: 1);

		// Assert
		await act.Should().ThrowAsync<KeyNotFoundException>()
				 .WithMessage("Post 99 não encontrado.");
	}

	[Fact]
	public async Task UpdatePostAsync_ComUsuarioDiferenteDoAutor_DeveLancarUnauthorizedAccessException()
	{
		// Arrange
		var post = EntityBuilder.BuildPost(id: 1, userId: 1); // autor = user 1
		_postRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(post);

		// Act — tenta editar como user 2
		var act = () => _sut.UpdatePostAsync(1, new UpdatePostRequest("T", "C"), userId: 2);

		// Assert
		await act.Should().ThrowAsync<UnauthorizedAccessException>()
				 .WithMessage("Você só pode editar suas próprias postagens.");
	}

	[Fact]
	public async Task UpdatePostAsync_DeveDefinirUpdatedAt_AoAtualizar()
	{
		// Arrange
		var post = EntityBuilder.BuildPost(id: 1, userId: 1);
		_postRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(post);
		_postRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);
		_postRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		var antes = DateTime.UtcNow;

		// Act
		var result = await _sut.UpdatePostAsync(1, new UpdatePostRequest("T", "C"), userId: 1);

		// Assert
		result.UpdatedAt.Should().NotBeNull();
		result.UpdatedAt!.Value.Should().BeOnOrAfter(antes);
	}

	// ── DeletePostAsync ────────────────────────────────────────────────────────

	[Fact]
	public async Task DeletePostAsync_ComAutorCorreto_DeveExcluirPost()
	{
		// Arrange
		var post = EntityBuilder.BuildPost(id: 1, userId: 1);
		_postRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(post);
		_postRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);
		_postRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		// Act
		await _sut.DeletePostAsync(1, userId: 1);

		// Assert
		_postRepositoryMock.Verify(r => r.DeleteAsync(post), Times.Once);
		_postRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task DeletePostAsync_ComPostInexistente_DeveLancarKeyNotFoundException()
	{
		// Arrange
		_postRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Post?)null);

		// Act
		var act = () => _sut.DeletePostAsync(99, userId: 1);

		// Assert
		await act.Should().ThrowAsync<KeyNotFoundException>()
				 .WithMessage("Post 99 não encontrado.");
	}

	[Fact]
	public async Task DeletePostAsync_ComUsuarioDiferenteDoAutor_DeveLancarUnauthorizedAccessException()
	{
		// Arrange
		var post = EntityBuilder.BuildPost(id: 1, userId: 1); // autor = user 1
		_postRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(post);

		// Act — tenta excluir como user 2
		var act = () => _sut.DeletePostAsync(1, userId: 2);

		// Assert
		await act.Should().ThrowAsync<UnauthorizedAccessException>()
				 .WithMessage("Você só pode excluir suas próprias postagens.");
	}

	[Fact]
	public async Task DeletePostAsync_NaoDeveChamarDelete_QuandoPostNaoEncontrado()
	{
		// Arrange
		_postRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Post?)null);

		// Act
		try { await _sut.DeletePostAsync(1, 1); } catch { /* esperado */ }

		// Assert
		_postRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Post>()), Times.Never);
	}
}
