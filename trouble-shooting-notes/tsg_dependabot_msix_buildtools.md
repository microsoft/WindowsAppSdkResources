# Error: "The specified task executable 'MakePri.exe' could not be run" (MSB6003) - Microsoft.Windows.SDK.BuildTools

**Keywords:** MSB6003, MakePri.exe, Microsoft.Windows.SDK.BuildTools, Dependabot, MSIX, .NET, Linux

**Error Example:**
```
/home/dependabot/.nuget/packages/microsoft.windows.sdk.buildtools.msix/1.7.251221100/build/Microsoft.Windows.SDK.BuildTools.MSIX.MrtCore.PriGen.targets(785,5): error MSB6003: The specified task executable "MakePri.exe" could not be run. System.ComponentModel.Win32Exception (8): An error occurred trying to start process '/home/dependabot/.nuget/packages/microsoft.windows.sdk.buildtools/10.0.26100.4654/bin/10.0.26100.0/x86/makepri.exe' with working directory '/home/dependabot/dependabot-updater/repo/nxtsoft.Shared/nxtsoft.Shared.OIDC'. Exec format error [/home/dependabot/dependabot-updater/repo/nxtsoft.Shared/nxtsoft.Shared.OIDC/nxtsoft.Shared.OIDC.csproj]
```

---

## Quick Match

**You're seeing this if:**
- Error contains "The specified task executable 'MakePri.exe' could not be run"
- Error code is MSB6003
- Using Dependabot to update a project with `Microsoft.Windows.SDK.BuildTools` as a dependency
- Platform: Linux (e.g., GitHub-hosted runners)

→ Check scenarios below for your specific cause

---

## Related Issues

- [#6533](https://github.com/microsoft/WindowsAppSDK/issues/6533) - Dependabot fails to update library project that has microsoft.windows.sdk.buildtools as transitive dependency (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: Dependabot running on Linux cannot execute Windows-only tools like `MakePri.exe`

**Cause:** The `MakePri.exe` tool, part of `Microsoft.Windows.SDK.BuildTools`, is a Windows-only executable. Dependabot runs on Linux-based environments by default, which cannot execute `.exe` files. This results in the `MSB6003` error during the build process.
> Source: @lauren-ciha in [#6533](https://github.com/microsoft/WindowsAppSDK/issues/6533)  
> Source: @dongle-the-gadget in [#6533](https://github.com/microsoft/WindowsAppSDK/issues/6533)

**Fix:**
1. Ensure that `MakePri.exe` is not invoked during the restore process. This may involve modifying the build configuration to skip tasks that require `MakePri.exe` during restore.
   > Suggested by: @richlander in [#6533](https://github.com/microsoft/WindowsAppSDK/issues/6533)
2. Alternatively, configure Dependabot to run in a Windows-based environment (e.g., a Windows VM) to ensure compatibility with Windows-only tools.
   > Suggested by: @richlander in [#6533](https://github.com/microsoft/WindowsAppSDK/issues/6533)

**Verify:** Run Dependabot in the updated environment or with the modified configuration and confirm that the error no longer occurs.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- Avoid running `MakePri.exe` on Linux entirely.  
  > Suggested by: @davesmits in [#6533](https://github.com/microsoft/WindowsAppSDK/issues/6533)

---

## References

- [Issue #6533](https://github.com/microsoft/WindowsAppSDK/issues/6533) - Dependabot fails to update library project that has microsoft.windows.sdk.buildtools as transitive dependency
- [NuGet updater issue in dependabot-core](https://github.com/dependabot/dependabot-core/issues/13080)

---

**Updated:** 2026-06-15 | **Confidence:** 0.8  
**Sources:** [#6533](https://github.com/microsoft/WindowsAppSDK/issues/6533), [dependabot-core issue](https://github.com/dependabot/dependabot-core/issues/13080)