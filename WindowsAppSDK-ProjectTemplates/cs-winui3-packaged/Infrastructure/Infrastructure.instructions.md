# Infrastructure Layer - Cross-cutting Concerns

## Purpose
Application infrastructure: logging configuration, DI setup, and cross-cutting concerns.

## Files

### LoggingConfiguration.cs
Configures logging with multiple destinations:

**Destinations:**
- **File**: All log levels, 4KB rotation, daily rolling
- **Windows Event Log**: Warning and above only

**File Locations:**
- **Debug**: `{ProjectFolder}/logs/app-{date}.log`
- **Release**: `%LocalAppData%/BlankApp/logs/app-{date}.log`

## Usage

In `App.xaml.cs`:
```csharp
private void ConfigureServices(IServiceCollection services)
{
    // Add logging first
    services.AddAppLogging();
    
    // Then add your services...
    services.AddSingleton<IUserRepository, UserRepository>();
    services.AddSingleton<IUserService, UserService>();
    services.AddTransient<MainViewModel>();
}
```

## Logging Best Practices

**In any class:**
```csharp
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public async Task<User> GetUserAsync(int id)
    {
        _logger.LogTrace("Entering GetUserAsync with id: {Id}", id);
        var sw = Stopwatch.StartNew();
        
        try
        {
            var user = await _repository.GetByIdAsync(id);
            _logger.LogInformation("GetUserAsync completed in {Duration}ms", sw.ElapsedMilliseconds);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserAsync failed after {Duration}ms", sw.ElapsedMilliseconds);
            throw;
        }
    }
}
```

## Log Levels

| Level | Use For | Goes To |
|-------|---------|---------|
| Trace | Method entry/exit | File only |
| Debug | Detailed diagnostics | File only |
| Information | Normal operations | File only |
| Warning | Potential issues | File + Event Log |
| Error | Failures | File + Event Log |
| Critical | App-breaking issues | File + Event Log |

## Packages Used

- `Microsoft.Extensions.Logging` - Core abstraction (Microsoft)
- `Microsoft.Extensions.Logging.EventLog` - Windows Event Log (Microsoft)
- `Serilog.Extensions.Logging` - Integrates Serilog with MS Logging
- `Serilog.Sinks.File` - File logging with rotation
