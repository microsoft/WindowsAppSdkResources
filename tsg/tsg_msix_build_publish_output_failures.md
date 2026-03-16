# Error: MSIX Build/Publish Output Failures (msixupload, msixbundle, appxsym)

**Keywords:** msixupload, msixbundle, appxsym, WinAppSdkSignAppxPackageBundle, MSB4036, MSB6006, mspdbcmf.exe, CMF1106, UapAppxPackageBuildMode, StoreUpload, StoreAndSideload

**Error Examples:**
```
error MSB4036: The "WinAppSdkSignAppxPackageBundle" task was not found.
error MSB6006: "mspdbcmf.exe" have exited, fatal error CMF1106
```

---

## Quick Match

**You're seeing this if:**
- Building an MSIX bundle or upload package fails with MSB4036 or MSB6006
- `msbuild` with `UapAppxPackageBuildMode=StoreUpload` does not produce `.msixupload`
- Publishing an MSIX from Visual Studio pollutes `.csproj.user` with `UapAppxPackageBuildMode`
- `mspdbcmf.exe` exits with fatal error CMF1106 during symbol package generation

→ Check scenarios below for your specific cause

---

## Related Issues

- [#5820](https://github.com/microsoft/WindowsAppSDK/issues/5820) - Create app bundle fails with scaled images due to typo in targets (Status: Closed/Fixed)
- [#5825](https://github.com/microsoft/WindowsAppSDK/issues/5825) - Single packaging generates msixbundle but fails on appxsym (Status: Open)
- [#5501](https://github.com/microsoft/WindowsAppSDK/issues/5501) - UWP .NET 9 msbuild does not produce .msixupload file (Status: Open)
- [#5537](https://github.com/microsoft/WindowsAppSDK/issues/5537) - Publishing MSIX influences future builds via UapAppxPackageBuildMode (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: "WinAppSdkSignAppxPackageBundle" Task Not Found (MSB4036) When Creating App Bundle

**Cause:** A typo in `Microsoft.Windows.SDK.BuildTools.MSIX.Packaging.targets` references a non-existent task name `WinAppSdkSignAppxPackageBundle` instead of the correct `WinAppSdkSignAppxPackage`. This causes failures when `UapAppxPackageBuildMode` is set to `CI` or `StoreUpload` and the manifest contains scaled images or resource packages.
> Source: @zhuxb711 in [#5820](https://github.com/microsoft/WindowsAppSDK/issues/5820)

**Conditions:**
- Only occurs with `UapAppxPackageBuildMode = CI` or `StoreUpload`
- `SideloadOnly` mode works fine
- Triggered when the manifest contains scaled visual assets (e.g., `SmallTile.scale-100.png`)

**Fix:**
1. Locate the NuGet package targets file:
   ```
   %USERPROFILE%\.nuget\packages\microsoft.windows.sdk.buildtools.msix\<version>\build\Microsoft.Windows.SDK.BuildTools.MSIX.Packaging.targets
   ```
2. Find the `_CreateUploadResourcePackages` target
3. Replace `WinAppSdkSignAppxPackageBundle` with `WinAppSdkSignAppxPackage`

**CI/CD Workaround (GitHub Actions):**
```yaml
- name: Restore NuGet Packages
  shell: pwsh
  run: |
    msbuild "<Your Solution Path>" /nr:false /restore /p:RestorePackagesConfig=true /p:PreferredToolArchitecture="x64"

- name: Patch for WindowsAppSDK#5820
  shell: pwsh
  run: |
    $NuGetPackagesPath = $env:NUGET_PACKAGES ?? (Join-Path $env:USERPROFILE ".nuget/packages")
    $TargetFilePath = Join-Path $NuGetPackagesPath "microsoft.windows.sdk.buildtools.msix/<version>/build/Microsoft.Windows.SDK.BuildTools.MSIX.Packaging.targets"
    (Get-Content $TargetFilePath) -replace 'WinAppSdkSignAppxPackageBundle', 'WinAppSdkSignAppxPackage' | Set-Content $TargetFilePath
```

> ✅ Confirmed by: @zhuxb711, @MUJaCHe66 in [#5820](https://github.com/microsoft/WindowsAppSDK/issues/5820)
> @Scottj1s (Microsoft) acknowledged the bug and indicated a fix would be published in an updated `Microsoft.Windows.SDK.BuildTools.MSIX`.

**Verify:** Build with `UapAppxPackageBuildMode=StoreUpload` completes without MSB4036 errors.

---

### Scenario 2: mspdbcmf.exe Fails with Fatal Error CMF1106 During appxsym Generation

**Cause:** The `_GenerateAppxSymbolPackage` target invokes `mspdbcmf.exe` to generate `.appxsym` symbol packages, but the tool fails with exit code CMF1106 on certain PDB files. The target does not respect `AppxSymbolPackageEnabled=false`, so setting that property alone does not bypass the failure.
> Source: @zhuxb711 in [#5825](https://github.com/microsoft/WindowsAppSDK/issues/5825)

**Fix:**
1. Set `MsPdbCmfExeFullpath` to an invalid path to bypass `_GenerateAppxSymbolPackage`:
   ```xml
   <PropertyGroup>
     <MsPdbCmfExeFullpath>None</MsPdbCmfExeFullpath>
   </PropertyGroup>
   ```
2. Also pass `/p:AppxSymbolPackageEnabled="false"` to msbuild to fully disable symbol package generation:
   ```
   msbuild YourProject.csproj /p:AppxSymbolPackageEnabled="false"
   ```

> ✅ Confirmed by: @zhuxb711 in [#5825](https://github.com/microsoft/WindowsAppSDK/issues/5825)

**Note:** This is a combined workaround. `AppxSymbolPackageEnabled=false` alone is insufficient because `_GenerateAppxSymbolPackage` does not check that property (see also [#6183](https://github.com/microsoft/WindowsAppSDK/issues/6183)).

**Verify:** `msbuild` completes the msixbundle generation without CMF1106 errors.

---

### Scenario 3: msbuild with StoreUpload Does Not Produce .msixupload File (UWP .NET 9)

**Cause:** When using `msbuild` from the command line with `UapAppxPackageBuildMode=StoreUpload` for UWP .NET 9 projects, the build does not produce a `.msixupload` file. Multiple `AppxBundlePlatforms` are also not respected — only the first platform is compiled. The Visual Studio "Create App Packages" wizard works correctly, but the CLI equivalent does not.
> Source: @DilanBoskan in [#5501](https://github.com/microsoft/WindowsAppSDK/issues/5501)

**Workaround:**
1. Build each platform separately using `msbuild`
2. Bundle and sign manually using `MakeAppx.exe` and `SignTool.exe`:
   ```powershell
   # Build each platform
   MSBuild.exe YourApp.csproj /p:Configuration=Release /p:Platform=x86
   MSBuild.exe YourApp.csproj /p:Configuration=Release /p:Platform=x64
   MSBuild.exe YourApp.csproj /p:Configuration=Release /p:Platform=ARM64

   # Create bundle manually
   MakeAppx.exe bundle /d <output-directory> /p YourApp.msixbundle

   # Sign the bundle
   SignTool.exe sign /fd SHA256 /a /f YourCert.pfx /p <password> YourApp.msixbundle
   ```

> Source: @DilanBoskan in [#5501](https://github.com/microsoft/WindowsAppSDK/issues/5501)

**Note:** @0x5bfa confirmed that in WinAppSDK (non-UWP), the latest `Microsoft.Windows.SDK.BuildTools.MSIX` does produce msixupload and msixbundle. UWP .NET 9 may still be affected.

**Verify:** Check that `.msixupload` file is generated in the output directory.

---

### Scenario 4: Publishing MSIX Pollutes .csproj.user and Breaks Future Builds

**Cause:** After publishing an MSIX from Visual Studio (1.8+), the property `<UapAppxPackageBuildMode>StoreAndSideload</UapAppxPackageBuildMode>` is written to `.csproj.user`. This causes every subsequent Release build to unnecessarily create an MSIX package, and publishing as an unpackaged app fails.
> Source: @lhak in [#5537](https://github.com/microsoft/WindowsAppSDK/issues/5537)

**Fix:**
1. After publishing, delete or edit the `.csproj.user` file to remove:
   ```xml
   <UapAppxPackageBuildMode>StoreAndSideload</UapAppxPackageBuildMode>
   ```
2. Alternatively, set `UapAppxPackageBuildMode` to `SideloadOnly` to avoid the unwanted behavior:
   ```xml
   <UapAppxPackageBuildMode>SideloadOnly</UapAppxPackageBuildMode>
   ```

> Source: @Scottj1s (Microsoft) in [#5537](https://github.com/microsoft/WindowsAppSDK/issues/5537) confirmed that `StoreAndSideload` always publishes on build, and `SideloadOnly` can be used to opt out.

> @lhak confirmed this was not an issue in previous WinAppSDK versions, suggesting a regression in 1.8.

**Verify:** Build in Release mode without `.csproj.user` containing `UapAppxPackageBuildMode` — no MSIX package should be created unless explicitly requested.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- For #5501: Use the Visual Studio "Create App Packages" wizard as the only reliable method for generating `.msixupload` with multi-platform UWP .NET 9 projects (from @DilanBoskan in #5501)
- Add `.csproj.user` to `.gitignore` to prevent `UapAppxPackageBuildMode` pollution from propagating across team members (general recommendation related to #5537)

---

## References

- [Single-project MSIX Packaging documentation](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/single-project-msix)
- [MakeAppx.exe documentation](https://learn.microsoft.com/en-us/windows/msix/package/create-app-package-with-makeappx-tool)
- [Visual Studio Developer Community: mspdbcmf.exe failed](https://developercommunity.visualstudio.com/t/mspdbcmfexe-failed-with-exit-code-1106/11037870)

---

**Updated:** 2025-07-18 | **Confidence:** 0.8
**Sources:** [#5820](https://github.com/microsoft/WindowsAppSDK/issues/5820), [#5825](https://github.com/microsoft/WindowsAppSDK/issues/5825), [#5501](https://github.com/microsoft/WindowsAppSDK/issues/5501), [#5537](https://github.com/microsoft/WindowsAppSDK/issues/5537)
