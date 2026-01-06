# Windows App SDK Project - Architecture Guide

## 📋 Read This First

This project uses **layered architecture** with **co-located tests**. Before coding:
1. **Identify the layer** you're working in
2. **Read that layer's instructions** (`[Layer]/[Layer].instructions.md`)
3. **Follow SOLID, DRY, KISS principles**
4. **Create tests beside source** (`{Class}.cs` → `{Class}.Test.cs`)

## 🎯 Core Principles (MANDATORY)

### SOLID Principles
- **Single Responsibility**: Each class has ONE reason to change
- **Open/Closed**: Open for extension, closed for modification  
- **Liskov Substitution**: Derived classes must be substitutable for base
- **Interface Segregation**: Many specific interfaces > one general interface
- **Dependency Inversion**: Depend on abstractions (interfaces), inject via constructor

### DRY (Don't Repeat Yourself)
- Extract common logic into shared methods/services
- Use inheritance or composition for shared behavior
- Create helper/utility classes for repeated operations
- **Never copy-paste code** - refactor instead

### KISS (Keep It Simple, Stupid)
- Choose the simplest solution that works
- Avoid premature optimization
- Favor readability over cleverness
- If it's hard to explain, it's too complex

## 🗂️ Architecture - Layered Design

Each layer has detailed instructions - **READ BEFORE CODING!**

| Layer | Purpose | Depends On | Instructions |
|-------|---------|-----------|
| **Infrastructure** | Logging, DI setup | Nothing | [`Infrastructure.instructions.md`](Infrastructure/Infrastructure.instructions.md) |
| **Views** | UI (XAML, code-behind) | ViewModels | [`Views.instructions.md`](Views/Views.instructions.md) |
| **ViewModels** | UI state, commands | Services + Models | [`ViewModels.instructions.md`](ViewModels/ViewModels.instructions.md) |
| **Services** | Business logic | Data + Models | [`Services.instructions.md`](Services/Services.instructions.md) |
| **Data** | Repositories, APIs | Models | [`Data.instructions.md`](Data/Data.instructions.md) |
| **Models** | Entities, DTOs | Nothing | [`Models.instructions.md`](Models/Models.instructions.md) |

**Dependency Flow:** Views → ViewModels → Services → Data → Models (NEVER violate)

## Code Samples
- See per-layer sample files in each folder (e.g., `*.Sample.cs` and `*.Sample.Test.cs`) for concrete patterns.

### DI and Logging Setup (App.xaml.cs)
- Configure services with `AddAppLogging()` from Infrastructure.
- Register sample implementations for DI: `IUserRepository` → `UserRepository`, `IUserService` → `UserService`, and `MainViewModel`.
- **Prefer constructor injection** for all classes except UI components (Pages/Controls).
- Use `App.GetService<T>()` **only** when constructor injection is not possible (e.g., in XAML-instantiated Pages that require parameterless constructors).
- Global exception hooks log critical failures (UI, task, AppDomain).

### Dependency Injection Best Practices
**✅ DO: Use Constructor Injection (Preferred)**
```csharp
// Services, ViewModels, Data classes
public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    // Dependencies are explicit and testable
    public UserService(IUserRepository repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}
```

**⚠️ USE WITH CAUTION: Service Locator Pattern**
```csharp
// Only for XAML-instantiated Pages/Controls
public MainPage()
{
    InitializeComponent();
    
    // Document why GetService is necessary here:
    // MainPage is instantiated by XAML framework and cannot use constructor injection
    _logger = App.GetService<ILogger<MainPage>>();
    ViewModel = App.GetService<MainViewModel>();
}
```

**❌ DON'T: Use GetService in classes that support constructor injection**
```csharp
// BAD - ViewModels, Services, Data classes should use constructor injection
public class BadViewModel
{
    public BadViewModel()
    {
        var service = App.GetService<IUserService>(); // ❌ Hidden dependency
    }
}
```

