# Error: MSIX Project Configuration & Build Tools Issues

**Keywords:** EnableMsixTooling, Microsoft.Windows.SDK.BuildTools.MSIX, AppxOSMinVersionReplaceManifestVersion, AppxOSMaxVersionTestedReplaceManifestVersion, mspdbcmf.exe, wapproj, single-project MSIX, NuGet, Visual Studio, unpackaged, resources.pri, DebugType, APPX1101, APPXUPLOAD, buildTransitive, CustomBeforeMicrosoftCommonTargets, dotnet msbuild, symbols package, PublishError, MetadataNotFound, UapAppxPackageBuildMode, RuntimeIdentifier, AppxBundlePlatforms, AppxPackageName, AppxBundleNameForOutput

**Error Examples:**
```
error: "mspdbcmf.exe" hard-coded path dependency into Visual Studio installation
error: MinVersion and MaxVersionTested always replaced in package.appxmanifest
Application crashes due to missing resources.pri after publish
error: APPX1101 - Payload contains two or more files with the same destination path
error: Missing APPXUPLOAD file for store submission
error: CustomBeforeMicrosoftCommonTargets reassignment breaks MSIX build tools
warning: Path to `mspdbcmf.exe` could not be found. A symbols package will not be generated
error: Metadata file 'Test.WinUI.dll' could not be found during Publish
error: APPX1101 - Payload contains duplicate 'resources.pri' files in WinUI integration tests
error: NETSDK1032 - RuntimeIdentifier conflicts during MSIX bundle recursive builds
error: Setting AppxPackageName breaks msixbundle generation
```

---

## Quick Match

**You're seeing this if:**
- Building MSIX packages without Visual Studio installed fails due to missing `mspdbcmf.exe`
- Removing `EnableMsixTooling` from unpackaged app projects causes publish failures or missing `.pri` files
- `AppxOSMinVersionReplaceManifestVersion=false` is ignored by single-project MSIX
- Need to include multiple executables in a single MSIX package
- Encountering APPX1101 errors due to duplicate files in the payload
- Unable to generate `.appxupload` files for store submission
- MSIX build tools fail due to `CustomBeforeMicrosoftCommonTargets` reassignment
- Building MSIX packages using `dotnet msbuild` or `dotnet publish` fails to generate symbols package due to missing `mspdbcmf.exe`
- Publishing unpackaged apps fails with "Metadata file 'Test.WinUI.dll' could not be found"
- Publishing an MSIX package influences future builds due to `.csproj.user` pollution
- Encountering APPX1101 errors with duplicate `resources.pri` files in WinUI integration tests
- MSIX bundle recursive builds fail due to `RuntimeIdentifier` conflicts
- Setting `AppxPackageName` breaks MSIX bundle generation

→ Check scenarios below for your specific cause

---

## Related Issues

