# WinRT Interop, AOT Compatibility & Runtime Version Issues - Windows App SDK

**Keywords:** AOT, trimming, FrameworkElement, theme, LaunchActivatedEventArgs, WinRT, CsWinRT, IInspectable, ReleaseInfo, version string, HDR, CompositionDrawingSurface, scRGB, swap chain, DirectXPixelFormat

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
```

---

## Quick Match

**You're seeing this if:**
- Theme switching code fails silently when app is published with Native AOT
- `Window.Content as FrameworkElement` returns `null` in AOT builds
- `AppActivationArguments.Data` is `WinRT.IInspectable` instead of a typed activation args object
- `ReleaseInfo.AsString` returns wrong version (e.g., "1.7.0" on 1.7.1)
- HDR content renders as SDR when using `CompositionDrawingSurface` with float16 pixel format

→ Check scenarios below for your specific cause

---

## Related Issues

- [#5389](https://github.com/microsoft/WindowsAppSDK/issues/5389) - Unable to change theme color when AOT is released (Status: Closed, area-AOT)
- [#6219](https://github.com/microsoft/WindowsAppSDK/issues/6219) - WASDK LaunchActivatedEventArgs cannot be directly converted into WinRT (Status: Open, area-Projections)
- [#5323](https://github.com/microsoft/WindowsAppSDK/issues/5323) - ReleaseInfo returns "1.7.0" in 1.7.1 release (Status: Closed, area-VersionInfo)
- [#6291](https://github.com/microsoft/WindowsAppSDK/issues/6291) - Unlike UWP, WindowsAppSdk does not support HDR composition (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: Theme Switching Fails Under Native AOT (Trimming)

**Cause:** When publishing a WinUI app with Native AOT, the .NET trimmer removes `FrameworkElement` because the C# code doesn't directly construct it. As a result, `Window.Content as FrameworkElement` returns `null`, and theme switching code silently fails. The `as` operator cannot succeed because the type metadata has been trimmed.
> Source: @manodasanW (MEMBER) in [#5389](https://github.com/microsoft/WindowsAppSDK/issues/5389)

**Fix:**
1. Upgrade to [CsWinRT 2.3 previews](https://www.nuget.org/packages/Microsoft.Windows.CsWinRT/2.3.0-prerelease.251115.2) or later
2. Add the `[DynamicWindowsRuntimeCast]` attribute to the method performing the cast, passing `typeof(FrameworkElement)`:
```csharp
[DynamicWindowsRuntimeCast(typeof(FrameworkElement))]
private void SetTheme(ApplicationTheme theme)
{
    if (Window.Content is FrameworkElement rootElement)
    {
        rootElement.RequestedTheme = theme == ApplicationTheme.Dark
            ? ElementTheme.Dark
            : ElementTheme.Light;
    }
}
```
3. Enable AOT diagnostics to catch similar issues by setting `CsWinRTAotWarningLevel` to `3` in your project file:
```xml
<PropertyGroup>
    <CsWinRTAotWarningLevel>3</CsWinRTAotWarningLevel>
