# Tests Folder - Test Project Configuration

## Purpose

This folder contains the **test project configuration** (`BlankApp.Tests.csproj`) that references all `*.Test.cs` files from parent layer folders.

## Why Separate Folder?

Having the test project in a separate `Tests/` folder avoids bin/obj conflicts:
- ✅ `BlankApp.csproj` → outputs to `bin/`, `obj/`
- ✅ `Tests/BlankApp.Tests.csproj` → outputs to `Tests/bin/`, `Tests/obj/`
- ✅ No conflicts or mixed outputs

## Test File Locations (Co-located)

Test files remain **beside their source files** in layer folders:

```
Views/
├── MainPage.xaml.cs        ← Source
└── MainPage.Test.cs        ← Test (co-located)

Services/
├── UserService.cs          ← Source
└── UserService.Test.cs     ← Test (co-located)

Data/
├── UserRepository.cs       ← Source
└── UserRepository.Test.cs  ← Test (co-located)
```

## Project Configuration

### BlankApp.Tests.csproj (This Project)

Includes `*.Test.cs` files from parent folders:

```xml
<ItemGroup>
  <!-- Reference the main project (one level up) -->
  <ProjectReference Include="..\BlankApp.csproj" />
</ItemGroup>

<ItemGroup>
  <!-- Include test files from co-located layer folders -->
  <Compile Include="..\Views\**\*.Test.cs" LinkBase="Views" Exclude="..\**\*.Sample.cs" />
  <Compile Include="..\ViewModels\**\*.Test.cs" LinkBase="ViewModels" Exclude="..\**\*.Sample.cs" />
  <Compile Include="..\Services\**\*.Test.cs" LinkBase="Services" Exclude="..\**\*.Sample.cs" />
  <Compile Include="..\Data\**\*.Test.cs" LinkBase="Data" Exclude="..\**\*.Sample.cs" />
  <Compile Include="..\Models\**\*.Test.cs" LinkBase="Models" Exclude="..\**\*.Sample.cs" />
  <Compile Include="..\Infrastructure\**\*.Test.cs" LinkBase="Infrastructure" Exclude="..\**\*.Sample.cs" />
</ItemGroup>
```

**Path Explanation:**
- Each layer folder is explicitly included
- Excludes `*.Sample.cs` (example files not compiled)

## Running Tests

### From Solution Root

**Important:** WinUI3 projects require platform specification.

```bash
# Run all tests
dotnet test .\Tests\BlankApp.Tests.csproj -p:Platform=x64

# Run tests for specific class (incremental)
dotnet test .\Tests\BlankApp.Tests.csproj --filter "FullyQualifiedName~UserService"

# Run specific test method
dotnet test .\Tests\BlankApp.Tests.csproj --filter "FullyQualifiedName~UserServiceTests.GetUserAsync_WhenExists_ShouldReturnUser"
```

### From Tests Folder

```bash
cd Tests

# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Copilot Integration

Copilot automatically knows:
1. Test files are beside source files (co-located)
2. Test project is at `.\Tests\BlankApp.Tests.csproj`
3. To run incremental tests: `dotnet test .\Tests\BlankApp.Tests.csproj --filter "FullyQualifiedName~{ClassName}"`

## Build Outputs

```
BlankApp/
├── bin/                    ← Main project output
├── obj/                    ← Main project intermediate
├── Tests/
│   ├── bin/                ← Test project output
│   ├── obj/                ← Test project intermediate
│   └── BlankApp.Tests.csproj
```

**No conflicts!** Each project has its own build output directory.

## Adding New Tests

1. **Create source**: `Services/UserService.cs`
2. **Create test (same folder)**: `Services/UserService.Test.cs`
3. **Run**: `dotnet test .\Tests\BlankApp.Tests.csproj -p:Platform=x64 --filter "FullyQualifiedName~UserService"`

## Dependencies

- MSTest (Microsoft's test framework)
- Moq (mocking library)

## Important Notes

⚠️ **Never put source code in this folder** - only test project configuration  
✅ **Test files stay beside source files** in layer folders  
✅ **This project only contains BlankApp.Tests.csproj**