- [#6197](https://github.com/microsoft/WindowsAppSDK/issues/6197) - MSIX NuGet package requires Visual Studio for mspdbcmf.exe (Status: Fixed internally)
- [#3718](https://github.com/microsoft/WindowsAppSDK/issues/3718) - Removing EnableMsixTooling breaks published unpackaged apps (Status: Open)
- [#5598](https://github.com/microsoft/WindowsAppSDK/issues/5598) - BuildTools.MSIX does not support AppxOSMinVersionReplaceManifestVersion (Status: Open)
- [#5586](https://github.com/microsoft/WindowsAppSDK/issues/5586) - Single-project packaging lacks multi-executable support (Status: Open)
- [#5262](https://github.com/microsoft/WindowsAppSDK/issues/5262) - DebugType=embedded generates misleading mspdbcmf.exe error (Status: Closed)
- [#5675](https://github.com/microsoft/WindowsAppSDK/issues/5675) - APPXUPLOAD-bundle creation unsupported pre-1.8 (Status: Closed)
- [#5626](https://github.com/microsoft/WindowsAppSDK/issues/5626) - Missing buildTransitive folder in BuildTools.MSIX (Status: Closed)
- [#5811](https://github.com/microsoft/WindowsAppSDK/issues/5811) - CustomBeforeMicrosoftCommonTargets breaks MSIX build tools (Status: Open)
- [#5826](https://github.com/microsoft/WindowsAppSDK/issues/5826) - APPX1101 errors after upgrading to WindowsAppSDK 1.8 (Status: Open)
- [#5102](https://github.com/microsoft/WindowsAppSDK/issues/5102) - Missing symbols package when building in dotnet msbuild workflow (Status: Open)
- [#3065](https://github.com/microsoft/WindowsAppSDK/issues/3065) - Publish error: Metadata file 'Test.WinUI.dll' not found (Status: Open)
- [#5537](https://github.com/microsoft/WindowsAppSDK/issues/5537) - Publishing MSIX package influences future builds (Status: Open)
- [#5845](https://github.com/microsoft/WindowsAppSDK/issues/5845) - APPX1101 error with duplicate resources.pri in WinUI integration tests (Status: Closed)
- [#6322](https://github.com/microsoft/WindowsAppSDK/issues/6322) - MSIX bundle recursive build does not propagate RuntimeIdentifier (Status: Closed)
- [#6508](https://github.com/microsoft/WindowsAppSDK/issues/6508) - Setting AppxPackageName breaks msixbundle generation (Status: Closed)

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

**Verify:** Published output directory contains the `.pri` file, and the application launches without resource-related crashes.

---

### Scenario 3: Publish Error — Metadata File 'Test.WinUI.dll' Not Found

**Cause:** Publishing fails when the reference library project contains the `Microsoft.WindowsAppSDK` package. This occurs due to incorrect project configuration or MSIX packaging being enabled for an unpackaged app.
> Source: @Scottj1s in [#3065](https://github.com/microsoft/WindowsAppSDK/issues/3065)

**Fix:**
1. Disable MSIX packaging for unpackaged apps by adding the following to the `.csproj` file:
   ```xml
   <PropertyGroup>
     <WindowsPackageType>None</WindowsPackageType>
   </PropertyGroup>
   ```
2. Ensure the correct "Publish" menu option is selected in Visual Studio. For unpackaged apps, use the "Publish" menu item, not "Package and Publish."

**Verify:** Publish completes without errors, and the application runs successfully.

---

### Scenario 4: Publishing MSIX Package Influences Future Builds

**Cause:** Publishing an MSIX package from Visual Studio adds the `UapAppxPackageBuildMode` property to the `.csproj.user` file, causing MSIX packages to be created for every subsequent build in Release mode. This also prevents publishing unpackaged apps.
> Source: @Scottj1s in [#5537](https://github.com/microsoft/WindowsAppSDK/issues/5537)

**Workaround:**
1. Delete the `.csproj.user` file or remove the `<UapAppxPackageBuildMode>` property after publishing.
2. Alternatively, set the property to `SideloadOnly` to prevent automatic package creation:
   ```xml
   <PropertyGroup>
     <UapAppxPackageBuildMode>SideloadOnly</UapAppxPackageBuildMode>
   </PropertyGroup>
   ```

**Verify:** Subsequent builds do not create MSIX packages unless explicitly requested.

---

### Scenario 5: APPX1101 Errors Due to Duplicate Files in Payload

**Cause:** Duplicate files in the MSIX package payload, often caused by conflicting versions of dependencies or incorrect project configurations.
> Source: @manodasanW in [#5826](https://github.com/microsoft/WindowsAppSDK/issues/5826)

**Fix:** Ensure all projects in the solution use consistent versions of dependencies. Remove any outdated `WindowsSdkPackageVersion` properties from project files.

**Verify:** Build completes without APPX1101 errors.

---

### Scenario 6: APPX1101 Error with Duplicate 'resources.pri' Files in WinUI Integration Tests

**Cause:** When a WinUI integration test references a WinUI app, upgrading to .NET 9 can cause duplicate `resources.pri` files in the MSIX package payload.
> Source: @manodasanW in [#5845](https://github.com/microsoft/WindowsAppSDK/issues/5845)

**Fix:** Update to `Microsoft.Windows.SDK.BuildTools.MSIX` version `1.7.251221100` or later, which resolves this issue.

**Verify:** Build completes without APPX1101 errors related to `resources.pri`.

---

### Scenario 7: MSIX Bundle Recursive Build Fails Due to RuntimeIdentifier Conflicts

**Cause:** When creating an MSIX bundle with `GenerateAppxPackageOnBuild=true` and `AppxBundle=Always`, the recursive build process does not propagate the correct `RuntimeIdentifier` when switching platforms, causing `NETSDK1032` errors.
> Source: @guimafelipe in [#6322](https://github.com/microsoft/WindowsAppSDK/issues/6322)

**Fix:** Update to `Microsoft.Windows.SDK.BuildTools.MSIX` version `1.7.260425100` or later.

**Verify:** MSIX bundle builds successfully without `NETSDK1032` errors.

---

### Scenario 8: Setting AppxPackageName Breaks MSIX Bundle Generation

**Cause:** Setting `AppxPackageName` in the project file causes all MSIX files in the bundle to have the same name, leading to overwriting and incomplete bundles.
> Source: @guimafelipe in [#6508](https://github.com/microsoft/WindowsAppSDK/issues/6508)

**Fix:** Update to `Microsoft.Windows.SDK.BuildTools.MSIX` version `1.7.260610101` or later.

**Workaround (if update is not possible):**
1. Use `AppxBundleNameForOutput` instead of `AppxPackageName` to change the bundle name without affecting individual MSIX file names.

**Verify:** MSIX bundle contains all expected packages for multiple platforms.

---

## Known Issues

### Issue: Missing Symbols Package When Building in dotnet msbuild Workflow

**Cause:** When using `dotnet msbuild` or `dotnet publish` to build an MSIX package, the symbol package creation fails with a warning about `mspdbcmf.exe` not being found. This occurs because the MSIX build tools rely on Visual Studio-specific paths that are not available in CLI-only workflows.
> Source: @riverar in [#5102](https://github.com/microsoft/WindowsAppSDK/issues/5102)

**Status:** Open. No confirmed solution yet.

**Workaround:** None verified. Some users suggest ensuring all prerequisites for Windows App SDK development are installed, but this has not been confirmed to resolve the issue.

---

## ⚠️ Unverified / Community Suggestions

- For build environments without Visual Studio, consider using `Microsoft.Windows.SDK.BuildTools` NuGet alongside the MSIX package to reduce VS dependencies (from @wjk in #6197).
- For wapproj to single-project migration, consider tracking [#5586](https://github.com/microsoft/WindowsAppSDK/issues/5586) and [#6261](https://github.com/microsoft/WindowsAppSDK/issues/6261) for official guidance.

---

## References

- [Single-project MSIX Packaging documentation](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/single-project-msix)
- [Single-project MSIX limitations](https://github.com/MicrosoftDocs/windows-dev-docs/blob/docs/hub/apps/windows-app-sdk/single-project-msix.md#limitations)
- [Microsoft.Windows.SDK.BuildTools.MSIX on NuGet](https://www.nuget.org/packages/Microsoft.Windows.SDK.BuildTools.MSIX)

---

**Updated:** 2026-06-15 | **Confidence:** 0.8
**Sources:** [#6197](https://github.com/microsoft/WindowsAppSDK/issues/6197), [#3718](https://github.com/microsoft/WindowsAppSDK/issues/3718), [#5598](https://github.com/microsoft/WindowsAppSDK/issues/5598), [#5586](https://github.com/microsoft/WindowsAppSDK/issues/5586), [#5262](https://github.com/microsoft/WindowsAppSDK/issues/5262), [#5675](https://github.com/microsoft/WindowsAppSDK/issues/5675), [#5626](https://github.com/microsoft/WindowsAppSDK/issues/5626), [#5811](https://github.com/microsoft/WindowsAppSDK/issues/5811), [#5826](https://github.com/microsoft/WindowsAppSDK/issues/5826), [#5102](https://github.com/microsoft/WindowsAppSDK/issues/5102), [#3065](https://github.com/microsoft/WindowsAppSDK/issues/3065), [#5537](https://github.com/microsoft/WindowsAppSDK/issues/5537), [#5845](https://github.com/microsoft/WindowsAppSDK/issues/5845), [#6322](https://github.com/microsoft/WindowsAppSDK/issues/6322), [#6508](https://github.com/microsoft/WindowsAppSDK/issues/6508)