# Error: App Crash on Launch / "FindPackagesByPackageFamily" Failure — WASDK 1.8 on Windows 10

**Keywords:** FindPackagesByPackageFamily, WASDK 1.8, Windows 10, 17763, 19041, DeploymentManager, crash on launch, 0x80070005, DDLM PackageFullName, MSIX.inventory, DeploymentManager.Initialize, experimental builds

**Error Example:**
```
Faulting module name: Microsoft.UI.Xaml.dll, version: 3.1.8.0
Exception code: 0xc000027b
```
```
The program "[3468] App1.exe" has exited, returning 2147942405 (0x80070005).
```

---

## Quick Match

**You're seeing this if:**
- WinUI 3 / WASDK 1.8+ packaged app crashes immediately on launch on Windows 10 (builds 17763, 19041, 19045)
- `FindPackagesByPackageFamily()` fails to locate WASDK 1.8 runtime packages on Windows 10
- `DeploymentManager.Initialize()` causes a fast fail crash
- No error dialog — the app silently exits
- Same app works fine on Windows 11

→ Check scenarios below for your specific cause

---

## Related Issues

- [#6117](https://github.com/microsoft/WindowsAppSDK/issues/6117) — FindPackagesByPackageFamily unable to find WASDK 1.8 packages (Status: Open)
- [#6254](https://github.com/microsoft/WindowsAppSDK/issues/6254) — WindowAppSdk v1.8 cannot run on lower versions of Windows 10 (Status: Open)
- [#6156](https://github.com/microsoft/WindowsAppSDK/issues/6156) — MSIX.inventory has wrong DDLM PackageFullName (Status: Closed/Fixed)
- [#5605](https://github.com/microsoft/WindowsAppSDK/issues/5605) — 1.8 exp4: App crashes at `DeploymentManager.Initialize();` (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: DeploymentManager Auto-Initializer Fails on Windows 10

**Cause:** WASDK 1.8 includes a static global initializer that calls `FindPackagesByPackageFamily()` via `DeploymentManager.Initialize()`. On Windows 10 (especially builds 17763 and 19041), this call fails to find the runtime packages, causing the app to crash before any UI loads. The `WinAppRuntime.Main.1.8` package may not be registered, and `DeploymentManager.Initialize()` crashes instead of registering it.
> Source: @HO-COOH in [#6117](https://github.com/microsoft/WindowsAppSDK/issues/6117), confirmed in [#6254](https://github.com/microsoft/WindowsAppSDK/issues/6254)

**Fix (C# projects):**
Add the following to your `.csproj` to disable the auto-initializer:
```xml
<PropertyGroup>
    <WindowsAppSdkBootstrapInitialize>false</WindowsAppSdkBootstrapInitialize>
</PropertyGroup>
```

**Fix (C++ projects):**
Disable auto-initializer in your `.vcxproj`:
```xml
<PropertyGroup>
    <WindowsAppSdkBootstrapInitialize>false</WindowsAppSdkBootstrapInitialize>
</PropertyGroup>
```

> ✅ Confirmed by: @HO-COOH in [#6117](https://github.com/microsoft/WindowsAppSDK/issues/6117), @Marv51 suggested the same workaround in [#6254](https://github.com/microsoft/WindowsAppSDK/issues/6254)

**Verify:**
```powershell
# Confirm runtime packages are installed for the user
Get-AppxPackage micro*win*app*run*1.8* -AllUsers
# Confirm Main package registration
Get-AppxPackage MicrosoftCorporationII.WinAppRuntime.Main.1.8
```

---

### Scenario 2: WinAppRuntime.Main.1.8 Package Not Registered

**Cause:** On affected Windows 10 machines, `Get-AppxPackage MicrosoftCorporationII.WinAppRuntime.Main.1.8` returns nothing. The Framework packages (x86/x64) are registered but the Main package is missing. `DeploymentManager.Initialize()` is supposed to detect and register it, but crashes instead.
> Source: @DrusTheAxe and @HO-COOH in [#6117](https://github.com/microsoft/WindowsAppSDK/issues/6117)

**Fix:**
1. Disable the auto-initializer as shown in Scenario 1.
2. If needed, manually register the Main package or reinstall the WASDK 1.8 runtime.

**Diagnostic steps** (suggested by @DrusTheAxe in [#6117](https://github.com/microsoft/WindowsAppSDK/issues/6117)):
```powershell
# Check all WASDK 1.8 packages
Get-AppxPackage micro*win*app*run*1.8* -AllUsers
# Check your app's package
Get-AppxPackage *your*package*name* -AllUsers
# Check appxmanifest.xml for PackageDependency
# Verify TargetDeviceFamily MaxVersionTested values
```

---

### Scenario 3: MSIX.inventory Has Incorrect DDLM PackageFullName

**Cause:** The `MSIX.inventory` file in the `Microsoft.WindowsAppSDK.Runtime` NuGet package listed an incorrect prefix for the DDLM package. The file used `Microsoft.WindowsAppRuntime.DDLM` but the actual installed package name uses `Microsoft.WinAppRuntime.DDLM`, causing package validation checks to fail.
> Source: Issue reporter in [#6156](https://github.com/microsoft/WindowsAppSDK/issues/6156)

**Fix:** This was fixed in the aggregator.
> ✅ Confirmed by: @ssparach and @guimafelipe (CONTRIBUTOR) in [#6156](https://github.com/microsoft/WindowsAppSDK/issues/6156)

**Verify:**
Check that the DDLM entry in `MSIX.inventory` uses the `Microsoft.WinAppRuntime.DDLM` prefix:
```
Microsoft.WindowsAppRuntime.DDLM.1.8.msix=Microsoft.WinAppRuntime.DDLM.<version>-<arch2char>...
```

---

### Scenario 4: DeploymentManager.Initialize() Called Explicitly in Packaged Apps

**Cause:** In WASDK 1.8 experimental builds, explicitly calling `DeploymentManager.Initialize()` in packaged apps causes a fast fail crash. This is because the auto-initializer is already enabled by default, and calling the API explicitly results in duplicate initialization attempts.
> Source: @ssparach in [#5605](https://github.com/microsoft/WindowsAppSDK/issues/5605)

**Fix:**
Disable the auto-initializer in your `.csproj` file:
```xml
<PropertyGroup>
    <WindowsAppSdkDeploymentManagerInitialize>false</WindowsAppSdkDeploymentManagerInitialize>
</PropertyGroup>
```

> ✅ Confirmed by: @ssparach in [#5605](https://github.com/microsoft/WindowsAppSDK/issues/5605)

**Verify:**
Ensure the crash is resolved after applying the fix. If the issue persists, check for other scenarios listed in this guide.

---

### Scenario 5: Third-Party Apps (AppInstaller, Xbox) Also Crashing

**Cause:** The issue is not limited to custom apps. Any app using `WindowsAppRuntime.1.8` framework can crash on Windows 10, including first-party Microsoft apps like AppInstaller and Xbox.
> Source: @RemyYYZ in [#6117](https://github.com/microsoft/WindowsAppSDK/issues/6117) and [#6254](https://github.com/microsoft/WindowsAppSDK/issues/6254)

**Symptoms:**
```
Faulting application name: AppInstaller.exe, version: 1.27.460.0
Faulting module name: Microsoft.UI.Xaml.dll, version: 3.1.8.0
Exception code: 0xc000027b
Windows Version: 10.0.19045.6937
```

**Fix:**
No user-side workaround for pre-built apps. This requires a fix from Microsoft in the WASDK runtime. For your own apps, use the auto-initializer disable workaround from Scenario 1.

---

## ⚠️ Known Issues / Unverified Suggestions

### Known Issue: FindPackagesByPackageFamily Fails on Older Windows 10 Versions

**Cause:** On Windows 10 17763, `FindPackagesByPackageFamily()` fails to locate WASDK 1.8 runtime packages, causing `DeploymentManager.Initialize()` to crash. This affects all apps using WASDK 1.8 unless the auto-initializer is disabled.
> Source: @HO-COOH in [#6117](https://github.com/microsoft/WindowsAppSDK/issues/6117)

**Workaround:** Disable the auto-initializer as described in Scenario 1. No permanent fix is available yet.

---

## Diagnostic Steps

```powershell
# 1. Check Windows version
winver.exe
# Or programmatically:
[System.Environment]::OSVersion.Version

# 2. Check all WASDK 1.8 runtime packages
Get-AppxPackage micro*win*app*run*1.8* -AllUsers

# 3. Check if Main package is registered (should NOT be empty)
Get-AppxPackage MicrosoftCorporationII.WinAppRuntime.Main.1.8

# 4. Check your app's package dependencies
# Open your appxmanifest.xml and look for:
# <PackageDependency Name="Microsoft.WindowsAppRuntime.1.8" .../>

# 5. Check Event Viewer for crash details
Get-WinEvent -LogName Application -MaxEvents 20 | 
    Where-Object { $_.Message -like "*WindowsAppRuntime*" -or $_.Message -like "*Microsoft.UI.Xaml*" }

# 6. Verify DDLM package name format in MSIX.inventory
# Navigate to NuGet package cache and check:
# .nuget\packages\microsoft.windowsappsdk.runtime\<ver>\tools\MSIX\<platform>\MSIX.inventory
```

---

## References

- [WASDK Deployment project properties](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/project-properties)
- [WASDK Downloads](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads)
- [DeploymentManager.Initialize API](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.windows.applicationmodel.windowsappruntime.deploymentmanager.initialize)

---

**Updated:** 2026-03-17 | **Confidence:** 0.9
**Sources:** #6117, #6254, #6156, #5605