using Microsoft.Extensions.Logging;

namespace BlankApp.Services;

/// <summary>
/// Interface for user service operations.
/// Following Dependency Inversion Principle - depend on abstractions.
/// </summary>
public interface IUserService
{
    Task<User> GetUserAsync(int userId);
    Task<bool> ValidateUserAsync(User user);
    Task<int> CreateUserAsync(User user);
}

/// <summary>
/// Service for user-related business logic.
/// Demonstrates SOLID principles, proper logging, and error handling.
/// </summary>
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;

    public UserService(ILogger<UserService> logger, IUserRepository userRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Retrieves a user by ID.
    /// Demonstrates logging (entry/exit/errors) and error handling.
    /// </summary>
    public async Task<User> GetUserAsync(int userId)
    {
        _logger.LogTrace("Entering GetUserAsync with userId: {UserId}", userId);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // Validation (business rule)
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID", nameof(userId));
            }

            // Call data layer
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return null;
            }

            _logger.LogInformation("GetUserAsync completed in {Duration}ms for user: {UserId}", 
                sw.ElapsedMilliseconds, userId);
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserAsync failed after {Duration}ms for user: {UserId}", 
                sw.ElapsedMilliseconds, userId);
            throw;
        }
    }

    /// <summary>
    /// Validates user data according to business rules.
    /// Demonstrates Single Responsibility - only business logic, no data access.
    /// </summary>
    public async Task<bool> ValidateUserAsync(User user)
    {
        _logger.LogTrace("Entering ValidateUserAsync");
        
        if (user == null)
        {
            _logger.LogWarning("ValidateUserAsync: User is null");
            return false;
        }

        // Business validation rules
        var isValid = !string.IsNullOrWhiteSpace(user.Name) && 
                     user.Age >= 0 && 
                     user.Age <= 150 &&
                     !string.IsNullOrWhiteSpace(user.Email);
        
        _logger.LogInformation("ValidateUserAsync result: {IsValid} for user: {UserName}", 
            isValid, user.Name);
        
        return await Task.FromResult(isValid);
    }

    /// <summary>
    /// Creates a new user after validation.
    /// Demonstrates orchestration of validation and data operations.
    /// </summary>
    public async Task<int> CreateUserAsync(User user)
    {
        _logger.LogTrace("Entering CreateUserAsync");
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // Validate first (business logic)
            if (!await ValidateUserAsync(user))
            {
                throw new ArgumentException("User validation failed");
            }

            // Create in data layer
            var userId = await _userRepository.CreateAsync(user);
            
            _logger.LogInformation("CreateUserAsync completed in {Duration}ms, new userId: {UserId}", 
                sw.ElapsedMilliseconds, userId);
            
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateUserAsync failed after {Duration}ms", 
                sw.ElapsedMilliseconds);
            throw;
        }
    }
}
