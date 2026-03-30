# Overview and Usage of Microsoft.Windows.SDK.BuildTools.MSIX NuGet Package

**Keywords:** Microsoft.Windows.SDK.BuildTools.MSIX, MSIX packaging, single-project MSIX, MSIX bundles, .winmd harvesting, .appxmanifest rewriting

**Error Example:**
```
No specific error message provided. This guide addresses questions and known issues related to the Microsoft.Windows.SDK.BuildTools.MSIX NuGet package.
```

---

## Quick Match

**You're seeing this if:**
- You're using the `Microsoft.Windows.SDK.BuildTools.MSIX` NuGet package for MSIX packaging.
- You have questions about its purpose, usage, or compatibility.
- You encounter issues with MSIX bundles or `.msixupload` generation.
- Platform: Windows

→ Check scenarios below for your specific cause

---

## Related Issues

- [#5060](https://github.com/microsoft/WindowsAppSDK/issues/5060) - What is Microsoft.Windows.SDK.BuildTools.MSIX? (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: Understanding the Purpose of Microsoft.Windows.SDK.BuildTools.MSIX

**Cause:** Developers are unclear about the purpose and scope of the `Microsoft.Windows.SDK.BuildTools.MSIX` NuGet package.
> Source: @Sergio0694 in [#5060](https://github.com/microsoft/WindowsAppSDK/issues/5060)

**Details:**
- The package contains all the single-project MSIX tooling that Windows App SDK includes (e.g., MSIX packaging, MRT Core, etc.).
- It was decoupled from Windows App SDK to make it easier to service and to allow UWP .NET 9 projects to leverage it.
- All new improvements for single-project MSIX tooling (e.g., MSIX bundle support, improved `.winmd` harvesting, enhanced `.appxmanifest` rewriting) are being added to this package.

**Fix:**
No action required. This is informational.

---

### Scenario 2: MSIX Bundle Support

**Cause:** Developers are unsure about MSIX bundle support in the `Microsoft.Windows.SDK.BuildTools.MSIX` NuGet package.
> Source: @Sergio0694, @snigdha011997 in [#5060](https://github.com/microsoft/WindowsAppSDK/issues/5060)

**Details:**
- The latest version of the package ([1.2.20250107.1](https://www.nuget.org/packages/Microsoft.Windows.SDK.BuildTools.MSIX/1.2.20250107.1)) already supports MSIX bundles.
- Support for `.msixupload` bundles was planned for release in February 2025.

**Fix:**
1. Ensure you are using version `1.2.20250107.1` or later of the `Microsoft.Windows.SDK.BuildTools.MSIX` NuGet package.
2. Check for updates to the package for additional features like `.msixupload` support.

> ✅ Confirmed by: @Sergio0694, @snigdha011997 in issue comments

**Verify:** Check the generated MSIX bundle or `.msixupload` file in your output directory.

---

### Scenario 3: Compatibility with Windows App SDK

**Cause:** The `Microsoft.Windows.SDK.BuildTools.MSIX` NuGet package is not yet fully compatible with Windows App SDK.
> Source: @Sergio0694 in [#5060](https://github.com/microsoft/WindowsAppSDK/issues/5060)

**Details:**
- If you reference both the `Microsoft.Windows.SDK.BuildTools.MSIX` NuGet package and Windows App SDK, Visual Studio may load the build task `.dll` from Windows App SDK instead of the MSIX NuGet package.
- This is expected behavior because Windows App SDK is not yet compatible with the MSIX NuGet package.

**Fix:**
1. Avoid using the `Microsoft.Windows.SDK.BuildTools.MSIX` NuGet package alongside Windows App SDK until compatibility is officially supported.
2. Compatibility is planned for Windows App SDK 1.8.

> ✅ Confirmed by: @Sergio0694 in issue comments

**Verify:** Ensure that the build task `.dll` is loaded from the correct package.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- None provided in the issue comments.

---

## References

- [Microsoft.Windows.SDK.BuildTools.MSIX NuGet Package](https://www.nuget.org/packages/Microsoft.Windows.SDK.BuildTools.MSIX/)
- [#5060](https://github.com/microsoft/WindowsAppSDK/issues/5060)

---

**Updated:** 2026-03-30 | **Confidence:** 0.9  
**Sources:** [#5060](https://github.com/microsoft/WindowsAppSDK/issues/5060)