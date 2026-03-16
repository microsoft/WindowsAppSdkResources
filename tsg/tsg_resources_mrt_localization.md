# Error: "COMException: NamedResource Not Found" / PrimaryLanguageOverride Not Working — MRT Resource Loading

**Keywords:** ResourceLoader, GetString, COMException, NamedResource Not Found, PrimaryLanguageOverride, unpackaged, localization, x:Uid, MRTCore, Resources.resw, dot in key, signing projection

**Error Example:**
```
COMException: NamedResource Not Found.
```
```
COMException: No such interface supported
```

---

## Quick Match

**You're seeing this if:**
- `ResourceLoader.GetString()` throws `COMException` for keys containing `.` (dots)
- `PrimaryLanguageOverride` does not work in unpackaged WinUI 3 apps
- `x:Uid` XAML localization fails for unpackaged apps
- Signing `Microsoft.Windows.ApplicationModel.Resources.Projection` fails during build

→ Check scenarios below for your specific cause

---

## Related Issues

- [#6247](https://github.com/microsoft/WindowsAppSDK/issues/6247) — ResourceLoader.GetString throws COMException for keys containing '.' (Status: Open)
- [#1687](https://github.com/microsoft/WindowsAppSDK/issues/1687) — Support PrimaryLanguageOverride from unpackaged apps (Status: Closed/Fixed in 1.6-experimental2)
- [#3705](https://github.com/microsoft/WindowsAppSDK/issues/3705) — Signing Resources.Projection fails with ilasm errors (Status: Closed)

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

### Scenario 4: Signing Microsoft.Windows.ApplicationModel.Resources.Projection Fails

**Cause:** When using code analyzers configured for assembly signing, attempting to sign the `Microsoft.Windows.ApplicationModel.Resources.Projection` assembly via `ildasm`/`ilasm` round-trip fails. The assembly contains non-standard IL patterns (non-virtual instance methods in interfaces) that `ilasm` rejects.
> Source: Issue reporter in [#3705](https://github.com/microsoft/WindowsAppSDK/issues/3705)

**Error:**
```
error : Method cannot have body if it is non-static declared in interface abstract
error : Non-public instance method in interface
```

**Fix:** WASDK 1.3 has reached End of Support. Upgrade to WASDK 1.8 or later where this may be resolved. If the issue persists on 1.8+, file a new issue.
> Source: @alexlamtest (CONTRIBUTOR) in [#3705](https://github.com/microsoft/WindowsAppSDK/issues/3705)

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

**Updated:** 2026-03-13 | **Confidence:** 0.8
**Sources:** #6247, #1687, #3705
