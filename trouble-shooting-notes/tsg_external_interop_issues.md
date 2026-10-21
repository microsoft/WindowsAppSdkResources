# External / Interop Issues — Packaging, Device APIs, and Widgets

**Keywords:** packaging project, NuGet, stale assembly, MSIX bundle, FindPackagesByPackageFamily, 0x8007007A, ERROR_INSUFFICIENT_BUFFER, DeviceInformationPairing, Bluetooth, ProximityDevice, NFC, Widgets, WinUI 3, HybridWebView.js, build failure, WindowsAppSDK upgrade, PackageDeploymentManager, RegisterPackageAsync, RPC failure

**Error Example:**
```
WinRT error 0x8007007A: "The data area passed to a system call is too small."
   at FindPackagesByPackageFamily()
```
```
System.InvalidCastException
   at WinRT.Interop.InitializeWithWindow.Initialize(DeviceInformation.Pairing, windowHandle)
```
```
Could not copy the file "C:\Users\Jochem\.nuget\packages\microsoft.maui.controls.core\9.0.120\lib\net9.0-windows10.0.19041\Microsoft.Maui\Handlers\HybridWebView\HybridWebView.js" because it was not found.
```
```
Exception thrown at 0x00007FFF613A83EA (KernelBase.dll) in PackageManagerRegisterTest.exe: WinRT originate error - 0x800706BE : 'The remote procedure call failed.'
Exception thrown at 0x00007FFF613A83EA (KernelBase.dll) in PackageManagerRegisterTest.exe: 0x000006BE: The remote procedure call failed.
```

---

## Quick Match

**You're seeing this if:**
- MSIX bundle contains wrong (stale) NuGet DLL versions after package downgrade
- Error `0x8007007A` on app startup from `FindPackagesByPackageFamily`
- Bluetooth device pairing UI fails to show in WinUI 3 desktop app
- NFC `ProximityDevice` events never fire after UWP → WinUI 3 migration
- Widgets panel: first widget (alphabetically) cannot be added
- Build fails with "Could not copy the file HybridWebView.js" after upgrading to Windows App SDK 1.8
- `PackageDeploymentManager.RegisterPackageAsync()` fails with RPC error while `RegisterPackageByFullNameAsync()` succeeds

→ Check scenarios below for your specific cause

---

## Related Issues

