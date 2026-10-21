# Error: "Specified cast is not valid" - Custom Cursor with AOT Compilation

**Keywords:** Specified cast is not valid, AOT, custom cursor, dynamic cursor, CsWinRT, LibraryImport, DllImport, partial class

**Error Example:**
```
Specified cast is not valid
```

---

## Quick Match

**You're seeing this if:**
- Error contains "Specified cast is not valid"
- Using a custom cursor in a C# project
- AOT (Ahead-of-Time) compilation is enabled
- Platform: Windows 11 version 24H2 (26100, June 2025 Update)

→ Check scenarios below for your specific cause

---

## Related Issues

- [#5716](https://github.com/microsoft/WindowsAppSDK/issues/5716) - Custom Cursor AOT Error (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: Missing `partial` Keyword in Custom Classes

**Cause:** The custom button or other classes used in the project are not marked as `partial`, which is required for compatibility with AOT compilation.
> Source: @ghost1372 in [#5716](https://github.com/microsoft/WindowsAppSDK/issues/5716)

**Fix:**
1. Add the `partial` keyword to all custom classes. For example:
   ```csharp
   public partial class MyButton : Button
   ```
2. Ensure all relevant classes in your project are updated similarly.

> ✅ Confirmed by: @BlameTwo in issue comments

**Verify:** Rebuild the project with AOT enabled and confirm the error no longer occurs.

---

### Scenario 2: Hard-Coded Resource Paths in Project Files

**Cause:** Resource paths for the custom cursor are hard-coded in project files, causing issues when running the application on different systems.
> Source: @ghost1372 in [#5716](https://github.com/microsoft/WindowsAppSDK/issues/5716)

**Fix:**
1. Open your `.csproj` file and replace hard-coded paths with relative paths. For example:
   ```xml
   <ItemGroup>
     <None Include="Resources\Alternate.ani" />
   </ItemGroup>
   ```
2. Update any `.rc` files to use relative paths instead of absolute paths.

**Verify:** Rebuild the project and confirm the custom cursor is displayed correctly.

---

### Scenario 3: Missing AOT Compatibility Settings

**Cause:** The project is not explicitly marked as AOT-compatible, leading to warnings or runtime issues.
> Source: @ghost1372 in [#5716](https://github.com/microsoft/WindowsAppSDK/issues/5716)

**Fix:**
1. Add the following property to your `.csproj` file:
   ```xml
   <PropertyGroup>
     <IsAotCompatible>true</IsAotCompatible>
   </PropertyGroup>
   ```
2. Rebuild the project to ensure compatibility.

**Verify:** Check for any remaining warnings during the build process and confirm the application runs without issues.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- Disable runtime marshaling in your project settings.  
  > Suggested by @ghost1372 in [#5716](https://github.com/microsoft/WindowsAppSDK/issues/5716)

- Use `LibraryImport` instead of `DllImport` for native code interop.  
  > Suggested by @ghost1372 in [#5716](https://github.com/microsoft/WindowsAppSDK/issues/5716)

- Avoid using reflection in your codebase.  
  > Suggested by @ghost1372 in [#5716](https://github.com/microsoft/WindowsAppSDK/issues/5716)

---

## References

- [Official docs](https://learn.microsoft.com/en-us/windows/apps/)
- [API docs](https://learn.microsoft.com/en-us/dotnet/api/)

---

**Updated:** 2026-03-17 | **Confidence:** 0.9  
**Sources:** [#5716](https://github.com/microsoft/WindowsAppSDK/issues/5716)