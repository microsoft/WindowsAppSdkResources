# WinRT Interop, AOT Compatibility & Runtime Version Issues - Windows App SDK

**Keywords:** AOT, trimming, FrameworkElement, theme, LaunchActivatedEventArgs, WinRT, CsWinRT, IInspectable, ReleaseInfo, version string, HDR, CompositionDrawingSurface, scRGB, swap chain, DirectXPixelFormat, ApplicationDataContainer, MathML, C++WinRT, Dispose, GetVirtualMethodTableInfoForKey, AppInstance, FindOrRegisterForKey, ArgumentException, COMException

**Error Example:**
```
// AOT theme switching failure
(Window.Content as FrameworkElement) returns null when published with AOT

// LaunchActivatedEventArgs interop
AppActivationArguments.Data shows as WinRT.IInspectable instead of LaunchActivatedEventArgs

// ReleaseInfo version mismatch
ReleaseInfo.AsString returns "1.7.0" on WinAppSDK 1.7.1 runtime

// HDR composition
CompositionDrawingSurface with R16G16B16A16Float renders as SDR in WinAppSDK

// ApplicationDataContainer Dispose crash
Calling Dispose() on ApplicationDataContainer throws "Specified cast is not valid" on Windows 10

// MathML crash in RichEditBox
Setting MathML content in RichEditBox causes app crash

// C++WinRT interop error
CS0400: The type or namespace name could not be found in the global namespace

// AppInstance.FindOrRegisterForKey exception
FindOrRegisterForKey("main") throws ArgumentException or COMException
```

---

## Quick Match

**You're seeing this if:**
- Theme switching code fails silently when app is published with Native AOT
- `Window.Content as FrameworkElement` returns `null` in AOT builds
- `AppActivationArguments.Data` is `WinRT.IInspectable` instead of a typed activation args object
- `ReleaseInfo.AsString` returns wrong version (e.g., "1.7.0" on 1.7.1)
- HDR content renders as SDR when using `CompositionDrawingSurface` with float16 pixel format
- `ApplicationDataContainer.Dispose()` throws "Specified cast is not valid" on Windows 10
- Setting MathML content in `RichEditBox` crashes the app
- C++WinRT component in C# project causes CS0400 error
- `AppInstance.FindOrRegisterForKey` throws `ArgumentException` or `COMException`

→ Check scenarios below for your specific cause

---

## Related Issues

