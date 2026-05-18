# Error: "Unexpected AI-related DLLs are bundled in my app" - Microsoft.WindowsAppSDK

**Keywords:** AI-related DLLs, Microsoft.WindowsAppSDK, transitive dependencies, onnxruntime.dll, DirectML.dll, Microsoft.Windows.AI.MachineLearning.dll, WindowsAppSDKSelfContained

**Error Example:**
```
onnxruntime.dll 21MB
DirectML.dll 18MB
Microsoft.Windows.AI.MachineLearning.dll 870kb
```

---

## Quick Match

**You're seeing this if:**
- Your app includes unexpected AI-related DLLs such as `onnxruntime.dll`, `DirectML.dll`, or `Microsoft.Windows.AI.MachineLearning.dll`
- You are referencing the `Microsoft.WindowsAppSDK` NuGet package
- Platform: Windows

→ Check scenarios below for your specific cause

---

## Related Issues

- [#6464](https://github.com/microsoft/WindowsAppSDK/issues/6464) - Unexpected AI-related DLLs are bundled into my app (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: Transitive dependencies from `Microsoft.WindowsAppSDK`

**Cause:** Referencing the `Microsoft.WindowsAppSDK` metapackage pulls in transitive dependencies, including AI-related DLLs. These dependencies are part of the package and are included by default.
> Source: @ssparach in [#6464](https://github.com/microsoft/WindowsAppSDK/issues/6464)

**Fix:**
1. Instead of referencing the `Microsoft.WindowsAppSDK` metapackage, reference only the specific component packages your app depends on. For example:
   ```xml
   <PackageReference Include="Microsoft.WindowsAppSDK.Base" Version="2.0.3" />
   ```
2. If you need additional functionality, add the required component packages individually.

> ✅ Confirmed by: @lgztx96 in [#6464](https://github.com/microsoft/WindowsAppSDK/issues/6464)

**Verify:** Check your app's output directory to ensure the AI-related DLLs (`onnxruntime.dll`, `DirectML.dll`, `Microsoft.Windows.AI.MachineLearning.dll`) are no longer included.

---

### Scenario 2: Using `WindowsAppSDKSelfContained` property

**Cause:** The `WindowsAppSDKSelfContained` property determines whether the Windows App SDK is packaged as a self-contained deployment or as a framework-dependent deployment. Setting this property to `true` excludes the AI-related DLLs.
> Source: @catmanjan in [#6464](https://github.com/microsoft/WindowsAppSDK/issues/6464)

**Fix:**
1. Add the following property to your project file:
   ```xml
   <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
   ```
2. Build your project again. This will exclude the AI-related DLLs from your app.

> ✅ Confirmed by: @catmanjan in [#6464](https://github.com/microsoft/WindowsAppSDK/issues/6464)

**Verify:** Check your app's output directory to ensure the AI-related DLLs (`onnxruntime.dll`, `DirectML.dll`, `Microsoft.Windows.AI.MachineLearning.dll`) are no longer included.

---

### Scenario 3: Using `Microsoft.WindowsAppSDK.Runtime` instead of `Microsoft.WindowsAppSDK`

**Cause:** The `Microsoft.WindowsAppSDK` metapackage includes all dependent packages, including AI-related ones. Using `Microsoft.WindowsAppSDK.Runtime` as a standalone package avoids pulling in unnecessary dependencies.
> Source: @lgztx96 in [#6464](https://github.com/microsoft/WindowsAppSDK/issues/6464)

**Fix:**
1. Remove the `Microsoft.WindowsAppSDK` package reference from your project file.
2. Add a reference to `Microsoft.WindowsAppSDK.Runtime` and any other specific component packages you need. For example:
   ```xml
   <PackageReference Include="Microsoft.WindowsAppSDK.Runtime" Version="2.0.3" />
   <PackageReference Include="Microsoft.WindowsAppSDK.WinUI" Version="2.0.3" />
   ```

> ✅ Confirmed by: @lgztx96 in [#6464](https://github.com/microsoft/WindowsAppSDK/issues/6464)

**Verify:** Check your app's output directory to ensure the AI-related DLLs (`onnxruntime.dll`, `DirectML.dll`, `Microsoft.Windows.AI.MachineLearning.dll`) are no longer included.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- Use `<ExcludeAssets>` to exclude specific assets from the `Microsoft.WindowsAppSDK` package:
  ```xml
  <PackageReference Include="Microsoft.WindowsAppSDK" Version="2.0.1">
      <ExcludeAssets>native</ExcludeAssets>
  </PackageReference>
  <PackageReference Include="Microsoft.WindowsAppSDK.Base" Version="2.0.3" />
  ```
  > Source: @catmanjan in [#6464](https://github.com/microsoft/WindowsAppSDK/issues/6464)

---

## References

- [Official docs](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/deploy-overview)
- [Issue #6464](https://github.com/microsoft/WindowsAppSDK/issues/6464)

---

**Updated:** 2026-05-18 | **Confidence:** 0.9
**Sources:** [#6464](https://github.com/microsoft/WindowsAppSDK/issues/6464)