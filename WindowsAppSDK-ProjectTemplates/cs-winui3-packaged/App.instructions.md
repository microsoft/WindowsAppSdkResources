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
|-------|---------|-----------|--------------|
| **Infrastructure** | Logging, DI setup | Nothing | [`Infrastructure.instructions.md`](Infrastructure/Infrastructure.instructions.md) |
| **Views** | UI (XAML, code-behind) | ViewModels | [`Views.instructions.md`](Views/Views.instructions.md) |
| **ViewModels** | UI state, commands | Services + Models | [`ViewModels.instructions.md`](ViewModels/ViewModels.instructions.md) |
| **Services** | Business logic | Data + Models | [`Services.instructions.md`](Services/Services.instructions.md) |
| **Data** | Repositories, APIs | Models | [`Data.instructions.md`](Data/Data.instructions.md) |
| **Models** | Entities, DTOs | Nothing | [`Models.instructions.md`](Models/Models.instructions.md) |

**Dependency Flow:** Views → ViewModels → Services → Data → Models (NEVER violate)

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

## Summary

✓ Read layer instructions before coding  
✓ Follow SOLID/DRY/KISS principles  
✓ Co-locate tests beside source  
✓ Use dependency injection  
✓ Log entry/exit/errors
