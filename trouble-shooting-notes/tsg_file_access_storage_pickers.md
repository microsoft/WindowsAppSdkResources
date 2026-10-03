# Storage Picker Issues — File Access & Picker Dialogs

**Keywords:** FileOpenPicker, FileSavePicker, FolderPicker, storage pickers, file picker crash, COMException, E_FAIL, IInitializeWithWindow, DefaultFileExtension, FileTypeChoices, WSL, RTL, language override, Element not found, blank file type list, empty file creation, self-contained, pri files, SuggestedStartLocation

**Error Example:**
```
System.Runtime.InteropServices.COMException: 'Error HRESULT E_FAIL has been returned from a call to a COM component.'
   at Windows.Storage.Pickers.FileOpenPicker.PickSingleFileAsync()
```

---

## Quick Match

**You're seeing this if:**
- Error contains "E_FAIL" or "COMException" when calling `PickSingleFileAsync()` or `PickSaveFileAsync()`
- File/Folder picker dialog behaves unexpectedly (wrong extension, missing folders, wrong language, blank file type list, or crashes)
- Using `FileOpenPicker`, `FileSavePicker`, or `FolderPicker` from a WinUI 3 / Windows App SDK desktop app
- Platform: Windows App SDK (packaged or unpackaged), WinUI 3

→ Check scenarios below for your specific cause

---

## Related Issues

