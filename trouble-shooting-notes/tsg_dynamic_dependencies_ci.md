# Error: "Tests hang in GitHub Actions CI after updating to WinAppSDK 1.8.250907003"

**Keywords:** tests hang, GitHub Actions, CI, bootstrapper initializer, WinAppSDK 1.8, Windows App SDK Runtime Framework package

**Error Example:**
```
Tests hang indefinitely after updating to Windows App SDK 1.8.250907003. No specific error message is displayed, but the CI pipeline does not complete.
```

---

## Quick Match

**You're seeing this if:**
- Tests hang indefinitely in GitHub Actions CI after updating to Windows App SDK 1.8.250907003.
- The project uses the Windows App SDK but does not include the Runtime Framework package in the CI environment.
- Platform: GitHub Actions CI.

→ Check scenarios below for your specific cause.

---

## Related Issues

- [#5851](https://github.com/microsoft/WindowsAppSDK/issues/5851) - WinAppSDK 1.8.250907003 Fails to run tests in GitHub Actions CI (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: Missing Runtime Framework Package in CI Environment

**Cause:**  
With Windows App SDK 1.8, the bootstrapper initializer is automatically enabled by default. This requires the Windows App SDK Runtime Framework package to be installed. If the package is not present, the application shows an error UI and the tests hang indefinitely.  
> Source: @ssparach [MSFT] in [#5851](https://github.com/microsoft/WindowsAppSDK/issues/5851)

**Fix:**  
Option 1: Disable the bootstrapper initializer in the project file.  
1. Open your `.csproj` file.  
2. Add the following line inside a `<PropertyGroup>`:  
   ```xml
   <WindowsAppSdkBootstrapInitialize>false</WindowsAppSdkBootstrapInitialize>
   ```  

Option 2: Install the Runtime Framework package as part of your CI build process.  
1. Add a step in your GitHub Actions workflow to install the Windows App SDK Runtime Framework package.  
   For example, you can use the installer provided by Microsoft as part of your build process.  

> ✅ Confirmed by: @ssparach [MSFT], @abdes in issue comments

**Verify:**  
- For Option 1: Ensure the tests run successfully in the CI environment without hanging.  
- For Option 2: Verify that the Runtime Framework package is installed in the CI environment and the tests complete successfully.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- None provided in the issue comments.

---

## References

- [Issue #5851](https://github.com/microsoft/WindowsAppSDK/issues/5851)

---

**Updated:** 2026-03-17 | **Confidence:** 0.9  
**Sources:** [#5851](https://github.com/microsoft/WindowsAppSDK/issues/5851)