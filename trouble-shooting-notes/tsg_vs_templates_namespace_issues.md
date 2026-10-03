# Error: "VS Templates Missing File-Scoped Namespace Changes" - Visual Studio Templates

**Keywords:** Visual Studio, templates, file-scoped namespaces, Windows App SDK, C#

**Error Example:**
When creating a new project using the Windows App SDK templates in Visual Studio 17.14 Preview 3 or 4, the generated code does not include file-scoped namespaces, even though other template changes (e.g., mica background, button removal) are present. The generated code still uses block-scoped namespaces.

---

## Quick Match

**You're seeing this if:**
- You are using Visual Studio 17.14 Preview 3 or 4.
- You create a new project using the Windows App SDK templates.
- The generated code uses block-scoped namespaces instead of file-scoped namespaces.

→ Check scenarios below for your specific cause.

---

## Related Issues

- [#5350](https://github.com/microsoft/WindowsAppSDK/issues/5350) - VS 17.14 Preview 3 C# template missing file-scoped namespace changes (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: Visual Studio Template Version Outdated

**Cause:** The installed Windows App SDK template version in Visual Studio is outdated and does not include the latest changes for file-scoped namespaces.
> Source: @haonanttt in [#5350](https://github.com/microsoft/WindowsAppSDK/issues/5350)

**Fix:**
1. Open Visual Studio.
2. Navigate to `Extensions -> Manage Extensions -> Installed`.
3. Search for `template` and locate the "Windows App SDK C++ (or C#) VS2022 Templates".
4. Check the installed version of the template. If it is outdated, update to the latest version.

> ✅ Confirmed by: @haonanttt in [#5350](https://github.com/microsoft/WindowsAppSDK/issues/5350)

**Verify:** Create a new project using the updated template and check if the generated code uses file-scoped namespaces.

---

### Scenario 2: Visual Studio Settings Default to Block-Scoped Namespaces

**Cause:** Visual Studio's default settings for C# Code Style are configured to use block-scoped namespaces instead of file-scoped namespaces.
> Source: @michael-hawker in [#5350](https://github.com/microsoft/WindowsAppSDK/issues/5350)

**Fix:**
1. Open Visual Studio.
2. Go to `Tools -> Options`.
3. Navigate to `Text Editor -> C# -> Code Style`.
4. Locate the setting for namespace declarations and change it from "Block Scoped" to "File Scoped".

> ✅ Confirmed by: @michael-hawker in [#5350](https://github.com/microsoft/WindowsAppSDK/issues/5350)

**Verify:** Create a new project using the Windows App SDK template and confirm that the generated code uses file-scoped namespaces.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- None reported for this issue.

---

## References

- [Issue #5350](https://github.com/microsoft/WindowsAppSDK/issues/5350)
- [Official Windows App SDK Documentation](https://learn.microsoft.com/windows/apps/windows-app-sdk/)

---

**Updated:** 2026-05-18 | **Confidence:** 0.9  
**Sources:** [#5350](https://github.com/microsoft/WindowsAppSDK/issues/5350)