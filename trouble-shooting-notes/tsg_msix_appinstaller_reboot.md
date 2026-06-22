# Error: "Install Operation Failed" (0x80070057, 0x80D05011) - MSIX AppInstaller

**Keywords:** Install Operation Failed, 0x80070057, 0x80D05011, MSIX, AppInstaller, InstallFromUriAsync, AddPackageByUriAsync

**Error Example:**
From a Windows 11 machine:
```
[11648] [Sat Jan 28 00:58:09 2023]{11648d} [InstallFromUriAsync] -> Setting AddPackageOptions: ForceTargetAppShutdown
[11648] [Sat Jan 28 00:58:09 2023]{11648d} [InstallFromUriAsync] -> Setting AddPackageOptions: ExpectedDigests: [https://install.eartrumpet.app/dev/EarTrumpet.Package.appinstaller] -> [Dlh8iwqPPrYDppX/XBEfLw1LKnPcW7KxRaZJCi9cOvo=]
[11648] [Sat Jan 28 00:58:09 2023]{11648d} [InstallFromUriAsync] -> Setting AddPackageOptions: ExpectedDigests: [https://install.eartrumpet.app/dev/EarTrumpet.Package_2.2.2.12_x86.appxbundle] -> [UraqUvwuxDvm4j0ib2MOmMtATJMlp8Azu4CSJnb/HCI=]
[11648] [Sat Jan 28 00:58:09 2023]{11648d} [InstallFromUriAsync] -> Setting AddPackageOptions: ExpectedDigests: [file:///C:/Users/Rafael/Downloads/EarTrumpet.Package%20(1).appinstaller] -> [fpIVgBWB8Sduap8i1mpWT2xDn/xgTms0ZTPua0RyATc=]
[11648] [Sat Jan 28 00:58:09 2023]{11648d} [InstallFromUriAsync] -> Deploying AppInstaller extension file
[11648] [Sat Jan 28 00:58:09 2023]{11648d} [InstallFromUriAsync] -> Starting AddPackageByUriAsync()
[11648] [Sat Jan 28 00:58:09 2023]{11648d} StartInstallOperation -> Install Operation Failed: 0x80070057
```

From a Windows 10 machine:
```
[9488] [Sat Jan 28 00:59:57 2023]{9488d} StartInstallOperation -> URI: file:///C:/Users/Rafael/Downloads/EarTrumpet.Package%20(4).appinstaller
[9488] [Sat Jan 28 00:59:57 2023]{9488d} GetIsSelfElevatedAppServi
```

---

## Quick Match

**You're seeing this if:**
- Error contains "Install Operation Failed: 0x80070057" or "0x80D05011"
- Using MSIX AppInstaller to install or update an app
- Server-side `.appinstaller` file has been modified (e.g., dependencies, update settings, file encoding)
- Platform: Windows 10 or Windows 11

→ Check scenarios below for your specific cause

---

## Related Issues

- [#3378](https://github.com/microsoft/WindowsAppSDK/issues/3378) - MSIX AppInstaller fails if `.appinstaller` changes server-side, requires a system reboot to clear (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: Server-side changes to `.appinstaller` file cause installation failure

**Cause:** Modifications to the `.appinstaller` file on the server (e.g., changes to dependencies, update settings, or file encoding) can cause the MSIX AppInstaller to fail with errors such as `0x80070057` or `0x80D05011`. This issue was observed on Windows 10 and early builds of Windows 11.

> Source: @riverar in [#3378](https://github.com/microsoft/WindowsAppSDK/issues/3378)

**Fix:**
1. Reboot the system to clear any cached state related to the `.appinstaller` file.
2. Retry the installation after the reboot.

> ✅ Confirmed by: @riverar in [#3378](https://github.com/microsoft/WindowsAppSDK/issues/3378)

**Verify:** Attempt the installation again after rebooting. If the issue persists, ensure the `.appinstaller` file is correctly configured and accessible.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- None provided in the issue comments.

---

## References

- [Issue #3378](https://github.com/microsoft/WindowsAppSDK/issues/3378)
- [TFS 30117191](https://aka.ms/30117191)

---

**Updated:** 2026-06-22 | **Confidence:** 0.8
**Sources:** [#3378](https://github.com/microsoft/WindowsAppSDK/issues/3378)