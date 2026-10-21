# Error: Self-Contained Deployment and MSIX Packaging Conflicts

**Keywords:** WindowsAppSDKSelfContained, XamlParseException, COMException, 0x80070490, resources.pri, PRI, _OverrideGetPriIndexName, EnableMsixTooling, WMC1012, ApplicationXaml, codegen, library project, PublishTrimmed, onnxruntime.dll, DirectML.dll, PublishSingleFile, WinML, MSB8027, LNK4042, WindowsAppRuntimeAutoInitializer.cpp

**Error Examples:**
```
Microsoft.UI.Xaml.Markup.XamlParseException: 'XAML parsing failed.'
System.Runtime.InteropServices.COMException (0x80070490): Element not found.
NETSDK1022: Duplicate 'PRIResource' items were included.
WMC1012: A project cannot have more than one ApplicationXaml item
onnxruntime.dll and DirectML.dll unnecessarily included in output folder
warning MSB8027: Two or more files with the name of WindowsAppRuntimeAutoInitializer.cpp will produce outputs to the same location.
WindowsAppRuntimeAutoInitializer.obj : warning LNK4042: object specified more than once; extras ignored
Microsoft.Common.CurrentVersion.targets(2434,5): warning MSB3106: Assembly strong name is either a path which could not be found or it is a full assembly name which is badly formed.
```

---

## Quick Match

**You're seeing this if:**
- Setting `WindowsAppSDKSelfContained=true` on a library project causes XAML parsing or codegen failures
- Upgrading to WinAppSDK 1.8 causes COMException 0x80070490 (Element not found) related to `.pri` file naming changes
- Build errors include `NETSDK1022` (duplicate items) or `WMC1012` (multiple ApplicationXaml) after upgrading
- Publishing a self-contained app includes large, unused native dependencies like `onnxruntime.dll` and `DirectML.dll`
- Encountering warnings like `MSB8027` or `LNK4042` related to `WindowsAppRuntimeAutoInitializer.cpp` in solutions with multiple C++ projects
- Seeing `MSB3106` warnings about assembly strong names when using newer versions of WindowsAppSDK

→ Check scenarios below for your specific cause

---

## Related Issues

