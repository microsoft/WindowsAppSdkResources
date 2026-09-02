# Error: WinML ORT InferenceSession Fails When Model Path Contains Non-ASCII Unicode Characters

**Keywords:** WinML, ORT, ONNX Runtime, InferenceSession, Unicode, non-ASCII, model path, inference_session.cc, multi-byte code page, std::filesystem, QNN, DirectML, CPU Execution Provider

**Error Example:**
```
[E:onnxruntime:, inference_session.cc:2545 onnxruntime::InferenceSession::Initialize::<lambda_b590a375cc4159bef6c92b76b4894c14>::operator ()] Exception during initialization: No mapping for the Unicode character exists in the target multi-byte code page.
```

---

## Quick Match

**You're seeing this if:**
- Error contains "No mapping for the Unicode character exists in the target multi-byte code page"
- Using WinML with ONNX Runtime (ORT) on Windows
- Model file path contains non-ASCII characters (e.g., Chinese, Japanese, accented Latin characters)
- Works with CPU Execution Provider but fails with QNN or DirectML

→ See Scenario 1 below

---

## Related Issues

- [#6173](https://github.com/microsoft/WindowsAppSDK/issues/6173) — WinML ORT compile and inferenceSession error when the model path contains any non-ASCII Unicode characters (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: ORT Uses Narrow String Conversion for Model Paths

**Cause:** In 10 locations across ONNX Runtime's session and execution provider code, `std::filesystem::path::filename().string()` is used to convert model paths to narrow (ANSI) strings. On Windows, `.string()` converts via the system ANSI code page, which cannot represent many Unicode characters. This causes the path conversion to throw when the model path contains characters outside the system's ANSI code page.

The bug originates in `microsoft/onnxruntime`, not in WindowsAppSDK. The WinML runtime consumes ONNX Runtime as a binary dependency. The issue does not occur with the CPU Execution Provider because the CPU EP code path does not use the narrow-string conversion for model paths.

> Source: @sagarbhure-msft in [#6173](https://github.com/microsoft/WindowsAppSDK/issues/6173)

**Workaround — Move model to ASCII-only path:**
1. Copy or move the ONNX model file to a directory with only ASCII characters in the path:
   ```
   ❌ C:\Users\用户\Models\model.onnx
   ✅ C:\Models\model.onnx
   ```
2. Update your code to reference the new path.

**Permanent Fix:** A PR has been opened in the ONNX Runtime repository to replace all 10 occurrences of `.string()` with `.wstring()` or use wide-string APIs.
> Source: @sagarbhure-msft in [#6173](https://github.com/microsoft/WindowsAppSDK/issues/6173)

> ✅ Confirmed root cause by: @rnagata0 in [#6173](https://github.com/microsoft/WindowsAppSDK/issues/6173)

**Verify:** After moving the model to an ASCII-only path, the InferenceSession should initialize successfully with QNN/DirectML EPs.

---

## References

- [ONNX Runtime GitHub](https://github.com/microsoft/onnxruntime)
- [WinML overview](https://learn.microsoft.com/en-us/windows/ai/windows-ml/)

---

**Updated:** 2026-03-20 | **Confidence:** 0.90
**Sources:** [#6173](https://github.com/microsoft/WindowsAppSDK/issues/6173)
