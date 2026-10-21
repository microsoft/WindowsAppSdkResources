# Documentation Gaps and Errors in Windows App SDK

**Keywords:** missing documentation, broken links, experimental APIs, AppWindow, Windows App SDK, downloads archive, release notes

**Error Example:**
```
Was working on a partner app and received the following runtime error:

[Image showing runtime error]
```

---

## Quick Match

**You're seeing this if:**
- You encounter missing or incorrect documentation for Windows App SDK releases or APIs.
- You cannot find specific runtime versions in the downloads archive.
- You notice broken links or incorrect information in release notes or documentation pages.

→ Check scenarios below for your specific cause.

---

## Related Issues

- [#5938](https://github.com/microsoft/WindowsAppSDK/issues/5938) - Windows App Runtime 1.8-experimental releases missing from archive (Status: Closed)
- [#5896](https://github.com/microsoft/WindowsAppSDK/issues/5896) - Missing documentation and release plans for new AppWindow placement APIs (experimental) (Status: Open)
- [#5727](https://github.com/microsoft/WindowsAppSDK/issues/5727) - 1.8-preview documentation contains errors, broken links (Status: Closed)
- [#5351](https://github.com/microsoft/WindowsAppSDK/issues/5351) - Windows App SDK 1.7-exp releases are missing from downloads page (Status: Closed)
- [#5522](https://github.com/microsoft/WindowsAppSDK/issues/5522) - Windows App SDK 1.8 Experimental 3 runtimes missing (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: Missing runtime versions in the downloads archive

**Cause:** Specific experimental runtime versions were not properly listed in the downloads archive due to documentation update delays.
> Source: @riverar in [#5938](https://github.com/microsoft/WindowsAppSDK/issues/5938)

**Fix:**
1. Check the [downloads archive](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads-archive#windows-app-sdk-18-experimental) for the runtime versions. Experimental versions are listed after the stable and preview sections.
2. If the version is still missing, refer to the direct download links provided in the issue comments (if available).

> ✅ Confirmed by: @riverar in [#5938](https://github.com/microsoft/WindowsAppSDK/issues/5938)

**Verify:** Ensure the runtime version you need is listed and accessible in the downloads archive.

---

### Scenario 2: Missing or incomplete documentation for experimental APIs

**Cause:** Experimental APIs often receive minimal documentation, with only stubs or basic references available.
> Source: @Karl-Bridge-Microsoft in [#5896](https://github.com/microsoft/WindowsAppSDK/issues/5896)

**Fix:**
1. Use the version dropdown in the API reference to select the correct experimental version (e.g., 1.8 Experimental).
2. Refer to the [AppWindowPlacement spec](https://github.com/microsoft/WindowsAppSDK/blob/afd4ac42a32a329f4fdfc76d7d443a0200774135/specs/Windowing/AppWindowPlacement.md) for additional details on experimental APIs.
3. Note that some APIs, such as `SaveCurrentPlacementForAllPersistedStateIds`, require a `PersistedStateId` to function properly.
   > Source: @Lightczx in [#5896](https://github.com/microsoft/WindowsAppSDK/issues/5896)

**Verify:** Confirm that the API reference or spec provides the information you need to use the APIs effectively.

---

### Scenario 3: Broken links or incorrect information in release notes

**Cause:** Errors in the documentation update process can result in broken links, incorrect titles, or missing information.
> Source: @riverar in [#5727](https://github.com/microsoft/WindowsAppSDK/issues/5727)

**Fix:**
1. Use the following checklist to verify release notes and downloads:
   - Ensure release notes have the correct title, version, and matching NuGet package version.
   - Verify that download links point to the correct files and are placed in the appropriate sections (Stable / Preview / Experimental).
   - Confirm that all features and changes are documented in the release notes.
2. If links are broken, check for updates or corrections in the issue comments or related GitHub pull requests.

> ✅ Confirmed by: @riverar in [#5727](https://github.com/microsoft/WindowsAppSDK/issues/5727)

**Verify:** Test the corrected links and ensure they lead to the intended resources.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- Use the [OMD tool](https://github.com/dotMorten/DotNetOMDGenerator) to analyze API changes in experimental releases.  
  > Source: @ghost1372 in [#5522](https://github.com/microsoft/WindowsAppSDK/issues/5522)

---

## References

- [Windows App SDK Downloads Archive](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads-archive)
- [AppWindowPlacement Spec](https://github.com/microsoft/WindowsAppSDK/blob/afd4ac42a32a329f4fdfc76d7d443a0200774135/specs/Windowing/AppWindowPlacement.md)
- [Windows App SDK API Reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/)

---

**Updated:** 2026-03-17 | **Confidence:** 0.9  
**Sources:** [#5938](https://github.com/microsoft/WindowsAppSDK/issues/5938), [#5896](https://github.com/microsoft/WindowsAppSDK/issues/5896), [#5727](https://github.com/microsoft/WindowsAppSDK/issues/5727), [#5351](https://github.com/microsoft/WindowsAppSDK/issues/5351), [#5522](https://github.com/microsoft/WindowsAppSDK/issues/5522)