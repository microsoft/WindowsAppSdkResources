# Error: "Package dependency criteria could not be resolved" (0x80670016) - MddBootstrapInitialize

**Keywords:** Package dependency criteria could not be resolved, 0x80670016, MddBootstrapInitialize, WindowsAppRuntime.Bootstrap.dll

**Error Example:**
```
Package dependency criteria could not be resolved
```

---

## Quick Match

**You're seeing this if:**
- Error contains "Package dependency criteria could not be resolved"
- Calling `MddBootstrapInitialize`
- Platform: Windows 11, Visual Studio 2022

→ Check scenarios below for your specific cause

---

## Related Issues

- [#5530](https://github.com/microsoft/WindowsAppSDK/issues/5530) - MddBootstrapInitialize fails with error "Package dependency criteria could not be resolved" (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: Hardcoded version mismatch in sample code

**Cause:** The version specified in the sample code (`majorMinorVersion`) does not match the installed version of the Windows App Runtime. The sample code uses a hardcoded version (`0x00010002`), which may not align with the installed runtime version.
> Source: @lb90 in [#5530](https://github.com/microsoft/WindowsAppSDK/issues/5530)

**Fix:**
1. Update the `majorMinorVersion` in your code to match the installed version of the Windows App Runtime.
   - For example, if the installed version is `1.7`, update the code to:
     ```c++
     const UINT32 majorMinorVersion{ 0x00010007 };
     ```
2. Rebuild and run your application.

> ✅ Confirmed by: @lb90 in issue comments

**Verify:** Run your application again. The error should no longer occur.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- None for this issue.

---

## References

- [Windows App SDK Tutorial - Unpackaged Deployment](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/tutorial-unpackaged-deployment?tabs=cpp)
- [Windows App SDK Downloads](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads)

---

**Updated:** 2026-03-17 | **Confidence:** 0.9
**Sources:** [#5530](https://github.com/microsoft/WindowsAppSDK/issues/5530)