using Datum.Domain.DTOs;
using Datum.Domain.Entities;
using Datum.Domain.Interfaces.Repositories;
using Datum.Domain.Interfaces.Services;

namespace Datum.Infrastructure.Services;

public class PostService(IPostRepository postRepository, INotificationService notificationService) : IPostService
{
	private readonly IPostRepository _postRepository = postRepository;
	private readonly INotificationService _notificationService = notificationService;

	public async Task<IEnumerable<PostResponse>> GetAllPostsAsync()
	{
		var posts = await _postRepository.GetAllAsync();
		return posts.Select(MapToResponse);
	}

	public async Task<PostResponse?> GetPostByIdAsync(int id)
	{
		var post = await _postRepository.GetByIdAsync(id);
		return post == null ? null : MapToResponse(post);
	}

	public async Task<PostResponse> CreatePostAsync(CreatePostRequest request, int userId)
	{
		var post = new Post
		{
			Title   = request.Title,
			Content = request.Content,
			UserId  = userId
		};

		await _postRepository.AddAsync(post);
		await _postRepository.SaveChangesAsync();

		// Recarrega com Author navegado
		var saved = await _postRepository.GetByIdAsync(post.Id)
			?? throw new InvalidOperationException("Erro ao recuperar post após criação.");

		var notification = new PostNotification(saved.Id, saved.Title, saved.Author.Username, saved.CreatedAt);
		await _notificationService.NotifyNewPostAsync(notification);

		return MapToResponse(saved);
	}

	public async Task<PostResponse> UpdatePostAsync(int postId, UpdatePostRequest request, int userId)
	{
		var post = await _postRepository.GetByIdAsync(postId)
			?? throw new KeyNotFoundException($"Post {postId} não encontrado.");

		if (post.UserId != userId)
			throw new UnauthorizedAccessException("Você só pode editar suas próprias postagens.");

		post.Title     = request.Title;
		post.Content   = request.Content;
		post.UpdatedAt = DateTime.UtcNow;

		await _postRepository.UpdateAsync(post);
		await _postRepository.SaveChangesAsync();

		return MapToResponse(post);
	}

	public async Task DeletePostAsync(int postId, int userId)
	{
		var post = await _postRepository.GetByIdAsync(postId)
			?? throw new KeyNotFoundException($"Post {postId} não encontrado.");

		if (post.UserId != userId)
			throw new UnauthorizedAccessException("Você só pode excluir suas próprias postagens.");

		await _postRepository.DeleteAsync(post);
		await _postRepository.SaveChangesAsync();
	}

	private static PostResponse MapToResponse(Post post) => new(
		post.Id,
		post.Title,
		post.Content,
		post.Author?.Username ?? "Desconhecido",
		post.UserId,
		post.CreatedAt,
		post.UpdatedAt
	);
}
