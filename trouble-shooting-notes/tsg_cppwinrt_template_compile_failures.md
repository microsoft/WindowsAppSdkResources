# Error: "MIDL2011: unresolved type declaration" - C++/WinRT Template Compilation Failures

**Keywords:** MIDL2011, unresolved type declaration, Microsoft.UI.Xaml.Window, C++/WinRT, Windows App SDK, WinUI3

**Error Example:**
```
MIDL2011: [msg]unresolved type declaration [context]: Microsoft.UI.Xaml.Window [ RuntimeClass 'OlMaRudge.MainWindow' ]
```

---

## Quick Match

**You're seeing this if:**
- Error contains "MIDL2011" or "unresolved type declaration"
- Building a project created from the "Blank App, Packaged (WinUI3 in Desktop)" C++ template
- Platform: Visual Studio 2022 with Windows App SDK 1.7.x

→ Check scenarios below for your specific cause

---

## Related Issues

- [#5041](https://github.com/microsoft/WindowsAppSDK/issues/5041) - Blank App, Packaged (WinUI3 in Desktop) C++ fails to compile (Status: Closed, Fixed)
- [#5507](https://github.com/microsoft/WindowsAppSDK/issues/5507) - `base.h` `#elif _DEBUG` gives build error C1017: invalid integer constant expression (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: Missing CppWinRT NuGet Package

**Cause:** The Visual Studio 2022 "Blank App, Packaged (WinUI3 in Desktop)" C++ template does not include the required `Microsoft.Windows.CppWinRT` NuGet package, which is necessary for generating the projection and providing metadata for MIDL compilation.
> Source: @HUN73R [MSFT] in [#5041](https://github.com/microsoft/WindowsAppSDK/issues/5041)

**Fix:**
1. Open your project in Visual Studio 2022.
2. Install the `Microsoft.Windows.CppWinRT` NuGet package:
   - Right-click on your project in the Solution Explorer.
   - Select **Manage NuGet Packages**.
   - Search for `Microsoft.Windows.CppWinRT` and install the latest version.
3. Rebuild your project.

> ✅ Confirmed by: @HUN73R [MSFT] in [#5041](https://github.com/microsoft/WindowsAppSDK/issues/5041)

**Verify:** Rebuild the project. The error `MIDL2011: unresolved type declaration` should no longer appear.

---

### Scenario 2: Incorrect or Missing Windows SDK Version

**Cause:** The required Windows SDK version (e.g., 10.0.26100.0) is not installed or not properly configured in the Visual Studio environment.
> Source: @DarranRowe in [#5041](https://github.com/microsoft/WindowsAppSDK/issues/5041)

**Fix:**
1. Open the Visual Studio Installer.
2. Ensure the latest Windows 11 SDK (e.g., 10.0.26100.0) is installed:
   - Go to the **Individual Components** tab.
   - Check for the **Windows 11 SDK** and install it if missing.
3. Update your project settings to use the correct Windows SDK version:
   - Right-click on your project in Solution Explorer.
   - Select **Properties** > **General**.
   - Set the **Windows SDK Version** to the installed version (e.g., 10.0.26100.0).
4. Rebuild your project.

---

### Scenario 3: Incorrect Preprocessor Definitions

**Cause:** The `_DEBUG` macro is defined in a way that does not resolve to a valid integer constant expression, causing a conflict during compilation.
> Source: @DarranRowe in [#5507](https://github.com/microsoft/WindowsAppSDK/issues/5507)

**Fix:**
1. Open your project in Visual Studio 2022.
2. Check the **Preprocessor Definitions** in your project settings:
   - Right-click on your project in Solution Explorer.
   - Select **Properties** > **C/C++** > **Preprocessor**.
   - Ensure `_DEBUG` is defined correctly (e.g., `_DEBUG=1`).
3. If using third-party libraries (e.g., Boost), verify that they do not redefine `_DEBUG` in a conflicting manner.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- Ensure Visual Studio and all extensions are up to date.  
  > Suggested by: @DarranRowe in [#5041](https://github.com/microsoft/WindowsAppSDK/issues/5041)

- Downgrade the Windows App SDK NuGet package to an earlier version (e.g., 1.6.x) if using the latest version does not resolve the issue.  
  > Suggested by: @HUN73R [MSFT] in [#5041](https://github.com/microsoft/WindowsAppSDK/issues/5041)

---

## References

- [CppWinRT Repository](https://github.com/microsoft/cppwinrt)
- [Microsoft.Windows.CppWinRT NuGet Package](https://www.nuget.org/packages/Microsoft.Windows.CppWinRT)
- [Windows App SDK Documentation](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/)

---

**Updated:** 2026-03-17 | **Confidence:** 0.9  
**Sources:** [#5041](https://github.com/microsoft/WindowsAppSDK/issues/5041), [#5507](https://github.com/microsoft/WindowsAppSDK/issues/5507)