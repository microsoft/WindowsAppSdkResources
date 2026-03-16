# Error: MSIX Project Configuration & Build Tools Issues

**Keywords:** EnableMsixTooling, Microsoft.Windows.SDK.BuildTools.MSIX, AppxOSMinVersionReplaceManifestVersion, AppxOSMaxVersionTestedReplaceManifestVersion, mspdbcmf.exe, wapproj, single-project MSIX, NuGet, Visual Studio, unpackaged, resources.pri

**Error Examples:**
```
error: "mspdbcmf.exe" hard-coded path dependency into Visual Studio installation
error: MinVersion and MaxVersionTested always replaced in package.appxmanifest
Application crashes due to missing resources.pri after publish
```

---

## Quick Match

**You're seeing this if:**
- Building MSIX packages without Visual Studio installed fails due to missing `mspdbcmf.exe`
- Removing `EnableMsixTooling` from unpackaged app projects causes publish failures or missing `.pri` files
- `AppxOSMinVersionReplaceManifestVersion=false` is ignored by single-project MSIX
- Need to include multiple executables in a single MSIX package

→ Check scenarios below for your specific cause

---

## Related Issues

- [#6197](https://github.com/microsoft/WindowsAppSDK/issues/6197) - MSIX NuGet package requires Visual Studio for mspdbcmf.exe (Status: Fixed internally)
- [#3718](https://github.com/microsoft/WindowsAppSDK/issues/3718) - Removing EnableMsixTooling breaks published unpackaged apps (Status: Open)
- [#5598](https://github.com/microsoft/WindowsAppSDK/issues/5598) - BuildTools.MSIX does not support AppxOSMinVersionReplaceManifestVersion (Status: Open)
- [#5586](https://github.com/microsoft/WindowsAppSDK/issues/5586) - Single-project packaging lacks multi-executable support (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: Cannot Build MSIX Without Visual Studio — mspdbcmf.exe Hard-Coded Path

**Cause:** The `Microsoft.Windows.SDK.BuildTools.MSIX` NuGet package targets contain a hard-coded path dependency on a Visual Studio installation to locate `mspdbcmf.exe`. This prevents MSIX package creation on machines without Visual Studio (e.g., when using JetBrains Rider or CI/CD environments with only the .NET SDK).
> Source: @wjk in [#6197](https://github.com/microsoft/WindowsAppSDK/issues/6197)

**Status:** Marked as "Fixed internally" by the Windows App SDK team. The fix has not yet been released as of the issue's last update.

**Workaround (while waiting for fix):**
1. If you only need to bypass symbol generation, set:
   ```xml
   <PropertyGroup>
     <MsPdbCmfExeFullpath>None</MsPdbCmfExeFullpath>
     <AppxSymbolPackageEnabled>false</AppxSymbolPackageEnabled>
   </PropertyGroup>
   ```
2. If you need `mspdbcmf.exe`, install the Visual Studio Build Tools workload (lighter than full VS):
   ```
   vs_buildtools.exe --add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools
   ```

> ⚠️ Ideally the MSIX NuGet package would bundle `mspdbcmf.exe` or reference it from `Microsoft.Windows.SDK.BuildTools`. Neither option is currently available.

**Verify:** `dotnet publish` completes without errors referencing Visual Studio paths.

---

### Scenario 2: Removing EnableMsixTooling Breaks Published Unpackaged Apps (Missing resources.pri)

**Cause:** `EnableMsixTooling` controls the renaming of `[ProjectName].pri` to `resources.pri` during the build process. When omitted from an unpackaged app project (`<WindowsPackageType>None</WindowsPackageType>`), the `.pri` file is not copied to the publish directory, causing the published app to crash at runtime with a missing resource error.
> Source: @Balkoth in [#3718](https://github.com/microsoft/WindowsAppSDK/issues/3718)

**Fix:**
1. **Option A:** Keep `EnableMsixTooling` in the project file even for unpackaged apps:
   ```xml
   <PropertyGroup>
     <WindowsPackageType>None</WindowsPackageType>
     <EnableMsixTooling>true</EnableMsixTooling>
   </PropertyGroup>
   ```
2. **Option B:** Manually copy the `.pri` file as a post-build step:
   ```xml
   <Target Name="CopyPriFile" AfterTargets="Publish">
     <Copy SourceFiles="$(OutDir)$(AssemblyName).pri"
           DestinationFiles="$(PublishDir)resources.pri"
           SkipUnchangedFiles="true" />
   </Target>
   ```

> ✅ Confirmed by: @Balkoth in [#3718](https://github.com/microsoft/WindowsAppSDK/issues/3718) — manually copying the `.pri` file resolves the crash.

**Important note on WinAppSDK 1.8+:** @DarranRowe identified in [#5746](https://github.com/microsoft/WindowsAppSDK/issues/5746) that in WinAppSDK 1.8, the behavior changed — unpackaged projects now correctly use `[AppName].pri` instead of `resources.pri`. If upgrading to 1.8, update any hard-coded references to `resources.pri` in your code.

> @Psychlist1972 in [#3718](https://github.com/microsoft/WindowsAppSDK/issues/3718) noted that without `EnableMsixTooling`, `dotnet` command-line builds fail to generate the `.pri` file at all, breaking automated build processes.

**Verify:** Published output directory contains the `.pri` file, and the application launches without resource-related crashes.

---

### Scenario 3: AppxOSMinVersionReplaceManifestVersion Not Supported by Single-Project MSIX

**Cause:** The `UpdateTargetDeviceFamily` task in `WinAppSdkGenerateAppxManifest.cs` (part of `Microsoft.Windows.SDK.BuildTools.MSIX`) does not support the `AppxOSMinVersionReplaceManifestVersion` and `AppxOSMaxVersionTestedReplaceManifestVersion` override properties. It always replaces `MinVersion` and `MaxVersionTested` in the generated `package.appxmanifest` with values from `TargetPlatformMinVersion` and `TargetPlatformVersion`, even when the override properties are set to `false`.
> Source: @Scottj1s in [#5598](https://github.com/microsoft/WindowsAppSDK/issues/5598) — internal tracking issue created at task.ms/58588581

**Context:** For UWP and wapproj-based projects, these override properties work correctly. The gap is only in the single-project MSIX tooling provided by the BuildTools.MSIX NuGet package.

**Workaround:**
1. Use a post-build step to patch the generated `package.appxmanifest` with the desired values:
   ```xml
   <Target Name="PatchAppxManifestVersions" AfterTargets="WinAppSdkGenerateAppxManifest">
     <!-- Use XmlPoke or a custom script to set MinVersion/MaxVersionTested -->
   </Target>
   ```
2. Alternatively, set `TargetPlatformMinVersion` and `TargetPlatformVersion` to match your desired manifest values (if acceptable for your project).

**Verify:** Inspect the generated `package.appxmanifest` to confirm `MinVersion` and `MaxVersionTested` values match your expectations.

---

### Scenario 4: Single-Project Packaging Does Not Support Multiple Executables

**Cause:** Single-project MSIX packaging does not support multi-headed packages (packages containing multiple executables). This is the last remaining gap versus wapproj-based solutions, preventing the complete deprecation of the WAP project template.
> Source: @Scottj1s in [#5586](https://github.com/microsoft/WindowsAppSDK/issues/5586)

**Current status:** This is a feature gap, not a bug. Microsoft is aware; @Scottj1s noted additional gaps (notably resource handling) also need to be closed for full parity. @michael-hawker in [#5586](https://github.com/microsoft/WindowsAppSDK/issues/5586) highlighted that removing the WAP template from Visual Studio would reduce confusion for new developers.

**Workaround:**
1. Continue using a Windows Application Packaging Project (wapproj) for multi-executable MSIX packages
2. Reference: [Single-project MSIX limitations](https://github.com/MicrosoftDocs/windows-dev-docs/blob/docs/hub/apps/windows-app-sdk/single-project-msix.md#limitations)

**Verify:** N/A — this is a feature request with no current workaround in single-project mode.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- For build environments without Visual Studio, consider using `Microsoft.Windows.SDK.BuildTools` NuGet alongside the MSIX package to reduce VS dependencies (from @wjk in #6197 — noted that `mspdbcmf.exe` is not included in that package either)
- For wapproj to single-project migration, consider tracking [#5586](https://github.com/microsoft/WindowsAppSDK/issues/5586) and [#6261](https://github.com/microsoft/WindowsAppSDK/issues/6261) for official guidance
- @riverar in [#3718](https://github.com/microsoft/WindowsAppSDK/issues/3718) suggested this as a candidate for inclusion in the updated MSIX tooling

---

## References

- [Single-project MSIX Packaging documentation](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/single-project-msix)
- [Single-project MSIX limitations](https://github.com/MicrosoftDocs/windows-dev-docs/blob/docs/hub/apps/windows-app-sdk/single-project-msix.md#limitations)
- [Windows AI APIs — TargetDeviceFamily guidance](https://learn.microsoft.com/en-us/windows/ai/apis/get-started?tabs=winget%2Cwinui%2Cwinui2#build-a-new-app)
- [Microsoft.Windows.SDK.BuildTools.MSIX on NuGet](https://www.nuget.org/packages/Microsoft.Windows.SDK.BuildTools.MSIX)

---

**Updated:** 2025-07-18 | **Confidence:** 0.7
**Sources:** [#6197](https://github.com/microsoft/WindowsAppSDK/issues/6197), [#3718](https://github.com/microsoft/WindowsAppSDK/issues/3718), [#5598](https://github.com/microsoft/WindowsAppSDK/issues/5598), [#5586](https://github.com/microsoft/WindowsAppSDK/issues/5586)