</PropertyGroup>
```
4. The diagnostics include a code fix that should help identify all such scenarios where types may be trimmed

> ✅ Confirmed by: @manodasanW (Microsoft, MEMBER) in #5389 comments

**Verify:** Publish with AOT and confirm `Window.Content as FrameworkElement` returns a non-null value.

**Environment:**
- WinAppSDK 1.7.1 (1.7.250401001)
- Packaged (MSIX) and Unpackaged
- Windows 11 24H2
- Visual Studio 2022-preview

---

### Scenario 2: WASDK LaunchActivatedEventArgs Not Convertible to WinRT Type

**Cause:** When `AppInstance.GetActivatedEventArgs()` is called in WASDK, it first tries `WinRT's AppInstance.GetActivatedEventArgs()`. If that returns null, WASDK falls back to its own custom implementation of `LaunchActivatedEventArgs`. The returned object is WASDK's custom type, not the WinRT `LaunchActivatedEventArgs`. On the .NET side, CsWinRT cannot recognize WASDK's custom implementation, so `AppActivationArguments.Data` appears as `WinRT.IInspectable` instead of the expected typed object.
> Source: Issue author in [#6219](https://github.com/microsoft/WindowsAppSDK/issues/6219)

**Workaround:**
1. Use `LaunchActivatedEventArgs.FromAbi()` to manually unwrap the inspectable pointer:
```csharp
var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
if (activatedArgs.Kind == ExtendedActivationKind.Launch)
{
    // Data is WinRT.IInspectable, need manual conversion
    var inspectable = activatedArgs.Data as IInspectable;
    if (inspectable != null)
    {
        var launchArgs = LaunchActivatedEventArgs.FromAbi(inspectable.ThisPtr);
        string arguments = launchArgs.Arguments;
    }
}
```
2. This is a known interop gap — WASDK team has been asked to add a wrapping layer so the returned object is recognized as `WinRT.LaunchActivatedEventArgs` by CsWinRT

**Status:** Open — no official fix yet. The request is for WASDK to wrap its custom implementation so .NET CsWinRT can recognize it correctly.

**Environment:**
- WinAppSDK 1.8.5 (1.8.260209005)
- Packaged (MSIX)
- Windows 11 24H2 LTSC (26100)

---

### Scenario 3: ReleaseInfo.AsString Returns Wrong Patch Version

**Cause:** `Microsoft.Windows.ApplicationModel.WindowsAppRuntime.ReleaseInfo.AsString` returns "1.7.0" even when running on the 1.7.1 runtime. The `Patch` property on `ReleaseInfo` also incorrectly returns `0`. Meanwhile, `RuntimeInfo.Version` correctly reports the full version (e.g., `7000.456.1632.0`).
> Source: Issue author in [#5323](https://github.com/microsoft/WindowsAppSDK/issues/5323)

**Workaround:**
1. Use `RuntimeInfo.Version` instead of `ReleaseInfo.AsString` for accurate version detection:
```csharp
// Unreliable in 1.7.x:
var releaseStr = ReleaseInfo.AsString; // Returns "1.7.0" even on 1.7.1

// Reliable alternative:
var runtimeVersion = RuntimeInfo.Version; // Returns correct version like 7000.456.1632.0
```
2. This was a known bug in the 1.7.x release train. Check whether the issue is resolved in your target SDK version

> ✅ Fix confirmed: Issue is closed, indicating the bug has been addressed in subsequent releases

**Verify:**
```csharp
Debug.WriteLine($"ReleaseInfo: {ReleaseInfo.AsString}");
Debug.WriteLine($"RuntimeInfo: {RuntimeInfo.Version}");
```

**Environment:**
- WinAppSDK 1.7.1 (1.7.250401001)
- Packaged (MSIX)
- Windows 11 24H2

---

### Scenario 4: HDR Composition Not Supported in WinAppSDK (UWP Parity Gap)

**Cause:** In UWP, creating a `CompositionDrawingSurface` with pixel format `DirectXPixelFormat.R16G16B16A16Float` enables HDR composition — the floating-point content is passed to the system compositor, which can display values outside [0,1]. In WinAppSDK, the same code does not produce HDR output. The surface appears to undergo a simple scRGB-to-sRGB gamma correction and renders as SDR. This suggests WinAppSDK may be composing to an 8-bit swap chain rather than a 16-bit floating-point swap chain.
> Source: Issue author in [#6291](https://github.com/microsoft/WindowsAppSDK/issues/6291)

**Symptoms:**
- `CompositionDrawingSurface` created with `R16G16B16A16Float` renders identically to SDR
- Values > 1.0 in HDR content do not appear brighter than SDR maximum
- SDR content is not dimmer (no SDR white level scaling at display's SDR white level / 80 nits)
- Photo viewing or media apps cannot display HDR content correctly in WinAppSDK

**Reproduction code summary:**
```csharp
// This produces HDR in UWP but SDR in WinAppSDK
var surface = compositionDevice.CreateDrawingSurface(
    pixelSize,
    DirectXPixelFormat.R16G16B16A16Float,
    DirectXAlphaMode.Premultiplied);

// Drawing HDR values > 1.0 should be brighter than SDR max
ds.FillRectangle(rect, CanvasSolidColorBrush.CreateHdr(device, new Vector4(5, 5, 5, 1)));
```

**Status:** Closed — but no confirmed workaround for achieving HDR composition in WinAppSDK was provided. This represents a UWP-to-WinAppSDK migration blocker for HDR content applications.

**Workaround (limited):**
1. For HDR content display, consider maintaining a UWP component or using DirectX interop with a custom swap chain
2. Monitor WinAppSDK releases for HDR composition support
3. No pure WinUI/Composition API workaround is currently available

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- For HDR (#6291): Using a custom DirectComposition swap chain with `DXGI_FORMAT_R16G16B16A16_FLOAT` created outside of WinUI's compositor may enable HDR rendering, but this bypasses the WinUI composition tree (not confirmed)
- For AOT (#5389): Some developers report that adding `[DynamicallyAccessedMembers]` attributes on certain types can also prevent trimming, though the `DynamicWindowsRuntimeCast` approach from @manodasanW is the recommended solution

---

## References

- [CsWinRT 2.3 Preview - NuGet](https://www.nuget.org/packages/Microsoft.Windows.CsWinRT/2.3.0-prerelease.251115.2)
- [DynamicWindowsRuntimeCast example - CsWinRT repo](https://github.com/microsoft/CsWinRT/blob/4475bb0d5a795bea964b4ce3db5db860233e7ba2/src/Tests/FunctionalTests/JsonValueFunctionCalls/Program.cs#L26)
- [WinAppSDK ReleaseInfo API](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.windows.applicationmodel.windowsappruntime.releaseinfo)
- [HDR and Advanced Color in Windows](https://learn.microsoft.com/en-us/windows/win32/direct3darticles/high-dynamic-range)

---

**Updated:** 2025-07-17 | **Confidence:** 0.7
**Sources:** #5389, #6219, #5323, #6291, CsWinRT documentation
