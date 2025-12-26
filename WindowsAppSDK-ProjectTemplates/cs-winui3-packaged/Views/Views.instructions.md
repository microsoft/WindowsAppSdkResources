# Views Layer - UI Components

## Purpose
This folder contains all UI-related code: XAML files, code-behind, value converters, and custom controls.

## Rules

### ✅ DO:
- Keep code-behind minimal (UI-specific logic only)
- Bind to ViewModels via DataContext
- Use value converters for data transformation in UI
- Create reusable user controls
- Handle UI-specific events (loaded, unloaded, size changed)

### ❌ DON'T:
- Put business logic in code-behind
- Access Services or Data layers directly
- Perform data access operations
- Implement complex validation logic

## Dependencies
- **Depends on**: ViewModels layer ONLY
- **Dependency Injection**: Use constructor injection to receive ViewModels

## File Naming Conventions
- Pages: `[Name]Page.xaml` / `[Name]Page.xaml.cs`
- Controls: `[Name]Control.xaml` / `[Name]Control.xaml.cs`
- Converters: `[Name]Converter.cs`

## Testing

**Tests are CO-LOCATED with source files:**
- Test file location: `Views/{PageName}.Test.cs` (SAME FOLDER as source)
- Example: `MainPage.xaml.cs` → `MainPage.Test.cs`
- Test class name: `{PageName}Tests` (with 's' suffix)
- Mock ViewModels in tests
- Test UI bindings, converters, and control behavior

**Running Tests:**
```bash
# Run all Views tests
dotnet test .\Tests\BlankApp.Tests.csproj --filter "FullyQualifiedName~Views"

# Run specific page tests
dotnet test .\Tests\BlankApp.Tests.csproj --filter "FullyQualifiedName~MainPage"
```

## Example Structure
```
Views/
├── MainPage.xaml
├── MainPage.xaml.cs
├── SettingsPage.xaml
├── SettingsPage.xaml.cs
├── Controls/
│   ├── CustomButton.xaml
│   └── CustomButton.xaml.cs
└── Converters/
    ├── BoolToVisibilityConverter.cs
    └── DateTimeFormatConverter.cs
```

## Code Samples

**Complete working examples available:**
- **Code-Behind**: [`MainPage.Sample.cs`](MainPage.Sample.cs) - MVVM pattern, minimal code-behind, DI
- **UI Tests**: [`MainPage.Test.Sample.cs`](MainPage.Test.Sample.cs) - UI testing approaches and notes

These samples demonstrate:
- ✅ ViewModel injection via DI
- ✅ Minimal code-behind (MVVM pattern)
- ✅ Proper event handling
- ✅ DataContext setup
- ✅ Includes example XAML structure in comments

**Quick template for new page:****
```csharp
public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<MainViewModel>();
        DataContext = ViewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // UI-specific initialization only
    }
}
```

**Cross-layer references:**
- ViewModels layer: [`../ViewModels/ViewModels.instructions.md`](../ViewModels/ViewModels.instructions.md)
- Complete samples: Check ViewModels folder for `*.Sample.cs` examples
