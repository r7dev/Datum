using Datum.Domain.Entities;

namespace Datum.Domain.Interfaces.Repositories;

public interface IUserRepository
{
	Task<User?> GetByIdAsync(int id);
	Task<User?> GetByEmailAsync(string email);
	Task<bool> ExistsByEmailAsync(string email);
	Task<bool> ExistsByUsernameAsync(string username);
	Task<User> AddAsync(User user);
	Task SaveChangesAsync();
}
