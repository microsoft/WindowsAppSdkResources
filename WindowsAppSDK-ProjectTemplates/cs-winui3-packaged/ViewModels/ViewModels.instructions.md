# ViewModels Layer - Presentation Logic

## Purpose
This folder contains MVVM view models that manage UI state, commands, and orchestrate calls to Services layer.

## Rules

### ✅ DO:
- Implement INotifyPropertyChanged (or use base class)
- Use RelayCommand/DelegateCommand for actions
- Validate user input
- Transform data for UI presentation
- Call Services layer for business operations
- Log state changes and command executions

### ❌ DON'T:
- Access Data layer directly (use Services instead)
- Perform database operations
- Make HTTP calls directly
- Reference UI elements (Views)

## Dependencies
- **Depends on**: Services layer, Models
- **Dependency Injection**: Constructor inject Service interfaces

## Required Logging
```csharp
_logger.LogTrace("Entering {Method}", nameof(LoadDataAsync));
_logger.LogInformation("{Method} completed in {Duration}ms", nameof(LoadDataAsync), sw.ElapsedMilliseconds);
_logger.LogError(ex, "{Method} failed", nameof(LoadDataAsync));
```

## Testing

**Tests are CO-LOCATED with source files:**
- Test file location: `ViewModels/{ViewModelName}.Test.cs` (SAME FOLDER as source)
- Example: `MainViewModel.cs` → `MainViewModel.Test.cs`
- Test class name: `{ViewModelName}Tests` (with 's' suffix)
- Mock all Service interfaces
- Test property changes, command execution, validation logic

**Running Tests:**
```bash
# Run all ViewModel tests
dotnet test .\Tests\BlankApp.Tests.csproj --filter "FullyQualifiedName~ViewModels"

# Run specific ViewModel tests
dotnet test .\Tests\BlankApp.Tests.csproj --filter "FullyQualifiedName~MainViewModel"
```

## File Naming Conventions
- ViewModels: `[Name]ViewModel.cs`
- Base class: `ViewModelBase.cs` or `ObservableObject.cs`

## Code Samples

**Complete working examples available:**
- Production: [`MainViewModel.Sample.cs`](MainViewModel.Sample.cs)
- Unit Tests: [`MainViewModel.Test.Sample.cs`](MainViewModel.Test.Sample.cs)

**Quick template for new ViewModel:**
```csharp
public class {Name}ViewModel : ObservableObject
{
    private readonly ILogger<{Name}ViewModel> _logger;
    private readonly I{Service}Service _service;
    private string _title;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public IRelayCommand LoadCommand { get; }

    public {Name}ViewModel(ILogger<{Name}ViewModel> logger, I{Service}Service service)
    {
        _logger = logger;
        _service = service;
        LoadCommand = new RelayCommand(async () => await LoadAsync(), () => !IsLoading);
    }
    
    // See MainViewModel.Sample.cs for complete implementation
}
```

**Cross-layer references:**
- Services layer: [`../Services/Services.instructions.md`](../Services/Services.instructions.md)
- Models layer: [`../Models/Models.instructions.md`](../Models/Models.instructions.md)
