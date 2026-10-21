# Error: "Microsoft.Windows.SDK.BuildTools.MSIX: Clarification needed on support and roadmap for non-UWP projects"

**Keywords:** Microsoft.Windows.SDK.BuildTools.MSIX, .wapproj, non-UWP, WPF, WinForms, CI/CD, Publish profiles, .msixbundle, capabilities configuration

**Error Example:**
```
Is Microsoft.Windows.SDK.BuildTools.MSIX officially intended as a replacement for .wapproj in non-UWP scenarios (WPF, WinForms, console apps)? Or is it complementary/optional?

What is the migration story? For projects currently using a Windows Application Packaging Project, what does moving to this NuGet look like? Is there a guide, or is one planned?

Does this package support:
- Packaging for CI/CD pipelines without Visual Studio? (There seems to be a reliance on UAP workload?)
- Publish profiles?
- Generating a .msixbundle?
- Entitlements / capabilities configuration (currently in Package.appxmanifest within .wapproj)?

Where are the docs? Neither docs.microsoft.com nor the NuGet page contains any meaningful usage documentation. If docs are in progress, is there a public tracking issue?
```

---

## Quick Match

**You're seeing this if:**
- You're using `Microsoft.Windows.SDK.BuildTools.MSIX` in a non-UWP project (e.g., WPF, WinForms, console apps).
- You are migrating from `.wapproj` (Windows Application Packaging Project) to the `Microsoft.Windows.SDK.BuildTools.MSIX` NuGet package.
- You encounter issues with CI/CD pipelines, publish profiles, or .msixbundle generation.
- You are looking for official documentation or guidance on using `Microsoft.Windows.SDK.BuildTools.MSIX`.

→ Check scenarios below for your specific cause.

---

## Related Issues

- [#6261](https://github.com/microsoft/WindowsAppSDK/issues/6261) - Microsoft.Windows.SDK.BuildTools.MSIX: Clarification needed on support and roadmap for non-UWP projects (Status: Open)
- [#5060](https://github.com/microsoft/WindowsAppSDK/issues/5060) - Foundational questions about Microsoft.Windows.SDK.BuildTools.MSIX (Status: Closed)
- [#6197](https://github.com/microsoft/WindowsAppSDK/issues/6197) - CI/CD pipeline support for Microsoft.Windows.SDK.BuildTools.MSIX (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: Migrating from `.wapproj` to `Microsoft.Windows.SDK.BuildTools.MSIX`

**Cause:** Developers using `.wapproj` for non-UWP projects (e.g., WPF, WinForms, console apps) are unclear about the migration process to the `Microsoft.Windows.SDK.BuildTools.MSIX` NuGet package.
> Source: @wjk in [#6261](https://github.com/microsoft/WindowsAppSDK/issues/6261)

**Fix:**
1. Add the `Microsoft.Windows.SDK.BuildTools` NuGet package to your project.
2. Include the following property in your `.csproj` file:
   ```xml
   <UseSdkBuildToolsPackage>true</UseSdkBuildToolsPackage>
   ```
3. If you previously used a publish profile, migrate its settings directly into your `.csproj` file. For example:
   ```xml
   <PropertyGroup>
       <GenerateAppInstallerFile>True</GenerateAppInstallerFile>
       <AppInstallerUpdateFrequency>1</AppInstallerUpdateFrequency>
       <AppInstallerCheckForUpdateFrequency>OnApplicationRun</AppInstallerCheckForUpdateFrequency>
   </PropertyGroup>
   ```

> ✅ Confirmed by: @wjk in [#6261](https://github.com/microsoft/WindowsAppSDK/issues/6261)

**Verify:** Build and package your application using the updated `.csproj` file. Ensure the output includes the expected `.msix` or `.msixbundle`.

---

### Scenario 2: CI/CD Pipelines Without Visual Studio

**Cause:** The `Microsoft.Windows.SDK.BuildTools.MSIX` package can be used in CI/CD pipelines without requiring Visual Studio, but certain configurations are necessary.
> Source: @wjk in [#6261](https://github.com/microsoft/WindowsAppSDK/issues/6261)

**Fix:**
1. Ensure your project does not include fastlink C++ PDBs, as they are not supported.
2. Add the `Microsoft.Windows.SDK.BuildTools` NuGet package to your project.
3. Include the following property in your `.csproj` file:
   ```xml
   <UseSdkBuildToolsPackage>true</UseSdkBuildToolsPackage>
   ```

> ✅ Confirmed by: @wjk in [#6261](https://github.com/microsoft/WindowsAppSDK/issues/6261)

**Verify:** Run your CI/CD pipeline and confirm that the `.msix` or `.msixbundle` is generated successfully without requiring Visual Studio.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- None provided in the issue comments.

---

## References

- [Issue #6261](https://github.com/microsoft/WindowsAppSDK/issues/6261)
- [Issue #5060](https://github.com/microsoft/WindowsAppSDK/issues/5060)
- [Issue #6197](https://github.com/microsoft/WindowsAppSDK/issues/6197)
- [Microsoft.Windows.SDK.BuildTools NuGet Package](https://www.nuget.org/packages/Microsoft.Windows.SDK.BuildTools)

---

**Updated:** 2026-05-04 | **Confidence:** 0.8
**Sources:** [#6261](https://github.com/microsoft/WindowsAppSDK/issues/6261), [#5060](https://github.com/microsoft/WindowsAppSDK/issues/5060), [#6197](https://github.com/microsoft/WindowsAppSDK/issues/6197)