- [#6091](https://github.com/microsoft/WindowsAppSDK/issues/6091) - Self-contained deployment breaks WinUI codegen in libraries (Status: Closed/Fixed in v2.0-preview1)
- [#5746](https://github.com/microsoft/WindowsAppSDK/issues/5746) - App update to 1.8-preview1 with COMException 0x80070490 errors (Status: Open)
- [#5969](https://github.com/microsoft/WindowsAppSDK/issues/5969) - Failed to get rid of ML libraries in app build (Status: Closed/Fixed in 1.8.4)
- [#6015](https://github.com/microsoft/WindowsAppSDK/issues/6015) - Huge native DLLs not trimmed in self-contained mode (Status: Closed)
- [#5395](https://github.com/microsoft/WindowsAppSDK/issues/5395) - MSB8027 and LNK4042 warnings with WindowsAppRuntimeAutoInitializer.cpp (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: WindowsAppSDKSelfContained=true on Library Project Breaks XAML Codegen

**Cause:** When `WindowsAppSDKSelfContained` is set to `true` in a class library project, the `_OverrideGetPriIndexName` target in `Microsoft.WindowsAppSDK.SelfContained.targets` (part of `Microsoft.WindowsAppSDK.Base` NuGet) sets the PRI root to empty. This is intended for app projects, not libraries, and causes WinUI XAML code generation to fail. The result is a `XamlParseException` at runtime or a silent launch failure.
> Source: @dongle-the-gadget in [#6091](https://github.com/microsoft/WindowsAppSDK/issues/6091)

**Affected versions:** WinAppSDK 1.8.x (confirmed with 1.8.3 / 1.8.251106002)

**Fix:**
1. **Remove** `WindowsAppSDKSelfContained` from all library/class library project files. Only set it on the main application (executable) project:
   ```xml
   <!-- ❌ Do NOT set in library .csproj -->
   <!-- <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained> -->

   <!-- ✅ Only set in app/exe .csproj -->
   <PropertyGroup>
     <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
   </PropertyGroup>
   ```
2. If setting the property globally via command line (`dotnet msbuild /p:WindowsAppSDKSelfContained=true`), this will apply to all projects in the solution including libraries — avoid this pattern.

> ✅ Confirmed by: @riverar (CONTRIBUTOR) in [#6091](https://github.com/microsoft/WindowsAppSDK/issues/6091) — removing the property from the library project resolves the issue.

**Official fix:** This has been fixed in [Windows App SDK 2.0.0-Preview1](https://github.com/microsoft/WindowsAppSDK/releases/tag/v2.0-preview1) by adding explicit validation that prevents `WindowsAppSDKSelfContained` from being applied to class library projects.
> Source: @ssparach (CONTRIBUTOR) in [#6091](https://github.com/microsoft/WindowsAppSDK/issues/6091)

**Verify:** Clean the solution, rebuild, and run the app — XAML should parse without exceptions.

---

### Scenario 2: COMException 0x80070490 "Element not found" After Upgrading to 1.8

**Cause:** In WinAppSDK 1.8, the `.pri` file naming behavior changed for unpackaged projects. Previously (1.7 and earlier), `EnableMsixTooling=true` caused the `.pri` file to be named `resources.pri` even for unpackaged apps. In 1.8, unpackaged projects correctly use `[AppName].pri` instead. Code that hard-codes `resources.pri` will fail with COMException 0x80070490 ("Element not found") when accessing localized resources.
> Source: @Sella-GH in [#5746](https://github.com/microsoft/WindowsAppSDK/issues/5746), root cause analysis by @DarranRowe

**Fix for COMException 0x80070490:**
1. Update any code that hard-codes `resources.pri` to use the correct file name. Use the Resource Manager API to determine the default path:
   ```csharp
   var resourceLoader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
   ```
2. If you have a custom resource loading service, update the file name from `resources.pri` to `[YourAppName].pri`.

> ✅ Confirmed by: @Sella-GH in [#5746](https://github.com/microsoft/WindowsAppSDK/issues/5746)

---

### Scenario 3: Large Native DLLs (onnxruntime.dll, DirectML.dll) Included in Self-Contained Builds

**Cause:** When using `WindowsAppSDKSelfContained=true` with the default metapackage (`Microsoft.WindowsAppSDK`), all component packages are included, even if they are not used. This includes large native dependencies like `onnxruntime.dll` and `DirectML.dll` (used for ML scenarios), which unnecessarily increase the app size.
> Source: @manodasanW in [#5969](https://github.com/microsoft/WindowsAppSDK/issues/5969)

**Fix:**
1. Replace the metapackage (`Microsoft.WindowsAppSDK`) with only the component packages you need. For example:
   ```xml
   <PackageReference Include="Microsoft.WindowsAppSDK.WinUI" Version="1.8.251106002" />
   ```
2. Remove the `Microsoft.WindowsAppSDK.Runtime` package unless explicitly required for your scenario.

**Additional Fix for PublishSingleFile Issue:** Fixed in [1.8.4](https://github.com/microsoft/WindowsAppSDK/releases/tag/v1.8.4).
> Source: @manodasanW in [#5969](https://github.com/microsoft/WindowsAppSDK/issues/5969)

**Verify:** Check the output folder for unnecessary DLLs:
```powershell
Get-ChildItem -Path "<YourOutputDir>" -Filter "*.dll"
```

---

## ⚠️ Known Issues / Unverified Suggestions

### Issue: MSB8027 and LNK4042 Warnings with WindowsAppRuntimeAutoInitializer.cpp

**Cause:** When using multiple C++ projects in a solution, both referencing the same version of WindowsAppSDK, the `WindowsAppRuntimeAutoInitializer.cpp` file from the `buildTransitive` directory is included multiple times, causing duplicate object file warnings (`MSB8027`, `LNK4042`).
> Source: @PetrMinar in [#5395](https://github.com/microsoft/WindowsAppSDK/issues/5395)

**Unverified Suggestions:**
- Modify the `ClCompile` item in `WindowsAppSDK-Nuget-Native.AutoInitializer.targets` to avoid duplicate inclusions. Example:
  ```xml
  <ClCompile Include="$(MSBuildThisFileDirectory)..\..\include\WindowsAppRuntimeAutoInitializer.cpp">
    <PrecompiledHeader>NotUsing</PrecompiledHeader>
    <PreprocessorDefinitions Condition="'$(WindowsAppSdkBootstrapInitialize)'=='true'">MICROSOFT_WINDOWSAPPSDK_AUTOINITIALIZE_BOOTSTRAP;%(PreprocessorDefinitions)</PreprocessorDefinitions>
  </ClCompile>
  ```
> Source: @mschofie in [#5395](https://github.com/microsoft/WindowsAppSDK/issues/5395)

---

## References

- [Self-contained deployment documentation](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/deploy-self-contained)
- [WinAppSDK 2.0.0-Preview1 release notes](https://github.com/microsoft/WindowsAppSDK/releases/tag/v2.0-preview1)
- [Resource management with MRT Core](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/mrtcore/mrtcore-overview)

---

**Updated:** 2026-03-17 | **Confidence:** 0.90
**Sources:** [#6091](https://github.com/microsoft/WindowsAppSDK/issues/6091), [#5746](https://github.com/microsoft/WindowsAppSDK/issues/5746), [#5969](https://github.com/microsoft/WindowsAppSDK/issues/5969), [#6015](https://github.com/microsoft/WindowsAppSDK/issues/6015), [#5395](https://github.com/microsoft/WindowsAppSDK/issues/5395)