- [#5389](https://github.com/microsoft/WindowsAppSDK/issues/5389) - Unable to change theme color when AOT is released (Status: Closed, area-AOT)
- [#6219](https://github.com/microsoft/WindowsAppSDK/issues/6219) - WASDK LaunchActivatedEventArgs cannot be directly converted into WinRT (Status: Open, area-Projections)
- [#5323](https://github.com/microsoft/WindowsAppSDK/issues/5323) - ReleaseInfo returns "1.7.0" in 1.7.1 release (Status: Closed, area-VersionInfo)
- [#6291](https://github.com/microsoft/WindowsAppSDK/issues/6291) - Unlike UWP, WindowsAppSdk does not support HDR composition (Status: Closed)
- [#5633](https://github.com/microsoft/WindowsAppSDK/issues/5633) - ApplicationDataContainer throws "Specified cast is not valid" exception on Dispose() (Status: Closed, area-dotnet)
- [#5257](https://github.com/microsoft/WindowsAppSDK/issues/5257) - App Crashes When Setting `MathML` in `RichEditBox` (Status: Closed, area-WinUI)
- [#5071](https://github.com/microsoft/WindowsAppSDK/issues/5071) - Using a C++WinRT component in C# project error CS0400 (Status: Open, area-Projections)
- [#5694](https://github.com/microsoft/WindowsAppSDK/issues/5694) - AppInstance.FindOrRegisterForKey throws ArgumentException: The parameter is incorrect (Status: Open, area-Lifecycle)

---

## Scenarios & Solutions

### Scenario 1: Theme Switching Fails Under Native AOT (Trimming)

**Cause:** When publishing a WinUI app with Native AOT, the .NET trimmer removes `FrameworkElement` because the C# code doesn't directly construct it. As a result, `Window.Content as FrameworkElement` returns `null`, and theme switching code silently fails. The `as` operator cannot succeed because the type metadata has been trimmed.
> Source: @manodasanW (MEMBER) in [#5389](https://github.com/microsoft/WindowsAppSDK/issues/5389)

**Fix:** [See full details in the original TSG]

---

### Scenario 2: WASDK LaunchActivatedEventArgs Not Convertible to WinRT Type

**Cause:** [See full details in the original TSG]

---

### Scenario 3: ReleaseInfo.AsString Returns Wrong Patch Version

**Cause:** The `ReleaseInfo.AsString` API in Windows App SDK 1.7.1 incorrectly returns "1.7.0" instead of the expected "1.7.1". This is due to a known issue where the `Patch` property always shows as "0" in version 1.7.x.
> Source: @RDMacLachlan (MSFT) in [#5323](https://github.com/microsoft/WindowsAppSDK/issues/5323)

**Workaround:** None. This issue is resolved in Windows App SDK 1.8.4 and later, where the `ReleaseInfo` API returns a more accurate version string.

**Environment:**
- Windows App SDK 1.7.1
- Windows 11 version 24H2 (22621, October 2024 Update)

---

### Scenario 4: HDR Composition Not Supported in WinAppSDK (UWP Parity Gap)

**Cause:** [See full details in the original TSG]

---

### Scenario 5: ApplicationDataContainer.Dispose() Throws "Specified Cast Is Not Valid"

**Cause:** [See full details in the original TSG]

---

### Scenario 6: Setting MathML Content in RichEditBox Causes Crash

**Cause:** [See full details in the original TSG]

---

### Scenario 7: C++WinRT Component in C# Project Causes CS0400 Error

**Cause:** [See full details in the original TSG]

---

### Scenario 8: AppInstance.FindOrRegisterForKey Throws ArgumentException or COMException

**Cause:** The `AppInstance.FindOrRegisterForKey` method can throw exceptions such as `ArgumentException`, `COMException`, or `UnauthorizedAccessException` under certain conditions. These issues may be related to incorrect parameters, COM activation issues, or permission problems.
> Source: @tipa in [#5694](https://github.com/microsoft/WindowsAppSDK/issues/5694)

**Workaround:** None confirmed. Developers encountering this issue are advised to log the exception details and ensure that the key passed to `FindOrRegisterForKey` is valid and unique. Further investigation is ongoing.

**Environment:**
- Windows App SDK 1.7.3
- Windows 11 version 24H2 LTSC (26100, June Update)

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- For HDR (#6291): Using a custom DirectComposition swap chain with `DXGI_FORMAT_R16G16B16A16_FLOAT` created outside of WinUI's compositor may enable HDR rendering, but this bypasses the WinUI composition tree (not confirmed).
- For ApplicationDataContainer (#5633): Using `<maxversiontested>` in the app manifest may prevent crashes on unsupported Windows versions.
- For `AppInstance.FindOrRegisterForKey` (#5694): Ensure that the key is unique and valid. Some users have reported issues with specific Windows versions or configurations.

---

## References

- [CsWinRT 2.3 Preview - NuGet](https://www.nuget.org/packages/Microsoft.Windows.CsWinRT/2.3.0-prerelease.251115.2)
- [MathML Specification](https://www.w3.org/TR/MathML3/)
- [WinAppSDK ReleaseInfo API](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.windows.applicationmodel.windowsappruntime.releaseinfo)
- [HDR and Advanced Color in Windows](https://learn.microsoft.com/en-us/windows/win32/direct3darticles/high-dynamic-range)

---

**Updated:** 2026-03-17 | **Confidence:** 0.8
**Sources:** #5389, #6219, #5323, #6291, #5633, #5257, #5071, #5694