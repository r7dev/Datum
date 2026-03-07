using Datum.Domain.Entities;

namespace Datum.Domain.Interfaces.Repositories;

public interface IPostRepository
{
	Task<IEnumerable<Post>> GetAllAsync();
	Task<Post?> GetByIdAsync(int id);
	Task<Post> AddAsync(Post post);
	Task UpdateAsync(Post post);
	Task DeleteAsync(Post post);
	Task SaveChangesAsync();
}