- [#6253](https://github.com/microsoft/WindowsAppSDK/issues/6253) — Packaging project caches stale NuGet assembly references across builds (Status: Closed)
- [#6274](https://github.com/microsoft/WindowsAppSDK/issues/6274) — WinRT error 0x8007007A in `FindPackagesByPackageFamily` (Status: Open)
- [#3091](https://github.com/microsoft/WindowsAppSDK/issues/3091) — Unable to display device pairing UI in WinUI 3 app (Status: Closed)
- [#4356](https://github.com/microsoft/WindowsAppSDK/issues/4356) — ProximityDevice NFC events not triggered (Status: Open)
- [#6140](https://github.com/microsoft/WindowsAppSDK/issues/6140) — Widget at top of list cannot be added until switching away (Status: Open)
- [#6032](https://github.com/microsoft/WindowsAppSDK/issues/6032) — "Could not copy the file HybridWebView.js" when upgrading to 1.8.251106002 (Status: Closed)
- [#4791](https://github.com/microsoft/WindowsAppSDK/issues/4791) — `PackageDeploymentManager.RegisterPackageAsync()` fails while `RegisterPackageByFullNameAsync()` succeeds (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: Packaging Project Caches Stale NuGet Assembly References Across Builds

**Cause:** When using the Packaging project to produce MSIX bundles, downgrading (or upgrading) a NuGet package reference and rebuilding results in the previously-built DLL version being included in the bundle. Neither **Clean Solution** nor restarting Visual Studio resolves this. The packaging project caches assembly paths and does not properly invalidate them when NuGet versions change.
> Source: Issue [#6253](https://github.com/microsoft/WindowsAppSDK/issues/6253) — also filed on [VS Developer Community](https://developercommunity.visualstudio.com/t/Packaging-project-caches-stale-NuGet-ass/110512955)

**Affected versions:** Visual Studio 2026, Windows App SDK (MSIX packaging), any NuGet package

**Repro:**  
1. Reference `CommunityToolkit.Mvvm` version **8.3.2**, build MSIX bundle → DLL is 8.3.2.1 ✅  
2. Upgrade to **8.4.0**, Clean Solution, rebuild → DLL is 8.4.0.1 ✅  
3. Downgrade back to **8.3.2**, Clean Solution, rebuild → DLL is still **8.4.0.1** ⚠️  

**Fix:**  
1. **Manually delete the packaging project output folders** before rebuilding:  
   - Delete `App1 (Package)\AppPackages\` folder  
   - Delete `App1 (Package)\bin\` and `App1 (Package)\obj\` folders  
2. **Force a full NuGet restore** after version change:  
```powershell
dotnet nuget locals all --clear
dotnet restore
```
3. **Rebuild** (not just Build) the entire solution after clearing caches.

**Verify:** Open the resulting `.msixbundle` in 7-Zip and confirm the DLL version matches the referenced NuGet version.

---

### Scenario 2: WinRT Error 0x8007007A — "Data Area Passed to a System Call Is Too Small"

**Cause:** On app startup, `FindPackagesByPackageFamily` reports `ERROR_INSUFFICIENT_BUFFER` (HRESULT `0x8007007A`). The debugger shows `bufferLength = 80` and corrupted locals (huge vector size, invalid pointer), indicating a buffer overflow. The root cause is a buffer size mismatch: the API returns a required **character count** for a `PWSTR` buffer, but the calling code likely allocated `bufferLength` **bytes** instead of `bufferLength * sizeof(wchar_t)`.
> Source: Issue [#6274](https://github.com/microsoft/WindowsAppSDK/issues/6274)

**Affected versions:** Windows App SDK 1.8.5 (1.8.260209005), Windows 11 24H2

**Important note:** This exception surfaces only when **"Break when thrown"** is enabled for all exceptions in Visual Studio's Exception Settings. In normal execution, the SDK may handle this internally.

**Fix:**  
1. **Check if this is a first-chance exception only.** Uncheck "Break when thrown" for `WinRT originate error` exceptions in Visual Studio → Exception Settings. If the app runs normally, this is an internal SDK exception that is caught and handled.  
2. If calling `FindPackagesByPackageFamily` in your own code, ensure proper two-call pattern:  
```cpp
UINT32 count = 0;
UINT32 bufferLength = 0;
// First call: get required sizes
FindPackagesByPackageFamily(familyName, PACKAGE_FILTER_HEAD, &count, nullptr, &bufferLength, nullptr, nullptr);
// Allocate with correct units (characters, not bytes)
PWSTR buffer = new WCHAR[bufferLength]; // bufferLength is in characters
PWSTR* packageFullNames = new PWSTR[count];
// Second call: retrieve data
FindPackagesByPackageFamily(familyName, PACKAGE_FILTER_HEAD, &count, packageFullNames, &bufferLength, buffer, nullptr);
```
3. Handle `ERROR_INSUFFICIENT_BUFFER` return code as expected (it is the normal first-call response).

**Verify:** Run app with first-chance exception breaking disabled and confirm no user-visible crash.

---

### Known Issue: `PackageDeploymentManager.RegisterPackageAsync()` Fails While `RegisterPackageByFullNameAsync()` Succeeds

**Cause:** `PackageDeploymentManager.RegisterPackageAsync()` fails with `PackageDeploymentStatus.CompletedFailure` and empty error properties, while the WinRT method `RegisterPackageByFullNameAsync()` succeeds. Debugging shows RPC failure (`0x800706BE`) and "Element not found" (`0x80070490`). This is due to improper memory handling in the WASDK method when passing `const hstring&` parameters.
> Source: @DrusTheAxe in [#4791](https://github.com/microsoft/WindowsAppSDK/issues/4791)

**Affected versions:** Windows App SDK 1.7+, Windows 11 22H2+

**Workaround:**  
1. Use the WinRT method `PackageManager.RegisterPackageByFullNameAsync()` instead of the WASDK method:
```csharp
var packageManager = new Windows.Management.Deployment.PackageManager();
await packageManager.RegisterPackageByFullNameAsync(packageFullName, null, DeploymentOptions.None);
```
2. Ensure your app is unpackaged or has the correct manifest capabilities (`packageManagement`).

> ⚠️ This issue is under investigation. A fix is expected in a future servicing update.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- For stale NuGet caching (#6253): Some developers report that switching the solution configuration (Debug ↔ Release) and rebuilding can force correct DLL resolution.
- For NFC (#4356): The original reporter tried `DeviceCapability` declarations and multiple SDK versions without success. There is no confirmed workaround within the WinUI 3 app model.
- For HybridWebView.js build failure (#6032): Some users suggest manually copying the missing file from a working version of the package, though this is not officially recommended.
- For `RegisterPackageAsync` (#4791): Some users report success by using unpackaged apps or switching to WinRT APIs.

---

## References

- [FindPackagesByPackageFamily function (Win32)](https://learn.microsoft.com/en-us/windows/win32/api/appmodel/nf-appmodel-findpackagesbypackagefamily)
- [Display UI objects in WinUI 3 (IInitializeWithWindow)](https://docs.microsoft.com/en-us/windows/apps/develop/ui-input/display-ui-objects)
- [DeviceInformationCustomPairing API](https://learn.microsoft.com/en-us/uwp/api/windows.devices.enumeration.deviceinformationcustompairing)
- [ProximityDevice API (UWP)](https://learn.microsoft.com/en-us/uwp/api/windows.networking.proximity.proximitydevice)
- [Windows Widgets overview](https://learn.microsoft.com/en-us/windows/apps/develop/widgets/)
- [MSIX packaging documentation](https://learn.microsoft.com/en-us/windows/msix/)
- [dotnet/maui#32683](https://github.com/dotnet/maui/issues/32683)
- [PackageManager API (WinRT)](https://learn.microsoft.com/en-us/uwp/api/windows.management.deployment.packagemanager)

---

**Updated:** 2026-03-23 | **Confidence:** 0.7
**Sources:** #6253, #6274, #3091, #4356, #6140, #6032, #4791