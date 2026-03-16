# Error: "XamlParseException: XAML parsing failed" — Renamed Single-File Unpackaged App / Broken Runtime Links

**Keywords:** XamlParseException, XAML parsing failed, single-file, unpackaged, rename executable, PublishSingleFile, runtime links broken, 2.0 preview, installer binaries, MRM.dll crash

**Error Example:**
```
Microsoft.UI.Xaml.Markup.XamlParseException: XAML parsing failed.
   at WinRT.ExceptionHelpers.<ThrowExceptionForHR>g__Throw|38_0(Int32)
   at ABI.Microsoft.UI.Xaml.IApplicationStaticsMethods.LoadComponent(...)
   at Microsoft.UI.Xaml.Application.LoadComponent(Object, Uri, ComponentResourceLocation)
   at TestApp1.MainWindow.InitializeComponent()
```

---

## Quick Match

**You're seeing this if:**
- You published a WinUI 3 app as single-file unpackaged and renamed the `.exe`
- Error contains "XamlParseException" or "XAML parsing failed" after renaming
- WASDK 2.0 Preview runtime download links return 404
- WASDK 1.6.3 installers ship binaries that still crash despite documented fixes

→ Check scenarios below for your specific cause

---

## Related Issues

- [#6248](https://github.com/microsoft/WindowsAppSDK/issues/6248) — Renaming published executable makes Single-File Unpackaged App crash (Status: Open)
- [#6220](https://github.com/microsoft/WindowsAppSDK/issues/6220) — WASDK 2.0 Preview runtime links are broken (Status: Closed/Fixed)
- [#4977](https://github.com/microsoft/WindowsAppSDK/issues/4977) — 1.6.3 installers contain binaries built from older source code (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: Renaming Single-File Published Executable Causes XamlParseException

**Cause:** When a WinUI 3 app is published as a single-file, self-contained, unpackaged app and the resulting `.exe` is renamed (e.g., `TestApp1.exe` → `TestApp11.exe`), XAML resource loading fails. The `Application.LoadComponent` call uses URI-based resource paths that are tied to the original executable name, so renaming breaks the lookup.
> Source: Issue reporter in [#6248](https://github.com/microsoft/WindowsAppSDK/issues/6248)

**Reproduction config:**
```xml
<PropertyGroup>
    <WindowsPackageType>None</WindowsPackageType>
    <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
</PropertyGroup>
```
```xml
<!-- Publish profile -->
<PublishSingleFile>true</PublishSingleFile>
<SelfContained>true</SelfContained>
<PublishTrimmed>true</PublishTrimmed>
```

**Fix:** No official fix available yet. **Do not rename the published executable.**

**Workaround:** Keep the original executable name after publishing. If branding requires a different name, change the `AssemblyName` in your `.csproj` before publishing instead of renaming the output file.

> ⚠️ @aepot in [#6248](https://github.com/microsoft/WindowsAppSDK/issues/6248) reports being stuck on WASDK 1.7 because of this bug.

---

### Scenario 2: WASDK 2.0 Preview Runtime Download Links Broken

**Cause:** The `aka.ms` download links for WASDK 2.0 Preview 1 runtime installers were returning 404 errors shortly after the preview release.
> Source: Issue reporter in [#6220](https://github.com/microsoft/WindowsAppSDK/issues/6220)

**Affected links:**
```
https://aka.ms/windowsappsdk/2.0/2.0-preview1/windowsappruntimeinstall-x64.exe
https://aka.ms/windowsappsdk/2.0/2.0-preview1/windowsappruntimeinstall-x86.exe
https://aka.ms/windowsappsdk/2.0/2.0-preview1/windowsappruntimeinstall-arm64.exe
https://aka.ms/windowsappsdk/2.0/2.0-preview1/Microsoft.WindowsAppRuntime.Redist.2.0.zip
```

**Fix:** Links have been corrected by Microsoft.
> ✅ Confirmed by: @lauren-ciha (MEMBER) in [#6220](https://github.com/microsoft/WindowsAppSDK/issues/6220)

**Working Redist link:**
```
https://aka.ms/windowsappsdk/2.0/2.0-preview1/Microsoft.WindowsAppRuntime.Redist.2.0-preview1.zip
```

**Verify:** Check the [official WASDK downloads page](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads) for current links.

---

### Scenario 3: WASDK 1.6.3 Installers Contain Stale Binaries (MRM.dll Crash)

**Cause:** The WASDK 1.6.3 (1.6.241114003) runtime installers shipped binaries built from an older commit in the `release/1.6-stable` branch rather than from the `v1.6.3` tag. Specifically, `MRM.dll` did not include the [buffer overrun fix](https://github.com/microsoft/WindowsAppSDK/commit/1db04b650194f090ba1b52ae48f61277737c19f0), causing crashes in apps that use MRT resource loading.
> Source: Issue reporter in [#4977](https://github.com/microsoft/WindowsAppSDK/issues/4977)

**Diagnostic:**
```
# Check installed MRM.dll version/timestamp
C:\Program Files\WindowsApps\Microsoft.WindowsAppRuntime.1.6_6000.318.2304.0_arm64__8wekyb3d8bbwe\MRM.dll
```

**Fix:** WASDK 1.6 has reached End of Support. Upgrade to WASDK 1.8 or later.
> Source: @alexlamtest (CONTRIBUTOR) in [#4977](https://github.com/microsoft/WindowsAppSDK/issues/4977)

**Additional note:** @tpoint75 in [#4977](https://github.com/microsoft/WindowsAppSDK/issues/4977) also noted that runtime installers had incorrect version numbers.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- For #6248: Changing `AssemblyName` before build instead of post-publish rename may avoid the issue (community suggestion — untested)

---

## Diagnostic Steps

### For Scenario 1 (Renamed EXE crash):
```powershell
# Check original executable name from assembly metadata
[System.Reflection.AssemblyName]::GetAssemblyName("C:\path\to\published\app.exe").Name

# Compare XAML resource URIs embedded in the assembly
# The URI paths reference the original assembly name
ildasm /text "C:\path\to\published\app.exe" | Select-String "ms-appx"
```

### For Scenario 2 (Broken links):
```powershell
# Test if aka.ms redirect resolves correctly
Invoke-WebRequest -Uri "https://aka.ms/windowsappsdk/2.0/2.0-preview1/windowsappruntimeinstall-x64.exe" -Method Head
```

### For Scenario 3 (Stale binaries):
```powershell
# Check installed MRM.dll file version and timestamp
Get-Item "C:\Program Files\WindowsApps\Microsoft.WindowsAppRuntime.1.6*\MRM.dll" | 
    Select-Object Name, VersionInfo, LastWriteTime

# Compare against expected version from release tag
# Expected: built from v1.6.3 tag commit, not release/1.6-stable branch
```

### General version verification:
```powershell
# Check installed WASDK runtime version
Get-AppxPackage *WindowsAppRuntime* -AllUsers | Select-Object Name, Version, Architecture
```

---

## References

- [WASDK Downloads](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads)
- [Single-file deployment docs](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file)
- [WASDK Release Notes](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/stable-channel)

---

**Updated:** 2026-03-13 | **Confidence:** 0.7
**Sources:** #6248, #6220, #4977
