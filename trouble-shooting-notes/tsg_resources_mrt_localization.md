# Error: "COMException: NamedResource Not Found" / PrimaryLanguageOverride Not Working — MRT Resource Loading

**Keywords:** ResourceLoader, GetString, COMException, NamedResource Not Found, PrimaryLanguageOverride, unpackaged, localization, x:Uid, MRTCore, Resources.resw, dot in key, signing projection, resources.pri, MrmGetFilePathFromName, ResourceManager, ResourceLoaderTest, MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY, PublishSingleFile, MSIX, AppxBundle, ManifestLanguages

**Error Example:**
```
COMException: NamedResource Not Found.
```
```
COMException: No such interface supported
```
```
System.IO.FileNotFoundException: Unable to find the specified file.
```

---

## Quick Match

**You're seeing this if:**
- `ResourceLoader.GetString()` throws `COMException` for keys containing `.` (dots)
- `PrimaryLanguageOverride` does not work in unpackaged WinUI 3 apps
- `x:Uid` XAML localization fails for unpackaged apps
- Signing `Microsoft.Windows.ApplicationModel.Resources.Projection` fails during build
- `MrmGetFilePathFromName` reports `ERROR_FILE_NOT_FOUND` when `resources.pri` is missing
- `ResourceLoader` crashes with `System.IO.FileNotFoundException` in unpackaged apps
- Launching one WinAppSDK app from another fails due to `MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY`
- Localization fails after packaging into MSIX bundles in WASDK 1.8

→ Check scenarios below for your specific cause

---

## Related Issues

