using BlankApp.Models;

namespace BlankApp.Data;

/// <summary>
/// Repository interface for user data access operations.
/// Following Repository Pattern and Dependency Inversion Principle.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<int> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
    Task<User?> GetByEmailAsync(string email);
}
