namespace BlankApp.Models;

/// <summary>
/// Represents a user in the system.
/// This is a domain model used across all layers.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// User's full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User's age.
    /// </summary>
    public int Age { get; set; }
    
    /// <summary>
    /// When the user was created in the system.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When the user was last updated (null if never updated).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Data transfer object for creating a new user.
/// Demonstrates immutable record pattern for DTOs.
/// </summary>
public record CreateUserDto(
    string Name,
    string Email,
    int Age
);

/// <summary>
/// Data transfer object for user responses.
/// Demonstrates record with init-only properties.
/// </summary>
public record UserDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int Age { get; init; }
    public DateTime CreatedAt { get; init; }
}
