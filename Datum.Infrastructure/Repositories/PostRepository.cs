using Datum.Domain.Entities;
using Datum.Domain.Interfaces.Repositories;
using Datum.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Datum.Infrastructure.Repositories;

public class PostRepository(ApplicationDbContext context) : IPostRepository
{
	private readonly ApplicationDbContext _context = context;

	public async Task<IEnumerable<Post>> GetAllAsync() =>
		await _context.Posts
			.Include(p => p.Author)
			.OrderByDescending(p => p.CreatedAt)
			.ToListAsync();

	public async Task<Post?> GetByIdAsync(int id) =>
		await _context.Posts
			.Include(p => p.Author)
			.FirstOrDefaultAsync(p => p.Id == id);

	public async Task<Post> AddAsync(Post post)
	{
		await _context.Posts.AddAsync(post);
		return post;
	}

	public Task UpdateAsync(Post post)
	{
		_context.Posts.Update(post);
		return Task.CompletedTask;
	}

	public Task DeleteAsync(Post post)
	{
		_context.Posts.Remove(post);
		return Task.CompletedTask;
	}

	public async Task SaveChangesAsync() =>
		await _context.SaveChangesAsync();
}