- [#1063](https://github.com/microsoft/WindowsAppSDK/issues/1063) — Simplify using UWP file pickers from desktop apps (Status: Closed — feature proposal)
- [#2504](https://github.com/microsoft/WindowsAppSDK/issues/2504) — FileOpenPicker crashes when app runs as Administrator (Status: Closed)
- [#5612](https://github.com/microsoft/WindowsAppSDK/issues/5612) — FileOpenPicker always crashes (Status: Closed)
- [#5747](https://github.com/microsoft/WindowsAppSDK/issues/5747) — ComException when using new FileSavePicker (Status: Closed)
- [#5749](https://github.com/microsoft/WindowsAppSDK/issues/5749) — FolderPicker not working in 1.8-preview with SelfContained: Element not found (Status: Closed)
- [#5827](https://github.com/microsoft/WindowsAppSDK/issues/5827) — Order of FileTypeChoices in new pickers is not respected (Status: Closed)
- [#5836](https://github.com/microsoft/WindowsAppSDK/issues/5836) — `SuggestedStartLocation` not working correctly in File/Folder pickers (Status: Closed)
- [#5837](https://github.com/microsoft/WindowsAppSDK/issues/5837) — Most entries in the FileOpenPicker file type list are blank (Status: Closed)
- [#5975](https://github.com/microsoft/WindowsAppSDK/issues/5975) — FileSavePicker cannot set default extension when defining FileTypeChoices (Status: Closed)
- [#5976](https://github.com/microsoft/WindowsAppSDK/issues/5976) — FileSavePicker auto creates empty file after clicking OK button (Status: Closed)
- [#6284](https://github.com/microsoft/WindowsAppSDK/issues/6284) — FolderPicker does not show WSL (Linux) folders (Status: Open)
- [#6105](https://github.com/microsoft/WindowsAppSDK/issues/6105) — How to change or force storage pickers to a specific language? (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: FileOpenPicker / FolderPicker Crashes with COMException When Running as Administrator

**Cause:** Calling `PickSingleFileAsync()` on a `FileOpenPicker` (or `FolderPicker`) while the app is running elevated (Run as Administrator) throws `COMException: Error HRESULT E_FAIL`. This is a known limitation of the Windows shell file picker COM infrastructure under elevated processes.
> Source: Issue [#2504](https://github.com/microsoft/WindowsAppSDK/issues/2504)

**Affected versions:** Windows App SDK 1.4.2+, .NET 7+, Windows 11

**Fix:**
1. Avoid running the app elevated when file picker usage is required. Separate elevated operations into a background service or helper process.
2. Use Win32 `GetOpenFileName` / `IFileDialog` COM APIs directly instead of UWP pickers when elevation is required.
3. If elevation is unavoidable, wrap the picker call in a try/catch and fall back to a Win32 file dialog.

---

### Scenario 2: IInitializeWithWindow Boilerplate Required for Desktop Apps

**Cause:** UWP file pickers (`FileOpenPicker`, `FileSavePicker`, `FolderPicker`) require a parent window handle (HWND) when used in Win32/desktop apps. Without calling `IInitializeWithWindow.Initialize()`, the picker has no owner window and will fail.
> Source: Issue [#1063](https://github.com/microsoft/WindowsAppSDK/issues/1063)

**Fix (WinUI 3 / Windows App SDK 1.8+):**
```csharp
var picker = new FileOpenPicker(appWindow.Id);
picker.FileTypeFilter.Add("*");
var file = await picker.PickSingleFileAsync();
```

**Fix (Older SDK versions):**
```csharp
var picker = new FileOpenPicker();
picker.FileTypeFilter.Add("*");
var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
var file = await picker.PickSingleFileAsync();
```

---

### Scenario 3: FileSavePicker Ignores DefaultFileExtension — Always Uses First Sorted Entry

**Cause:** The `DefaultFileExtension` property is ignored. `FileTypeChoices` is always sorted alphabetically, and the first sorted item is selected by default regardless of `DefaultFileExtension` or insertion order.
> Source: Issue [#5975](https://github.com/microsoft/WindowsAppSDK/issues/5975)

**Fix / Workaround:**
1. Order your `FileTypeChoices` so the desired default is alphabetically first.
2. Use `SuggestedFileName` with the desired extension to hint the dialog.

---

### Scenario 4: FolderPicker Does Not Show WSL (Linux) Folders

**Cause:** `FolderPicker` does not enumerate WSL network locations (`\\wsl$\...`).
> Source: Issue [#6284](https://github.com/microsoft/WindowsAppSDK/issues/6284)

**Fix / Workaround:**
1. Use `FileOpenPicker` first to navigate to a WSL path, then immediately open `FolderPicker`.
2. Use Win32 `IFileDialog` with `FOS_PICKFOLDERS` flag for reliable WSL folder browsing.

---

### Scenario 5: Storage Pickers Inherit App Language / RTL Layout Override

**Cause:** Setting `Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride` changes the language and layout direction (LTR/RTL) of storage picker dialogs.
> Source: Issue [#6105](https://github.com/microsoft/WindowsAppSDK/issues/6105)

**Fix / Workaround:**
1. Temporarily reset `PrimaryLanguageOverride` before opening the picker, then restore it.
2. Use `ResourceContext` with a specific qualifier instead of the global language override.

---

### Scenario 6: FileOpenPicker Always Crashes

**Cause:** Missing `InitializeWithWindow` call when creating the picker.
> Source: @castorix in [#5612](https://github.com/microsoft/WindowsAppSDK/issues/5612)

**Fix:** Ensure the picker is initialized with the correct window handle:
```cpp
const FileOpenPicker picker(AppWindow.Id);
WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
```

---

### Scenario 7: FolderPicker Not Working in SelfContained Mode

**Cause:** Missing `.pri` files in the app package when using `<WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>`.
> Source: @DinahK-2SO in [#5749](https://github.com/microsoft/WindowsAppSDK/issues/5749)

**Fix:** Update to `Microsoft.Windows.SDK.BuildTools.MSIX 1.7.20250829.1` or later.

---

### Scenario 8: Most Entries in FileOpenPicker File Type List Are Blank

**Cause:** File type descriptions are not displayed due to a bug in the picker.
> Source: @DinahK-2SO in [#5837](https://github.com/microsoft/WindowsAppSDK/issues/5837)

**Fix:** Update to Windows App SDK 1.8.4 or later.

---

### Scenario 9: FileSavePicker Auto-Creates Empty File After Clicking OK

**Cause:** Default behavior of `FileSavePicker` is to create an empty file when `PickSaveFileAsync()` is called.
> Source: @DinahK-2SO in [#5976](https://github.com/microsoft/WindowsAppSDK/issues/5976)

**Fix:** Update to Windows App SDK 2.0 or later. New property `ShowOverwritePrompt` and updated behavior prevent automatic file creation.

---

## Known Issues

### Issue: `SuggestedStartLocation` Not Working Correctly in File/Folder Pickers

**Cause:** The `SuggestedStartLocation` property does not always work as expected. Instead of opening in the specified location (e.g., Downloads or Desktop), the picker defaults to the last selected location.
> Source: Issue [#5836](https://github.com/microsoft/WindowsAppSDK/issues/5836)

**Workaround:** None confirmed. A potential workaround is to rename the app (if unpackaged) or reinstall the app (if packaged) to reset the "last selected location." However, this is not a reliable or practical solution.

---

## ⚠️ Unverified / Community Suggestions

- For the Administrator crash (#2504): Some users report that creating pickers on the UI thread specifically (not from a background task) may reduce crash frequency, but this is not a reliable fix.
- For `SuggestedStartLocation` (#5836): A potential fix may be available in a future update. See [#5772](https://github.com/microsoft/WindowsAppSDK/pull/5772).

---

## References

- [Windows App SDK Storage Pickers documentation](https://learn.microsoft.com/en-us/windows/apps/develop/files/file-pickers)
- [Display UI objects in WinUI 3 (IInitializeWithWindow)](https://docs.microsoft.com/en-us/windows/apps/develop/ui-input/display-ui-objects)
- [FileOpenPicker API reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.windows.storage.pickers.fileopenpicker)
- [FileSavePicker API reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.windows.storage.pickers.filesavepicker)

---

**Updated:** 2026-03-17 | **Confidence:** 0.9
**Sources:** #1063, #2504, #5612, #5747, #5749, #5827, #5836, #5837, #5975, #5976, #6284, #6105