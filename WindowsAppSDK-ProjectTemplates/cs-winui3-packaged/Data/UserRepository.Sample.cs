using BlankApp.Models;
using Microsoft.Extensions.Logging;

namespace BlankApp.Data;

/// <summary>
/// Repository implementation for user data access.
/// Demonstrates proper logging, error handling, and data operations.
/// In real implementation, this would use Entity Framework or similar.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ILogger<UserRepository> _logger;
    // In real implementation: private readonly AppDbContext _context;
    private static readonly List<User> _inMemoryStore = new(); // Demo only

    public UserRepository(ILogger<UserRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a user by their ID.
    /// </summary>
    public async Task<User?> GetByIdAsync(int id)
    {
        _logger.LogTrace("Entering GetByIdAsync with id: {Id}", id);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // In real implementation:
            // var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            
            var user = _inMemoryStore.FirstOrDefault(u => u.Id == id);
            
            _logger.LogInformation("GetByIdAsync completed in {Duration}ms for id: {Id}, found: {Found}", 
                sw.ElapsedMilliseconds, id, user != null);
            
            return await Task.FromResult(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByIdAsync failed after {Duration}ms for id: {Id}", 
                sw.ElapsedMilliseconds, id);
            throw;
        }
    }

    /// <summary>
    /// Retrieves all users.
    /// </summary>
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        _logger.LogTrace("Entering GetAllAsync");
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // In real implementation:
            // var users = await _context.Users.ToListAsync();
            
            var users = _inMemoryStore.ToList();
            
            _logger.LogInformation("GetAllAsync completed in {Duration}ms, count: {Count}", 
                sw.ElapsedMilliseconds, users.Count);
            
            return await Task.FromResult(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllAsync failed after {Duration}ms", sw.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Creates a new user in the database.
    /// </summary>
    public async Task<int> CreateAsync(User user)
    {
        _logger.LogTrace("Entering CreateAsync for user: {UserName}", user.Name);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // In real implementation:
            // _context.Users.Add(user);
            // await _context.SaveChangesAsync();
            
            user.Id = _inMemoryStore.Count + 1;
            user.CreatedAt = DateTime.UtcNow;
            _inMemoryStore.Add(user);
            
            _logger.LogInformation("CreateAsync completed in {Duration}ms, new id: {Id}", 
                sw.ElapsedMilliseconds, user.Id);
            
            return await Task.FromResult(user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateAsync failed after {Duration}ms", sw.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    public async Task UpdateAsync(User user)
    {
        _logger.LogTrace("Entering UpdateAsync for user id: {Id}", user.Id);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // In real implementation:
            // _context.Users.Update(user);
            // await _context.SaveChangesAsync();
            
            var existing = _inMemoryStore.FirstOrDefault(u => u.Id == user.Id);
            if (existing != null)
            {
                _inMemoryStore.Remove(existing);
                user.UpdatedAt = DateTime.UtcNow;
                _inMemoryStore.Add(user);
            }
            
            _logger.LogInformation("UpdateAsync completed in {Duration}ms for id: {Id}", 
                sw.ElapsedMilliseconds, user.Id);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateAsync failed after {Duration}ms", sw.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        _logger.LogTrace("Entering DeleteAsync for id: {Id}", id);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // In real implementation:
            // var user = await _context.Users.FindAsync(id);
            // if (user != null)
            // {
            //     _context.Users.Remove(user);
            //     await _context.SaveChangesAsync();
            // }
            
            var user = _inMemoryStore.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _inMemoryStore.Remove(user);
            }
            
            _logger.LogInformation("DeleteAsync completed in {Duration}ms for id: {Id}", 
                sw.ElapsedMilliseconds, id);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteAsync failed after {Duration}ms", sw.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email)
    {
        _logger.LogTrace("Entering GetByEmailAsync with email: {Email}", email);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // In real implementation:
            // var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            var user = _inMemoryStore.FirstOrDefault(u => u.Email == email);
            
            _logger.LogInformation("GetByEmailAsync completed in {Duration}ms, found: {Found}", 
                sw.ElapsedMilliseconds, user != null);
            
            return await Task.FromResult(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByEmailAsync failed after {Duration}ms", sw.ElapsedMilliseconds);
            throw;
        }
    }
}
