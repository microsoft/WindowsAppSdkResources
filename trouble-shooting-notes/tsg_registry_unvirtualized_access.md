# Error: "Cannot write to unvirtualized HKLM registry key in packaged app"

**Keywords:** HKLM, unvirtualized registry, packaged app, MSIX, unvirtualizedResources, StorageProviderSyncRootManager.Register

**Error Example:**
```
My app needs to write to some HKLM keys for Office interop support, however I can't find a solution.

I've looked at the Registry.dat but changes made using it are not visible to Office, I've looked at Package Support Framework, but it doesn't support ARM64 (which is required for Copilot+ PCs).

Something I don't quite understand, is that my app is packaged and therefore shouldn't be able to write to HKLM, yet when I call `StorageProviderSyncRootManager.Register()` keys are written to HKLM which are later read by explorer.exe - so how is that occurring?
```

---

## Quick Match

**You're seeing this if:**
- Your app is packaged (MSIX) and needs to write to HKLM registry keys.
- You encounter issues with registry virtualization or visibility of registry changes to other processes.
- You are using APIs like `StorageProviderSyncRootManager.Register()`.

→ Check scenarios below for your specific cause.

---

## Related Issues

- [#6410](https://github.com/microsoft/WindowsAppSDK/issues/6410) - I MUST write to an unvirtualized HKLM key in my packaged app (Status: Open)

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

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- None provided in the issue comments.

---

## References

- [Issue #6410](https://github.com/microsoft/WindowsAppSDK/issues/6410)
- [Example of unvirtualized registry keys in CascadiaPackage](https://github.com/microsoft/terminal/blob/7a83c0f1679ccac4c3f24f031bf403bd000ab320/src/cascadia/CascadiaPackage/Package.appxmanifest#L33-L37)

---

**Updated:** 2026-04-20 | **Confidence:** 0.8
**Sources:** [#6410](https://github.com/microsoft/WindowsAppSDK/issues/6410), [CascadiaPackage example](https://github.com/microsoft/terminal/blob/7a83c0f1679ccac4c3f24f031bf403bd000ab320/src/cascadia/CascadiaPackage/Package.appxmanifest#L33-L37)