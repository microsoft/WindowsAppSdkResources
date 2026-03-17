# Error: "The RPC server is unavailable" (System.Runtime.InteropServices.COMException) - AppInstance.GetActivatedEventArgs()

**Keywords:** RPC server is unavailable, System.Runtime.InteropServices.COMException, AppInstance.GetActivatedEventArgs, activation arguments, WinUI3, Windows.ApplicationModel.AppInstance

**Error Example:**
```
System.Runtime.InteropServices.COMException: 'The RPC server is unavailable.'
```

---

## Quick Match

**You're seeing this if:**
- Error contains "The RPC server is unavailable"
- Calling `AppInstance.GetCurrent().GetActivatedEventArgs()` in a WinUI3 app
- Platform: Windows, Packaged (MSIX)

→ Check scenarios below for your specific cause

---

## Related Issues

- [#5481](https://github.com/microsoft/WindowsAppSDK/issues/5481) - Exception triggered when calling `AppInstance.GetCurrent().GetActivatedEventArgs()` (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: Activation arguments lifetime tied to the calling process

**Cause:** The activation arguments object is created in the calling process and is only available while that process is running. If the calling process terminates too quickly, the activation arguments become unavailable.
> Source: @florelis [MSFT] in [#5481](https://github.com/microsoft/WindowsAppSDK/issues/5481)

**Fix:**
1. Add a delay to extend the lifetime of the calling process.
2. For example, in the console application, add a `Thread.Sleep()` after `Process.Start()` to keep the process alive longer.

> ✅ Confirmed by: @florelis [MSFT] in issue comments

**Verify:** Add a `Thread.Sleep()` in the console application and observe if the error no longer occurs.

---

### Scenario 2: Use Win32 APIs to retrieve activation parameters

**Cause:** The `AppInstance.GetCurrent().GetActivatedEventArgs()` method may not work reliably in certain scenarios, such as when called from a non-UWP packaged app.
> Source: @lgBlog in [#5481](https://github.com/microsoft/WindowsAppSDK/issues/5481)

**Fix:**
1. Use the following Win32 APIs to retrieve activation parameters directly:
   - `GetCommandLineW`
   - `CommandLineToArgvW`
   - `LocalFree`
2. Example implementation:
   ```csharp
   [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
   private static extern IntPtr GetCommandLineW();

   [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
   private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

   [DllImport("kernel32.dll")]
   private static extern IntPtr LocalFree(IntPtr hMem);

   private List<string> Win32GetActivationFiles()
   {
       var files = new List<string>(4);

       IntPtr ptr = GetCommandLineW();
       string cmdLine = Marshal.PtrToStringUni(ptr) ?? string.Empty;

       if (string.IsNullOrEmpty(cmdLine))
           return files;

       IntPtr argv = CommandLineToArgvW(cmdLine, out int argc);
       if (argv == IntPtr.Zero)
           return files;

       try
       {
           for (int i = 0; i < argc; i++)
           {
               string arg = Marshal.PtrToStringUni(Marshal.ReadIntPtr(argv, i * IntPtr.Size)) ?? string.Empty;
               files.Add(arg);
           }
       }
       finally
       {
           LocalFree(argv);
       }

       return files;
   }
   ```

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- None provided.

---

## References

- [Issue #5481](https://github.com/microsoft/WindowsAppSDK/issues/5481)
- [AppInstance.GetActivatedEventArgs() documentation](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/api/winrt/microsoft.windows.applicationsmodel.appinstance.getactivatedeventargs)

---

**Updated:** 2026-03-17 | **Confidence:** 0.9
**Sources:** [#5481](https://github.com/microsoft/WindowsAppSDK/issues/5481)