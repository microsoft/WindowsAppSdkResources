# Error: Self-Contained Deployment and MSIX Packaging Conflicts

**Keywords:** WindowsAppSDKSelfContained, XamlParseException, COMException, 0x80070490, resources.pri, PRI, _OverrideGetPriIndexName, EnableMsixTooling, WMC1012, ApplicationXaml, codegen, library project

**Error Examples:**
```
Microsoft.UI.Xaml.Markup.XamlParseException: 'XAML parsing failed.'
System.Runtime.InteropServices.COMException (0x80070490): Element not found.
NETSDK1022: Duplicate 'PRIResource' items were included.
WMC1012: A project cannot have more than one ApplicationXaml item
```

---

## Quick Match

**You're seeing this if:**
- Setting `WindowsAppSDKSelfContained=true` on a library project causes XAML parsing or codegen failures
- Upgrading to WinAppSDK 1.8 causes COMException 0x80070490 (Element not found) related to `.pri` file naming changes
- Build errors include `NETSDK1022` (duplicate items) or `WMC1012` (multiple ApplicationXaml) after upgrading

→ Check scenarios below for your specific cause

---

## Related Issues

- [#6091](https://github.com/microsoft/WindowsAppSDK/issues/6091) - Self-contained deployment breaks WinUI codegen in libraries (Status: Closed/Fixed in v2.0-preview1)
- [#5746](https://github.com/microsoft/WindowsAppSDK/issues/5746) - App update to 1.8-preview1 with COMException 0x80070490 errors (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: WindowsAppSDKSelfContained=true on Library Project Breaks XAML Codegen

**Cause:** When `WindowsAppSDKSelfContained` is set to `true` in a class library project, the `_OverrideGetPriIndexName` target in `Microsoft.WindowsAppSDK.SelfContained.targets` (part of `Microsoft.WindowsAppSDK.Base` NuGet) sets the PRI root to empty. This is intended for app projects, not libraries, and causes WinUI XAML code generation to fail. The result is a `XamlParseException` at runtime or a silent launch failure.
> Source: @dongle-the-gadget in [#6091](https://github.com/microsoft/WindowsAppSDK/issues/6091)

**Affected versions:** WinAppSDK 1.8.x (confirmed with 1.8.3 / 1.8.251106002)

**Fix:**
1. **Remove** `WindowsAppSDKSelfContained` from all library/class library project files. Only set it on the main application (executable) project:
   ```xml
   <!-- ❌ Do NOT set in library .csproj -->
   <!-- <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained> -->

   <!-- ✅ Only set in app/exe .csproj -->
   <PropertyGroup>
     <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
   </PropertyGroup>
   ```
2. If setting the property globally via command line (`dotnet msbuild /p:WindowsAppSDKSelfContained=true`), this will apply to all projects in the solution including libraries — avoid this pattern.

> ✅ Confirmed by: @riverar (CONTRIBUTOR) in [#6091](https://github.com/microsoft/WindowsAppSDK/issues/6091) — removing the property from the library project resolves the issue.

> @DrusTheAxe (Microsoft) in [#6091](https://github.com/microsoft/WindowsAppSDK/issues/6091) confirmed: "Library projects don't own context. WinAppSDK downlevel regfree WinRT support only applies to the Fusion manifest for the process' EXE."

**Official fix:** This has been fixed in [Windows App SDK 2.0.0-Preview1](https://github.com/microsoft/WindowsAppSDK/releases/tag/v2.0-preview1) by adding explicit validation that prevents `WindowsAppSDKSelfContained` from being applied to class library projects.
> Source: @ssparach (CONTRIBUTOR) in [#6091](https://github.com/microsoft/WindowsAppSDK/issues/6091)

**Verify:** Clean the solution, rebuild, and run the app — XAML should parse without exceptions.

---

### Scenario 2: COMException 0x80070490 "Element not found" After Upgrading to 1.8

**Cause:** In WinAppSDK 1.8, the `.pri` file naming behavior changed for unpackaged projects. Previously (1.7 and earlier), `EnableMsixTooling=true` caused the `.pri` file to be named `resources.pri` even for unpackaged apps. In 1.8, unpackaged projects correctly use `[AppName].pri` instead. Code that hard-codes `resources.pri` will fail with COMException 0x80070490 ("Element not found") when accessing localized resources.
> Source: @Sella-GH in [#5746](https://github.com/microsoft/WindowsAppSDK/issues/5746), root cause analysis by @DarranRowe

**Additional build errors on upgrade:** Upgrading to 1.8-preview1 may also cause:
- `NETSDK1022: Duplicate 'PRIResource' items were included` — Fix with `<EnableDefaultPriItems>false</EnableDefaultPriItems>`
- `NETSDK1022: Duplicate 'Page' items were included` — Fix with `<EnableDefaultPageItems>false</EnableDefaultPageItems>`
- `WMC1012: A project cannot have more than one ApplicationXaml item` — May require project restructuring

**Fix for COMException 0x80070490:**
1. Update any code that hard-codes `resources.pri` to use the correct file name. Use the Resource Manager API to determine the default path:
   ```csharp
   // Instead of hard-coding "resources.pri", use:
   var resourceLoader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
   // The loader will automatically find the correct .pri file
   ```
2. If you have a custom resource loading service, update the file name from `resources.pri` to `[YourAppName].pri`:
   ```csharp
   // Before (1.7.x):
   var priPath = "resources.pri";
   // After (1.8.x, unpackaged):
   var priPath = $"{Assembly.GetExecutingAssembly().GetName().Name}.pri";
   ```

> ✅ Confirmed by: @Sella-GH in [#5746](https://github.com/microsoft/WindowsAppSDK/issues/5746) — "When I changed the string to match the new name it works completely fine."

**Fix for duplicate item build errors:**
```xml
<PropertyGroup>
  <EnableDefaultPriItems>false</EnableDefaultPriItems>
  <EnableDefaultPageItems>false</EnableDefaultPageItems>
</PropertyGroup>
```

> @DarranRowe provided detailed analysis in [#5746](https://github.com/microsoft/WindowsAppSDK/issues/5746) confirming that the PRI naming in older versions was a bug (unpackaged projects shouldn't have used `resources.pri`), and 1.8 corrects this behavior.

**Verify:**
```powershell
# Check which .pri files exist in your output directory
Get-ChildItem -Path "<YourOutputDir>" -Filter "*.pri"
# Should show [AppName].pri, Microsoft.WindowsAppRuntime.pri, Microsoft.UI.pri, etc.
```

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- If encountering `WMC1012` (multiple ApplicationXaml) after upgrade, try cleaning all `obj` and `bin` directories before rebuilding (general cleanup suggestion related to #5746)
- Avoid setting `WindowsAppSDKSelfContained=true` via `Directory.Build.props` if your solution contains library projects — use per-project settings instead (from community analysis in #6091)

---

## References

- [Self-contained deployment documentation](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/deploy-self-contained)
- [WinAppSDK 2.0.0-Preview1 release notes](https://github.com/microsoft/WindowsAppSDK/releases/tag/v2.0-preview1)
- [Resource management with MRT Core](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/mrtcore/mrtcore-overview)

---

**Updated:** 2025-07-18 | **Confidence:** 0.85
**Sources:** [#6091](https://github.com/microsoft/WindowsAppSDK/issues/6091), [#5746](https://github.com/microsoft/WindowsAppSDK/issues/5746)
