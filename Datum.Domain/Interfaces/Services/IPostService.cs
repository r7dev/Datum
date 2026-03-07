using Datum.Domain.DTOs;

namespace Datum.Domain.Interfaces.Services;

public interface IPostService
{
	Task<IEnumerable<PostResponse>> GetAllPostsAsync();
	Task<PostResponse?> GetPostByIdAsync(int id);
	Task<PostResponse> CreatePostAsync(CreatePostRequest request, int userId);
	Task<PostResponse> UpdatePostAsync(int postId, UpdatePostRequest request, int userId);
	Task DeletePostAsync(int postId, int userId);
}
