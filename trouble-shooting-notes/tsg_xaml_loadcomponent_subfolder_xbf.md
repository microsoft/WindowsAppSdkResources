# Error: LoadComponent Fails for XAML Pages in Subfolders — XBF Filename-Only Indexing

**Keywords:** LoadComponent, XBF, XAML, subfolders, ms-appx, embed.resfiles, GenerateLibraryLayout, _CalculateXbfSupport, _SupportXbfAsEmbedFileResources, resources.pri, Microsoft.Build.Msix.Pri.targets, WinUI 3, subdirectory

**Error Example:**
```
Cannot locate resource from 'ms-appx:///Presentation/Shell.xaml'
```
or:
```
Layout cycle detected. Layout is not able to complete.
```

---

## Quick Match

**You're seeing this if:**
- App builds without errors but crashes at runtime with `Cannot locate resource from 'ms-appx:///.../Page.xaml'`
- XAML pages are organized in subdirectories (e.g., `Presentation\Shell.xaml`)
- Building from Visual Studio with `Platform=x64` (not `dotnet build -r win-x64`)
- Using WinUI 3 with the WindowsAppSDK NuGet package (1.0.0+)

→ See Scenario 1 below

---

## Related Issues

- [#6299](https://github.com/microsoft/WindowsAppSDK/issues/6299) — LoadComponent fails for XAML pages in subfolders — embedded XBF indexed by filename only (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: XBF Files Lose Subdirectory Path When Embedded as File Resources

**Cause:** The `_CalculateXbfSupport` target in `Microsoft.Build.Msix.Pri.targets` (shipped in the `Microsoft.WindowsAppSDK` NuGet package since v1.0.0) sets `_SupportXbfAsEmbedFileResources=true` by default. When building from Visual Studio with `Platform=x64`, the intermediate path structure differs from `dotnet build -r win-x64`, causing XBF files to be indexed by filename only (without subdirectory path) in `embed.resfiles`. At runtime, `LoadComponent` looks for `ms-appx:///Presentation/Shell.xaml` but the resource is registered as just `Shell.xbf`.

> Source: @xperiandri in [#6299](https://github.com/microsoft/WindowsAppSDK/issues/6299)

**Fix — Set `GenerateLibraryLayout=true`:**

Add the following property to your project file:
```xml
<PropertyGroup>
  <GenerateLibraryLayout>true</GenerateLibraryLayout>
</PropertyGroup>
```

This causes the `CustomOutputGroupForPackaging` target in `Microsoft.UI.Xaml.Markup.Compiler.interop.targets` to use the correct `XamlPackagingRootFolder`, preserving subdirectory paths in the XBF resource index.

> Source: @xperiandri in [#6299](https://github.com/microsoft/WindowsAppSDK/issues/6299) — "Better fix found: `GenerateLibraryLayout=true`"

**Verify:** Clean the solution, rebuild from Visual Studio, and confirm the app loads XAML pages from subfolders without runtime crash.

---

## References

- [Microsoft.Build.Msix.Pri.targets source](https://github.com/microsoft/WindowsAppSDK) — `_CalculateXbfSupport` target
- [XBF (XAML Binary Format) overview](https://learn.microsoft.com/en-us/windows/apps/winui/)

---

**Updated:** 2026-03-20 | **Confidence:** 0.85
**Sources:** [#6299](https://github.com/microsoft/WindowsAppSDK/issues/6299)
