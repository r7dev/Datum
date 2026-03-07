using Datum.Domain.Entities;
using Datum.Domain.Interfaces.Repositories;
using Datum.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Datum.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
	private readonly ApplicationDbContext _context = context;

	public async Task<User?> GetByIdAsync(int id) =>
		await _context.Users.FindAsync(id);

	public async Task<User?> GetByEmailAsync(string email) =>
		await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

	public async Task<bool> ExistsByEmailAsync(string email) =>
		await _context.Users.AnyAsync(u => u.Email == email);

	public async Task<bool> ExistsByUsernameAsync(string username) =>
		await _context.Users.AnyAsync(u => u.Username == username);

	public async Task<User> AddAsync(User user)
	{
		await _context.Users.AddAsync(user);
		return user;
	}

	public async Task SaveChangesAsync() =>
		await _context.SaveChangesAsync();
}
