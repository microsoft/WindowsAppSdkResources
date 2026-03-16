# External / Interop Issues — Packaging, Device APIs, and Widgets

**Keywords:** packaging project, NuGet, stale assembly, MSIX bundle, FindPackagesByPackageFamily, 0x8007007A, ERROR_INSUFFICIENT_BUFFER, DeviceInformationPairing, Bluetooth, ProximityDevice, NFC, Widgets, WinUI 3

**Error Example:**
```
WinRT error 0x8007007A: "The data area passed to a system call is too small."
   at FindPackagesByPackageFamily()
```
```
System.InvalidCastException
   at WinRT.Interop.InitializeWithWindow.Initialize(DeviceInformation.Pairing, windowHandle)
```

---

## Quick Match

**You're seeing this if:**
- MSIX bundle contains wrong (stale) NuGet DLL versions after package downgrade
- Error `0x8007007A` on app startup from `FindPackagesByPackageFamily`
- Bluetooth device pairing UI fails to show in WinUI 3 desktop app
- NFC `ProximityDevice` events never fire after UWP → WinUI 3 migration
- Widgets panel: first widget (alphabetically) cannot be added

→ Check scenarios below for your specific cause

---

## Related Issues

- [#6253](https://github.com/microsoft/WindowsAppSDK/issues/6253) — Packaging project caches stale NuGet assembly references across builds (Status: Closed)
- [#6274](https://github.com/microsoft/WindowsAppSDK/issues/6274) — WinRT error 0x8007007A in `FindPackagesByPackageFamily` (Status: Open)
- [#3091](https://github.com/microsoft/WindowsAppSDK/issues/3091) — Unable to display device pairing UI in WinUI 3 app (Status: Closed)
- [#4356](https://github.com/microsoft/WindowsAppSDK/issues/4356) — ProximityDevice NFC events not triggered (Status: Open)
- [#6140](https://github.com/microsoft/WindowsAppSDK/issues/6140) — Widget at top of list cannot be added until switching away (Status: Open)

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

### Scenario 3: Device Pairing UI Does Not Display in WinUI 3 Desktop App

**Cause:** Calling `DeviceInformationPairing.PairAsync()` in a WinUI 3 desktop app does not show the system Bluetooth pairing UI. The pairing process silently fails after a few seconds. The `IInitializeWithWindow` workaround documented for other UWP controls throws `System.InvalidCastException` when applied to `DeviceInformation` or `DeviceInformation.Pairing` because these objects do not implement `IInitializeWithWindow`.
> Source: Issue [#3091](https://github.com/microsoft/WindowsAppSDK/issues/3091)

**Affected versions:** Windows App SDK 1.0+, Windows 11 (21H2)

**Fix:**
1. **Use `DeviceInformationCustomPairing`** with a custom handler instead of the automatic pairing UI:
```csharp
var customPairing = deviceInfo.Pairing.Custom;
customPairing.PairingRequested += (sender, args) =>
{
    // Handle pairing ceremony in your own UI
    args.Accept(); // or args.Accept(pin) for PIN-based pairing
};
var result = await customPairing.PairAsync(
    DevicePairingKinds.ConfirmOnly | DevicePairingKinds.DisplayPin);
```
2. **Build your own pairing confirmation dialog** in WinUI 3 XAML since the system pairing UI is not compatible with Win32 desktop windows.
3. For Bluetooth LE devices, consider using `BluetoothLEDevice.FromIdAsync()` followed by `DeviceInformation.Pairing.Custom.PairAsync()`.

**Verify:** Initiate Bluetooth pairing and confirm your custom UI appears and the device pairs successfully.

---

### Scenario 4: ProximityDevice NFC Events Not Triggered After UWP → WinUI 3 Migration

**Cause:** `Windows.Networking.Proximity.ProximityDevice` events (e.g., `SubscribeForMessage("NDEF", ...)`) work in UWP but do not fire in WinUI 3 / Windows App SDK desktop apps. The `ProximityDevice` API is a UWP-era API that relies on the UWP app model and brokered device access, which is not fully supported in the Win32 desktop app model used by WinUI 3.
> Source: Issue [#4356](https://github.com/microsoft/WindowsAppSDK/issues/4356)

**Affected versions:** Windows App SDK 1.5.2+, Windows 11 22H2

**Repro:**
```csharp
var proximityDevice = Windows.Networking.Proximity.ProximityDevice.GetDefault();
var messageSubscriptionId = proximityDevice.SubscribeForMessage("NDEF", (device, message) =>
{
    Console.WriteLine(message.Data.ToArray()); // Never fires in WinUI 3
});
```

**Fix / Workaround:**
1. **Use the PC/SC (WinSCard) API** for NFC smart card access as an alternative to `ProximityDevice`:
```csharp
// Use Windows.Devices.SmartCards namespace
var reader = await SmartCardReader.FromIdAsync(deviceId);
reader.CardAdded += OnCardAdded;
```
2. **Use a third-party NFC library** that wraps the Win32 PC/SC APIs (e.g., PCSC-Sharp).
3. Ensure `<DeviceCapability Name="proximity"/>` is declared in your app manifest (required but not sufficient for WinUI 3).
4. As a last resort, **keep NFC functionality in a separate UWP component** or background task that communicates with the main WinUI 3 app.

> ⚠️ This is an open issue. `ProximityDevice` is not officially supported in WinUI 3 desktop apps.

---

### Scenario 5: Widget at Top of Alphabetical List Cannot Be Added

**Cause:** In the Windows Widgets panel, a widget whose name starts with "A" (appearing at the top of the list) cannot be added — the "Add" button stays gray/inactive. Switching to another widget and back causes it to become addable. This appears to be a UI initialization/selection state bug in the Widgets Board.
> Source: Issue [#6140](https://github.com/microsoft/WindowsAppSDK/issues/6140)

**Affected versions:** Windows 11 25H2 (Build 26220.7535)

**Fix / Workaround:**
1. **Workaround for end users:** Select a different widget first, then switch back to the desired widget — it will become addable.
2. **For widget developers:** Consider naming your widget to not appear first alphabetically as a temporary workaround (e.g., prefix with a space or non-"A" character), though this is not ideal.

> ⚠️ This is a Windows Widgets Board bug — no SDK-level fix available. Report via Feedback Hub for Windows team triage.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- For stale NuGet caching (#6253): Some developers report that switching the solution configuration (Debug ↔ Release) and rebuilding can force correct DLL resolution.
- For NFC (#4356): The original reporter tried `DeviceCapability` declarations and multiple SDK versions without success. There is no confirmed workaround within the WinUI 3 app model.

---

## References

- [FindPackagesByPackageFamily function (Win32)](https://learn.microsoft.com/en-us/windows/win32/api/appmodel/nf-appmodel-findpackagesbypackagefamily)
- [Display UI objects in WinUI 3 (IInitializeWithWindow)](https://docs.microsoft.com/en-us/windows/apps/develop/ui-input/display-ui-objects)
- [DeviceInformationCustomPairing API](https://learn.microsoft.com/en-us/uwp/api/windows.devices.enumeration.deviceinformationcustompairing)
- [ProximityDevice API (UWP)](https://learn.microsoft.com/en-us/uwp/api/windows.networking.proximity.proximitydevice)
- [Windows Widgets overview](https://learn.microsoft.com/en-us/windows/apps/develop/widgets/)
- [MSIX packaging documentation](https://learn.microsoft.com/en-us/windows/msix/)

---

**Updated:** 2025-07-17 | **Confidence:** 0.6
**Sources:** #6253, #6274, #3091, #4356, #6140
