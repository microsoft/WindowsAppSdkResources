# Error: "DeploymentAgent exitcode:0x80070005" (0x80070005) - AppContainer and Partial Trust Issues

**Keywords:** DeploymentAgent exitcode:0x80070005, 0x80070005, Access is denied, AppContainer, Partial Trust, packageManagement, packageQuery

**Error Example:**
```
Exception thrown at 0x00007FFAF80A7F7A (KernelBase.dll) in App.exe: WinRT originate error - 0x80070005 : 'DeploymentAgent exitcode:0x80070005'.
C:\__w\1\s\dev\Deployment\DeploymentManager.cpp(486)\Microsoft.WindowsAppRuntime.dll!00007FF9A368AB17: (caller: 00007FF9A368B70B) ReturnHr(2) tid(60d0) 80070005 Access is denied.
    Msg:[DeploymentAgent exitcode:0x80070005] 
C:\__w\1\s\dev\Deployment\DeploymentManager.cpp(585)\Microsoft.WindowsAppRuntime.dll!00007FF9A368B7BE: (caller: 00007FF9A368ADB0) ReturnHr(3) tid(60d0) 80070005 Access is denied.
C:\__w\1\s\dev\Deployment\DeploymentManager.cpp(495)\Microsoft.WindowsAppRuntime.dll!00007FF9A368ADCF: (caller: 00007FF9A3689971) ReturnHr(4) tid(60d0) 80070
```

---

## Quick Match

**You're seeing this if:**
- Error contains "DeploymentAgent exitcode:0x80070005" or "Access is denied"
- You are running an AppContainer app with partial trust
- Platform: Windows, using Windows App SDK 1.8-preview

→ Check scenarios below for your specific cause

---

## Related Issues

- [#5740](https://github.com/microsoft/WindowsAppSDK/issues/5740) - 1.8-preview: AppContainer apps crash at startup (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: Missing `packageManagement` Capability

**Cause:** The app crashes because the `packageManagement` capability is not included in the app manifest. This capability is required for AppContainer apps running in partial trust.

> Source: @ssparach [MSFT] in [#5740](https://github.com/microsoft/WindowsAppSDK/issues/5740)

**Fix:**
1. Open your app's manifest file (`Package.appxmanifest`).
2. Add the following capability under the `<Capabilities>` section:
   ```xml
   <rescap:Capability Name="packageManagement" />
   ```
3. Save the manifest file and rebuild your app.

> ✅ Confirmed by: @ssparach [MSFT] in [#5740](https://github.com/microsoft/WindowsAppSDK/issues/5740)

**Verify:** Launch the app and confirm it no longer crashes at startup.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- None reported for this issue.

---

## References

- [Official docs: Configure a WinUI 3 project for AppContainer](https://learn.microsoft.com/windows/msix/msix-container#configure-a-winui-3-project-for-appcontainer)

---

**Updated:** 2026-03-17 | **Confidence:** 0.9
**Sources:** [#5740](https://github.com/microsoft/WindowsAppSDK/issues/5740)