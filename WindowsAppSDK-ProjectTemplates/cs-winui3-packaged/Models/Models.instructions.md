# Models - Domain Models

## Purpose
This folder contains domain models (DTOs, entities) shared across all layers.

## Rules

### ✅ DO:
- Keep models as POCOs (Plain Old CLR Objects)
- Use data annotations for validation attributes
- Include XML documentation for properties
- Use records for immutable data
- Implement equality comparison when needed

### ❌ DON'T:
- Put business logic in models
- Reference any other layer (Services, ViewModels, Views)
- Include infrastructure concerns (database-specific attributes should be in Data layer mapping)

## File Naming Conventions
- Domain entities: `[Name].cs`
- DTOs: `[Name]Dto.cs`
- View models data: `[Name]Data.cs`

## Code Samples

**Complete working example:**
- **Domain Models**: [`User.Sample.cs`](User.Sample.cs) - Entity, DTOs, records

**Quick template for entity:**
```csharp
/// <summary>
/// Represents a user in the system.
/// </summary>
public class User
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    [EmailAddress]
    public string Email { get; set; }
    
    [Range(0, 150)]
    public int Age { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}
```

## Code Template - DTO
```csharp
/// <summary>
/// Data transfer object for user creation.
/// </summary>
public record CreateUserDto(
    string Name,
    string Email,
    int Age
);

/// <summary>
/// Data transfer object for user response.
/// </summary>
public record UserDto
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
    public int Age { get; init; }
}
```

## Usage Across Layers

- **Data Layer**: Repositories return/accept these models ([`Data.instructions.md`](../Data/Data.instructions.md))
- **Services Layer**: Business logic operates on these models ([`Services.instructions.md`](../Services/Services.instructions.md))
- **ViewModels Layer**: ViewModels expose these models ([`ViewModels.instructions.md`](../ViewModels/ViewModels.instructions.md))
- **Views Layer**: UI binds to these models through ViewModels ([`Views.instructions.md`](../Views/Views.instructions.md))

## Testing

Models typically don't require unit tests unless they:
- Contain validation logic beyond data annotations
- Implement complex equality comparisons
- Have computed properties with logic
- Override ToString, GetHashCode, or Equals

If tests are needed, co-locate them: `User.cs` → `User.Test.cs`

## Design Principles

- **DRY**: Reuse common base classes for shared properties (e.g., `EntityBase` with Id, CreatedAt)
- **KISS**: Keep models simple - complex logic belongs in Services
- **SOLID**: Models should have single responsibility - data representation only
