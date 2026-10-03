# Error: "Cannot write to unvirtualized HKLM registry key in packaged app"

**Keywords:** HKLM, unvirtualized registry, packaged app, MSIX, unvirtualizedResources, StorageProviderSyncRootManager.Register, RegNotifyChangeKeyValue, registry virtualization, light/dark theme listener, ARM64, Copilot+ PCs

**Error Example:**
```
My app needs to write to some HKLM keys for Office interop support, however I can't find a solution.

I've looked at the Registry.dat but changes made using it are not visible to Office, I've looked at Package Support Framework, but it doesn't support ARM64 (which is required for Copilot+ PCs).

Something I don't quite understand, is that my app is packaged and therefore shouldn't be able to write to HKLM, yet when I call `StorageProviderSyncRootManager.Register()` keys are written to HKLM which are later read by explorer.exe - so how is that occurring?
```

---

## Quick Match

**You're seeing this if:**
- Your app is packaged (MSIX) and needs to write to or monitor registry keys (e.g., HKLM or HKCU).
- You encounter issues with registry virtualization or visibility of registry changes to other processes.
- You are using APIs like `StorageProviderSyncRootManager.Register()` or `RegNotifyChangeKeyValue`.
- You are targeting ARM64 or Copilot+ PCs and encountering issues with registry access.

→ Check scenarios below for your specific cause.

---

## Related Issues

- [#6410](https://github.com/microsoft/WindowsAppSDK/issues/6410) - I MUST write to an unvirtualized HKLM key in my packaged app (Status: Open)
- [#4075](https://github.com/microsoft/WindowsAppSDK/issues/4075) - RegNotifyChangeKeyValue is not working in WinUI3 app (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: Writing to unvirtualized HKLM keys in a packaged app

**Cause:** By default, packaged apps (MSIX) have their registry writes virtualized. This means that changes made to the registry are isolated to the app and not visible to other processes. However, it is possible to exclude specific registry keys from virtualization by declaring the appropriate capability in the app's manifest.

> Source: @DrusTheAxe [MSFT] and @wjk in [#6410](https://github.com/microsoft/WindowsAppSDK/issues/6410)

**Fix:**
1. Open your app's `Package.appxmanifest` file.
2. Add the following capability to the `<Capabilities>` section:
   ```xml
   <rescap:Capability Name="unvirtualizedResources" />
   ```
3. Specify the registry keys you want to exclude from virtualization in the `<rescap:RegistryKeys>` section. For example:
   ```xml
   <rescap:RegistryKeys>
       <rescap:RegistryKey KeyName="HKEY_LOCAL_MACHINE\SOFTWARE\YourAppKey" />
   </rescap:RegistryKeys>
   ```
4. Rebuild and redeploy your app package.

> ✅ Confirmed by: @wjk in [#6410](https://github.com/microsoft/WindowsAppSDK/issues/6410)

**Verify:** Check if the specified HKLM registry keys are now accessible and visible to other processes.

---

### Scenario 2: Writing to HKLM keys without admin privileges

**Cause:** Some HKLM keys may be writable by non-admin users if the user has write access to the specific key. This behavior can vary depending on the system configuration and the permissions set on the registry key.

> Source: @catmanjan in [#6410](https://github.com/microsoft/WindowsAppSDK/issues/6410)

**Fix:**
1. Verify the permissions on the specific HKLM key you are trying to write to.
2. If the key allows write access for non-admin users, you may be able to write to it without additional configuration.
3. If write access is not allowed, consider using the solution in Scenario 1 to exclude the key from virtualization.

---

### Scenario 3: RegNotifyChangeKeyValue not working in a packaged WinUI3 app

**Cause:** Packaged apps (MSIX) are subject to registry virtualization, which can interfere with monitoring changes to unvirtualized registry keys using `RegNotifyChangeKeyValue`. This issue is particularly relevant for apps targeting older versions of Windows 10 (e.g., 17763), which do not support the `unvirtualizedResources` capability.

> Source: @ChrisGuzak [MSFT] and @DarranRowe in [#4075](https://github.com/microsoft/WindowsAppSDK/issues/4075)

**Fix:**
1. Test if the issue is related to packaging:
   - Temporarily remove packaging by adding `<WindowsPackageType>None</WindowsPackageType>` to your `.vcxproj` file.
   - Run the app and verify if `RegNotifyChangeKeyValue` works as expected.
2. If the issue is confirmed to be related to packaging, consider the following options:
   - Use the `unvirtualizedResources` capability in your app's manifest to disable registry virtualization for the specific key you want to monitor. Note that this requires Windows 10 version 18362 or later.
     ```xml
     <rescap:Capability Name="unvirtualizedResources" />
     <rescap:RegistryKeys>
         <rescap:RegistryKey KeyName="HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize" />
     </rescap:RegistryKeys>
     ```
   - If targeting Windows 10 version 17763 or earlier, this capability is not supported. As a workaround, consider using an alternative method to detect theme changes, such as handling the `WM_SETTINGSCHANGE` message and looking for the "ImmersiveColorSet" string.

> Source: @DarranRowe in [#4075](https://github.com/microsoft/WindowsAppSDK/issues/4075)

**Verify:** Test the app on the target Windows version to ensure that `RegNotifyChangeKeyValue` works as expected after applying the fix.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- **Behavior of HKLM writes with `registry.dat`**: 
  - Writes to `HKLM\Software` may succeed if the key is not part of the app's `registry.dat` hive and the user has sufficient privileges.
  - Keys in `registry.dat` are read-only because they are part of the app package content.
  > Source: @CarlosNihelton in [#6410](https://github.com/microsoft/WindowsAppSDK/issues/6410)

---

## References

- [Issue #6410](https://github.com/microsoft/WindowsAppSDK/issues/6410)
- [Issue #4075](https://github.com/microsoft/WindowsAppSDK/issues/4075)
- [Example of unvirtualized registry keys in CascadiaPackage](https://github.com/microsoft/terminal/blob/7a83c0f1679ccac4c3f24f031bf403bd000ab320/src/cascadia/CascadiaPackage/Package.appxmanifest#L33-L37)
- [Flexible Virtualization Documentation](https://learn.microsoft.com/en-us/windows/msix/desktop/flexible-virtualization)
- [Desktop to UWP: Behind the Scenes](https://learn.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-behind-the-scenes#common-registry-operations)

---

**Updated:** 2026-06-08 | **Confidence:** 0.8
**Sources:** [#6410](https://github.com/microsoft/WindowsAppSDK/issues/6410), [#4075](https://github.com/microsoft/WindowsAppSDK/issues/4075), [CascadiaPackage example](https://github.com/microsoft/terminal/blob/7a83c0f1679ccac4c3f24f031bf403bd000ab320/src/cascadia/CascadiaPackage/Package.appxmanifest#L33-L37)