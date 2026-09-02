# Error: "The product is not applicable or cannot be found." (0x80073D3B) - Windows ML Execution Provider

**Keywords:** 0x80073D3B, The product is not applicable or cannot be found, Windows ML, Execution Provider, Native AOT, WSUS, Microsoft.Windows.AI.MachineLearning

**Error Example:**
```
Exception thrown at 0x00007FF99FC566CA (KernelBase.dll) in CppConsoleDesktop.exe: WinRT originate error - 0x80073D3B : 'Deployment Add operation with target volume on Package Windows.Workload.ExecutionProvider.OpenVino.amd64 from: (Windows.Workload.ExecutionProvider.OpenVino.amd64) failed with error 0x80073D3B. See http://go.microsoft.com/fwlink/?LinkId=235160 for help diagnosing app deployment issues.'.
C:\__w\1\s\dev\PackageManager\API\M.W.M.D.PackageDeploymentManager.cpp(1989)\Microsoft.WindowsAppRuntime.dll!00007FF873D2DFEE: (caller: 00007FF873D865DF) ReturnHr(1) tid(7858) 80073D3B The product is not applicable or cannot be found.
    Msg:[ExtendedError:0x80073D3B PackageFamilyName:MicrosoftCorporationII.WinML.Intel.OpenVINO.EP.1.8_8wekyb3d8bbwe PackageUri:uup://Product/Windows.Workload.ExecutionProvider.OpenVino.amd64 : Deployment Add operation with target volume on Package Windows.Workload.ExecutionProvider.OpenVino.amd64 from: (Windows.Workload.ExecutionProvider.OpenVino.amd64) failed with error 0x80073D3B. See http://go.microsoft.com/fwlink/?LinkId=235160 for help diagnosing app deployment issues.] CallContext:[\EnsurePackageSetReadyAsync]
```

---

## Quick Match

**You're seeing this if:**
- Error contains "The product is not applicable or cannot be found" or "0x80073D3B"
- Encountered while using Windows ML Execution Provider
- Platform: Windows 11 (24H1 or 24H2)

→ Check scenarios below for your specific cause

---

## Related Issues

- [#5882](https://github.com/microsoft/WindowsAppSDK/issues/5882) - `Microsoft.Windows.AI.MachineLearning` does not support Native AOT (Status: Closed, Fixed in 1.8.251106002)
- [#5862](https://github.com/microsoft/WindowsAppSDK/issues/5862) - Fetching Execution provider with WindowsML ends up with "The product is not applicable or cannot be found." (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: Using an outdated version of the Windows App SDK

**Cause:** The issue occurs because the `Microsoft.Windows.AI.MachineLearning` package includes projection DLLs via its targets, which may cause problems with Native AOT publishing.
> Source: @manodasanW [MSFT] in [#5882](https://github.com/microsoft/WindowsAppSDK/issues/5882)

**Fix:**
1. Update the Windows App SDK to version `1.8.251106002` or later, where this issue has been addressed.

> ✅ Confirmed by: @manodasanW [MSFT], @Gaoyifei1011 in issue comments

**Verify:** Ensure that no managed DLLs are included in the output directory when publishing with Native AOT.

---

### Scenario 2: Enterprise WSUS server blocking Execution Provider downloads

**Cause:** The issue occurs when the system is configured to use a non-default WSUS (Windows Server Update Services) server, which may not deliver the required Execution Provider packages.
> Source: @vlejeune-dxo in [#5862](https://github.com/microsoft/WindowsAppSDK/issues/5862)

**Fix:**
1. Temporarily switch to the official Microsoft Update server:
   - Open the Windows Update settings.
   - Change the update source to the official Microsoft Update server.
2. Retry the operation to fetch the Execution Provider.

> ✅ Confirmed by: @vlejeune-dxo in issue comments

**Verify:** Ensure that the Execution Provider is successfully downloaded and the application runs without the error.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- Ensure that the WSUS server is configured to deliver the required Execution Provider packages. For more details, consult the [official Microsoft documentation](http://go.microsoft.com/fwlink/?LinkId=235160).
  > Source: @vlejeune-dxo in [#5862](https://github.com/microsoft/WindowsAppSDK/issues/5862)

---

## References

- [Official docs](http://go.microsoft.com/fwlink/?LinkId=235160)
- [API docs](https://learn.microsoft.com/en-us/windows/ai/windows-ml/)

---

**Updated:** 2026-03-17 | **Confidence:** 0.9  
**Sources:** [#5882](https://github.com/microsoft/WindowsAppSDK/issues/5882), [#5862](https://github.com/microsoft/WindowsAppSDK/issues/5862)