- [#6247](https://github.com/microsoft/WindowsAppSDK/issues/6247) — ResourceLoader.GetString throws COMException for keys containing '.' (Status: Open)
- [#1687](https://github.com/microsoft/WindowsAppSDK/issues/1687) — Support PrimaryLanguageOverride from unpackaged apps (Status: Closed/Fixed in 1.6-experimental2)
- [#3705](https://github.com/microsoft/WindowsAppSDK/issues/3705) — Signing Resources.Projection fails with ilasm errors (Status: Closed)
- [#5814](https://github.com/microsoft/WindowsAppSDK/issues/5814) — MrmGetFilePathFromName now reports error when `resources.pri` is missing (Status: Closed)
- [#5832](https://github.com/microsoft/WindowsAppSDK/issues/5832) — Upgrading to v1.8 breaks ResourceLoader in unpackaged apps (Status: Open)
- [#5987](https://github.com/microsoft/WindowsAppSDK/issues/5987) — MRTCore using MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY breaks launching one WinAppSDK app from another (Status: Open)
- [#5817](https://github.com/microsoft/WindowsAppSDK/issues/5817) — Localization fails after packaging into MSIX bundles in WASDK 1.8 (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: ResourceLoader.GetString Fails for Keys Containing Dots

**Cause:** Resource keys that contain `.` (period/dot) characters are not resolved correctly by `ResourceLoader.GetString()`. The MRT/PRI key normalization or URI fragment semantics treat `.` as a path separator, causing the lookup to fail with `NamedResource Not Found`.
> Source: Issue reporter in [#6247](https://github.com/microsoft/WindowsAppSDK/issues/6247)

**Example:**
```csharp
// FAILS — throws COMException: NamedResource Not Found
var loader = new ResourceLoader();
var value = loader.GetString("XXXXX.Common.Yes");

// WORKS — returns expected value
var value = loader.GetString("XXXXX_Common_Yes");
```

**Fix:**
Replace `.` with `_` in resource key names in your `Resources.resw` file and update all code references accordingly.

| Before | After |
|--------|-------|
| `XXXXX.Common.Yes` | `XXXXX_Common_Yes` |
| `App.Settings.Title` | `App_Settings_Title` |

> ⚠️ No official confirmation on whether `.` is a supported character in resource keys for `ResourceLoader.GetString`. The issue is open and under investigation.

**Verify:**
```csharp
var loader = new ResourceLoader();
var value = loader.GetString("XXXXX_Common_Yes");
Debug.Assert(value == "Yes");
```

---

### Scenario 2: PrimaryLanguageOverride Not Working in Unpackaged Apps

**Cause:** `Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride` was designed for packaged apps only. Calling it from an unpackaged context threw an error, breaking `x:Uid` XAML localization for unpackaged WinUI 3 apps.
> Source: @btueffers (CONTRIBUTOR) and @andrewleader (CONTRIBUTOR) in [#1687](https://github.com/microsoft/WindowsAppSDK/issues/1687)

**Fix (WASDK 1.6+ experimental and later):**
Use the new WinAppSDK-specific API instead of the Windows.Globalization API:
```csharp
// Use this (WinAppSDK API — works for both packaged and unpackaged):
Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";

// Instead of this (Windows platform API — packaged only):
Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
```

> ✅ Confirmed working by: @ghost1372 (CONTRIBUTOR) in [#1687](https://github.com/microsoft/WindowsAppSDK/issues/1687) — tested in 1.6.240701003-experimental2

**Pre-fix workaround (for older WASDK versions):**
Use `ResourceContext` directly and set the `Language` qualifier value:
```csharp
var context = new ResourceContext();
context.QualifierValues["Language"] = "en-US";
// Pass this context to resource lookups
```
> Source: @huichen123 (CONTRIBUTOR) in [#1687](https://github.com/microsoft/WindowsAppSDK/issues/1687)

**Community alternative:**
The [WinUI3Localizer](https://www.nuget.org/packages/WinUI3Localizer/) NuGet package works with unpackaged apps and uses standard `Resources.resw` strings.
> Source: @AndrewKeepCoding in [#1687](https://github.com/microsoft/WindowsAppSDK/issues/1687)

---

### Scenario 3: COMException During First Use of PrimaryLanguageOverride (Unpackaged)

**Cause:** Even after upgrading to a WASDK version with the fix, some users encountered `COMException: No such interface supported` when first using `PrimaryLanguageOverride` in unpackaged apps.
> Source: @TheVoidSeeker in [#1687](https://github.com/microsoft/WindowsAppSDK/issues/1687)

**Fix:**
1. Repair your Visual Studio installation.
2. Clean all intermediate and output files (`bin/`, `obj/`).
3. Rebuild the project.

> ✅ Confirmed by: @TheVoidSeeker in [#1687](https://github.com/microsoft/WindowsAppSDK/issues/1687) — resolved after VS repair + clean rebuild

---

### Scenario 4: MrmGetFilePathFromName Reports ERROR_FILE_NOT_FOUND

**Cause:** Starting with WASDK 1.8, `MrmGetFilePathFromName` now checks for the existence of `resources.pri` and returns `ERROR_FILE_NOT_FOUND` if the file is missing. This can break `ResourceManager` in unpackaged apps.
> Source: @Alovchin91 in [#5814](https://github.com/microsoft/WindowsAppSDK/issues/5814)

**Fix:**
Subscribe to the `Application.ResourceManagerRequested` event and provide a custom `ResourceManager` instance pointing to the correct `.pri` file:
```csharp
Application.ResourceManagerRequested += (sender, args) =>
{
    args.ResourceManager = new ResourceManager("<path_to_correct_pri_file>");
};
```
> Source: @Alovchin91 in [#5814](https://github.com/microsoft/WindowsAppSDK/issues/5814)

---

### Scenario 5: ResourceLoader Crashes with FileNotFoundException in Unpackaged Apps (WASDK 1.8)

**Cause:** In WASDK 1.8, the default `.pri` file name was changed from `resources.pri` to `<executable_name>.pri`. If the application expects `resources.pri`, it will fail to find the file and throw a `FileNotFoundException`.
> Source: @beeradmoore in [#5832](https://github.com/microsoft/WindowsAppSDK/issues/5832)

**Fix:**
Manually rename `<executable_name>.pri` to `resources.pri` after every build:
```xml
<Target Name="RenamePriFile" AfterTargets="Build">
  <Move SourceFiles="$(TargetDir)$(AssemblyName).pri" DestinationFiles="$(TargetDir)resources.pri" />
</Target>
```
> Source: @gautambjain in [#5832](https://github.com/microsoft/WindowsAppSDK/issues/5832)

**Alternative:** Use Syncfusion's latest WinUI3 suite (31.2.2) with WASDK 1.8.251003001, which resolves this issue for some users.
> Source: @Steffens-Bridgemate in [#5832](https://github.com/microsoft/WindowsAppSDK/issues/5832)

---

## Known Issues

### Issue: MRTCore using MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY breaks launching one WinAppSDK app from another

**Cause:** When a WinAppSDK application sets the `MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY` environment variable, MRTCore skips all local resource PRI locations (e.g., `ModuleName.pri`) and only loads `resources.pri` from the specified directory. This causes issues when one WinAppSDK app launches another using `CreateProcess`, as the environment variable is inherited.
> Source: @DHowett [MSFT] in [#5987](https://github.com/microsoft/WindowsAppSDK/issues/5987)

**Status:** Open

**Workaround:** None confirmed. Avoid using the `MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY` environment variable if possible.

---

### Issue: Localization Fails After Packaging into MSIX Bundles in WASDK 1.8

**Cause:** When packaging applications into MSIX bundles using WASDK 1.8, only one language is included in the bundle, and the `ManifestLanguages` list is reduced to a single language. This causes localization to fail after installation.
> Source: @BigHeadDev, @JulienTheron, @torum in [#5817](https://github.com/microsoft/WindowsAppSDK/issues/5817)

**Status:** Open

**Workaround:**
- Set `AppxBundle` to `Never` in your project settings to avoid creating a bundle.
  > Source: @JulienTheron in [#5817](https://github.com/microsoft/WindowsAppSDK/issues/5817)
- Manually create the bundle using `MakeAppx` to ensure all languages are included.
  > Source: @JulienTheron in [#5817](https://github.com/microsoft/WindowsAppSDK/issues/5817)

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- XAML Islands (non-WinUI 3) apps using `Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride` still do not work without packaging; WASDK fix only covers MRTCore-based scenarios (from @sylveon in #1687)
- On WASDK 1.8, `PrimaryLanguageOverride` value may not persist across app relaunch — see Known Issue #6118 (from @Galebra in #1687)

---

## References

- [ResourceLoader.GetString API docs](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.windows.applicationmodel.resources.resourceloader.getstring)
- [MRTCore ResourceContext source](https://github.com/microsoft/WindowsAppSDK/blob/main/dev/MRTCore/mrt/Microsoft.Windows.ApplicationModel.Resources/src/ResourceContext.cpp)
- [WinUI3Localizer NuGet](https://www.nuget.org/packages/WinUI3Localizer/)

---

**Updated:** 2026-04-06 | **Confidence:** 0.8
**Sources:** #6247, #1687, #3705, #5814, #5832, #5987, #5817