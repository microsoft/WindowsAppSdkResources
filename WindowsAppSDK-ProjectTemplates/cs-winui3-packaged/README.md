# Windows App SDK Template with Co-located Tests

This template implements a **layered architecture** with **co-located test files** for optimal Copilot integration and developer experience.

## 🎯 Key Features

✅ **Co-located Tests**: Test files beside source files (`UserService.cs` → `UserService.Test.cs`)  
✅ **Separate Build Projects**: Shipping build excludes tests automatically  
✅ **SOLID/DRY/KISS Principles**: Enforced through Copilot instructions  
✅ **Incremental Testing**: Automatic test discovery and execution  
✅ **Comprehensive Logging**: 4KB log rotation with structured logging  

## 📁 Project Structure

```
BlankApp/
├── Views/                      # UI Layer
│   ├── MainPage.xaml
│   ├── MainPage.xaml.cs
│   └── MainPage.Test.cs        ← Test beside source
├── ViewModels/                 # Presentation Logic
│   ├── MainViewModel.cs
│   └── MainViewModel.Test.cs   ← Test beside source
├── Services/                   # Business Logic
│   ├── UserService.cs
│   ├── UserService.Test.cs     ← Test beside source
│   ├── UserService.Sample.cs   ← Production code example (not compiled)
│   └── UserService.Test.Sample.cs ← Test code example (not compiled)
├── Data/                       # Data Access
│   ├── UserRepository.cs
│   └── UserRepository.Test.cs  ← Test beside source
├── Models/                     # Domain Models
│   └── User.cs
├── Tests/
│   └── BlankApp.Tests.csproj  # Test project (references parent *.Test.cs)
└── BlankApp.csproj            # Shipping (excludes *.Test.cs)
```

## 🏗️ Build Configuration

### BlankApp.csproj (Shipping Build)
Excludes all `*.Test.cs` files and Tests folder:
```xml
<ItemGroup>
  <Compile Remove="**\*.Test.cs" />
  <Compile Remove="Tests\**" />
  <None Include="**\*.Test.cs" />
</ItemGroup>
```

### Tests\BlankApp.Tests.csproj (Test Build)
Includes only `*.Test.cs` files from parent folders:
```xml
<ItemGroup>
  <Compile Include="..\..\**\*.Test.cs" LinkBase="Tests" Exclude="..\..\bin\**;..\..\obj\**;..\..\Tests\**" />
</ItemGroup>
```

**Note**: Test project is in separate `Tests` folder to avoid bin/obj conflicts.

## 🧪 Testing Workflow

### 1. Create Source File
```csharp
// Services/UserService.cs
public class UserService : IUserService
{
    // Implementation
}
```

### 2. Create Test File (Same Folder)
```csharp
// Services/UserService.Test.cs
public class UserServiceTests
{
    [Fact]
    public async Task GetUserAsync_WhenExists_ShouldReturnUser()
    {
        // Test implementation
    }
}
```

### 3. Run Tests Incrementally
```bash
# Run all tests
dotnet test .\Tests\BlankApp.Tests.csproj

# Run tests for specific class (incremental)
dotnet test .\Tests\BlankApp.Tests.csproj --filter "FullyQualifiedName~UserService"
```

## 📖 Layer Instructions

Each layer has detailed instructions:

- **Views**: [`Views/Views.instructions.md`](Views/Views.instructions.md)
- **ViewModels**: [`ViewModels/ViewModels.instructions.md`](ViewModels/ViewModels.instructions.md)
- **Services**: [`Services/Services.instructions.md`](Services/Services.instructions.md)
- **Data**: [`Data/Data.instructions.md`](Data/Data.instructions.md)
- **Models**: [`Models/Models.instructions.md`](Models/Models.instructions.md)

## 🤖 Copilot Integration

See [`App.instructions.md`](App.instructions.md) for complete Copilot guidelines.

**Key Features:**
- Automatic test discovery (test file always beside source)
- Incremental test execution
- SOLID/DRY/KISS principle enforcement
- Breaking change detection
- Auto-fix trivial test failures

## 🏃 Quick Start

### Build and Run
```pwsh
# Build shipping version (excludes tests)
dotnet build BlankApp.csproj

# Run tests
dotnet test .\Tests\BlankApp.Tests.csproj

# Run with coverage
dotnet test .\Tests\BlankApp.Tests.csproj /p:CollectCoverage=true
```

### Verify Test Exclusion
```pswh
# Should return nothing (tests excluded from shipping build)
dotnet build BlankApp.csproj -v:d | Select-String "\.Test\.cs"
```

## 🎨 VS Code / Visual Studio

Both IDEs handle this structure perfectly:

### VS Code
- Test Explorer automatically discovers tests
- Files grouped by folder
- Side-by-side editing of source + test

### Visual Studio
- Test Explorer works out of the box
- Solution Explorer shows logical structure
- Test coverage integration

## 🔗 Dependencies

### Main Project (BlankApp.csproj)
- Microsoft.WindowsAppSDK

### Test Project (BlankApp.Tests.csproj)
- xUnit (testing framework)
- NSubstitute (mocking)
- FluentAssertions (test readability)
- Coverlet (code coverage)

## 📝 Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Source File | `{ClassName}.cs` | `UserService.cs` |
| Test File | `{ClassName}.Test.cs` | `UserService.Test.cs` |
| Test Class | `{ClassName}Tests` | `UserServiceTests` |
| Test Method | `{Method}_{Scenario}_{ExpectedBehavior}` | `GetUserAsync_WhenExists_ShouldReturnUser` |

## 🎯 Benefits

### For Copilot
1. **Instant Discovery**: Test file always beside source
2. **Easy Identification**: Missing tests = missing `.Test.cs` file
3. **Atomic Changes**: Modify source + test in one location
4. **Clear Patterns**: Predictable naming conventions

### For Developers
1. **Proximity**: Source and tests side-by-side
2. **No Navigation**: No switching between projects
3. **Discoverability**: Easy to see what has tests
4. **IDE Support**: Works with VS Code and Visual Studio

## ⚠️ Important Notes

1. **Always** create `{ClassName}.Test.cs` beside `{ClassName}.cs`
2. **Always** read layer-specific instructions before coding
3. **Always** run incremental tests after changes
4. **Never** include test code in shipping build (automatic)

## 🤝 Contributing

Follow the layered architecture and co-located test pattern. Read the Copilot instructions for detailed guidelines.

## 📄 License

This template follows the same license as Windows App SDK.
