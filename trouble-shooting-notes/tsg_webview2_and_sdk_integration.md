# Error: "Could not copy the file 'WebView2Loader.dll' because it was not found" (MSB3030) - WebView2 Integration with Windows App SDK

**Keywords:** MSB3030, WebView2Loader.dll, WebView2, Windows App SDK, WinUI3, XAML.MapControl.WinUI

**Error Example:**
```
C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\amd64\Microsoft.Common.CurrentVersion.targets(5322,5): error MSB3030: Could not copy the file "c:\temp\xaml.mapcontrol.winui\12.1.0\lib\net8.0-windows10.0.17763\MapControl.WinUI\runtimes\win-arm64\native\WebView2Loader.dll" because it was not found.
C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\amd64\Microsoft.Common.CurrentVersion.targets(5322,5): error MSB3030: Could not copy the file "c:\temp\xaml.mapcontrol.winui\12.1.0\lib\net8.0-windows10.0.17763\MapControl.WinUI\runtimes\win-x64\native\WebView2Loader.dll" because it was not found.
C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\amd64\Microsoft.Common.CurrentVersion.targets(5322,5): error MSB3030: Could not copy the file "c:\temp\xaml.mapcontrol.winui\12.1.0\lib\net8.0-windows10.0.17763\MapControl.WinUI\runtimes\win-x86\native\WebView2Loader.dll" because it was not found.
```

---

## Quick Match

**You're seeing this if:**
- Error contains "MSB3030" and mentions "WebView2Loader.dll"
- Occurs when referencing a class library that uses `Microsoft.WindowsAppSDK` or `XAML.MapControl.WinUI`
- Platform: Windows 10 or Windows 11

→ Check scenarios below for your specific cause

---

## Related Issues

- [#5058](https://github.com/microsoft/WindowsAppSDK/issues/5058) - Error MSB3030 when referencing a class library with WebView2Loader.dll (Status: Closed)
- [#5489](https://github.com/microsoft/WindowsAppSDK/issues/5489) - WebView2 stopped working after installing a package (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: Missing WebView2Loader.dll in Class Library References

**Cause:** When referencing a class library that uses `Microsoft.WindowsAppSDK` or `XAML.MapControl.WinUI`, the `WebView2Loader.dll` file may not be properly included in the build output. This issue is linked to the version of the `Microsoft.WindowsAppSDK` package used in the project.
> Source: @RDMacLachlan [MSFT] in [#5058](https://github.com/microsoft/WindowsAppSDK/issues/5058)

**Fix:**
1. Update the `Microsoft.WindowsAppSDK` package in your project to version `1.7` or later. This version includes updates to the WebView2 reference that resolve the issue.
2. Ensure that all dependent projects in your solution are also updated to use the same version of `Microsoft.WindowsAppSDK`.

> ✅ Confirmed by: @RDMacLachlan [MSFT] in [#5058](https://github.com/microsoft/WindowsAppSDK/issues/5058)

**Verify:** Rebuild your solution and confirm that the error no longer occurs.

---

### Scenario 2: WebView2 Not Working After Installing a Package

**Cause:** The issue occurs due to a conflict with the default C#/WinRT projection used by WebView2 in certain scenarios, such as when integrating with WPF.
> Source: @1592363624 in [#5489](https://github.com/microsoft/WindowsAppSDK/issues/5489)

**Fix:**
1. Add the following property to your project file to override the default use of WebView2's C#/WinRT projection:
   ```xml
   <WebView2EnableCsWinRTProjection>false</WebView2EnableCsWinRTProjection>
   ```

> ✅ Confirmed by: @1592363624 in [#5489](https://github.com/microsoft/WindowsAppSDK/issues/5489)

**Verify:** Test your application to ensure that WebView2 is functioning as expected.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- None reported for these issues.

---

## References

- [#5058](https://github.com/microsoft/WindowsAppSDK/issues/5058)
- [#5489](https://github.com/microsoft/WindowsAppSDK/issues/5489)
- [Failure to build when Microsoft.WindowsAppSDK 1.6 referenced through secondary nuget package #4807](https://github.com/microsoft/WindowsAppSDK/issues/4807)

---

**Updated:** 2026-03-17 | **Confidence:** 0.9
**Sources:** [#5058](https://github.com/microsoft/WindowsAppSDK/issues/5058), [#5489](https://github.com/microsoft/WindowsAppSDK/issues/5489)