## 🧪 Co-located Testing

Tests **beside source files**: `UserService.cs` → `UserService.Test.cs`

**Run:** `dotnet test .\Tests\BlankApp.Tests.csproj --filter "FullyQualifiedName~UserService"`

## 📝 Logging Requirements

Every operation MUST log: entry, exit with duration, errors.
**Log rotation:** 4KB max per file.

## 🔄 Development Workflow

1. Identify layer → Read `[Layer].instructions.md`
2. Design solution (SOLID/DRY/KISS)
3. Create interface (Dependency Inversion)
4. Implement `{Class}.cs` + test `{Class}.Test.cs` (same folder)
5. Add logging, run tests

## ⚠️ Anti-Patterns (Violations will be FLAGGED)

### SOLID Violations
❌ God classes (>500 lines or >10 methods)  
❌ Tight coupling (direct instantiation)  
❌ Fat interfaces (>5-7 methods)  
❌ Concrete dependencies in constructors

### DRY Violations  
❌ Copy-pasted code  
❌ Duplicated validation  
❌ Multiple hardcoded strings  

### KISS Violations
❌ Overly clever one-liners  
❌ Premature optimization  
❌ Complex inheritance (>3 levels)  
❌ Methods >50 lines

### Layer Violations
❌ Views → Services (must go through ViewModels)  
❌ ViewModels → Data (must go through Services)  
❌ Any layer → Views

## Quick Commands

```bash
# Build (WinUI3 requires platform)
dotnet build BlankApp.csproj -p:Platform=x64

# Run all tests
dotnet test .\Tests\BlankApp.Tests.csproj -p:Platform=x64

# Run specific class tests
dotnet test .\Tests\BlankApp.Tests.csproj -p:Platform=x64 --filter "FullyQualifiedName~UserService"
```

## Sparse identity quick start

**When to use sparse identity:**
- You need package identity (unpackaged app) for package-only APIs (e.g., AppData folders, package-dependent features, Windows AI APIs: https://learn.microsoft.com/en-us/windows/ai/apis/)

**Dev/debug (no MSIX needed):**
```powershell
$manifest = Resolve-Path "..\PackageIdentity\AppxManifest.xml"
$ext = Resolve-Path ".\bin\x64\Debug\net8.0-windows10.0.19041.0"
Add-AppxPackage -Register $manifest -ExternalLocation $ext -ForceApplicationShutdown
```
(Run from BlankApp folder; ensure bin output exists by building first: `dotnet build -p:Platform=x64`)

**Alternative (MSIX path):**
- Run `..\PackageIdentity\BuildSparsePackage.ps1 -PackageName BlankApp -Publisher "CN=Dev Publisher"` then install with:
  ```powershell
	$msix = Resolve-Path "..\PackageIdentity\publish\Release\x64\BlankApp.Sparse.msix"
	$ext = Resolve-Path ".\bin\x64\Debug\net8.0-windows10.0.19041.0"
	Add-AppxPackage -Path $msix -ExternalLocation $ext
  ```

**Re-register only when manifest/identity changes; otherwise reuse the installed sparse identity.**

## Summary

✓ Read layer instructions before coding  
✓ Follow SOLID/DRY/KISS principles  
✓ Co-locate tests beside source  
✓ Use dependency injection  
✓ Log entry/exit/errors

# Language style rules
- Always enforce repo analyzers: root `.editorconfig` plus any `stylecop.json`.
- C# code follows StyleCop.Analyzers and Microsoft.CodeAnalysis.NetAnalyzers.
- Markdown files wrap at 80 characters and use ATX headers with fenced code
	blocks that include language tags.
- YAML files indent two spaces and add comments for complex settings while
	keeping keys clear.
- PowerShell scripts use Verb-Noun names and prefer single-quoted literals
	while documenting parameters and satisfying PSScriptAnalyzer.
