# Storage Picker Issues — File Access & Picker Dialogs

**Keywords:** FileOpenPicker, FileSavePicker, FolderPicker, storage pickers, file picker crash, COMException, E_FAIL, IInitializeWithWindow, DefaultFileExtension, FileTypeChoices, WSL, RTL, language override

**Error Example:**
```
System.Runtime.InteropServices.COMException: 'Error HRESULT E_FAIL has been returned from a call to a COM component.'
   at Windows.Storage.Pickers.FileOpenPicker.PickSingleFileAsync()
```

---

## Quick Match

**You're seeing this if:**
- Error contains "E_FAIL" or "COMException" when calling `PickSingleFileAsync()` or `PickSaveFileAsync()`
- File/Folder picker dialog behaves unexpectedly (wrong extension, missing folders, wrong language)
- Using `FileOpenPicker`, `FileSavePicker`, or `FolderPicker` from a WinUI 3 / Windows App SDK desktop app
- Platform: Windows App SDK (packaged or unpackaged), WinUI 3

→ Check scenarios below for your specific cause

---

## Related Issues

- [#1063](https://github.com/microsoft/WindowsAppSDK/issues/1063) — Simplify using UWP file pickers from desktop apps (Status: Closed — feature proposal)
- [#2504](https://github.com/microsoft/WindowsAppSDK/issues/2504) — FileOpenPicker crashes when app runs as Administrator (Status: Closed)
- [#5975](https://github.com/microsoft/WindowsAppSDK/issues/5975) — FileSavePicker cannot set default extension when defining FileTypeChoices (Status: Closed)
- [#6284](https://github.com/microsoft/WindowsAppSDK/issues/6284) — FolderPicker does not show WSL (Linux) folders (Status: Open)
- [#6105](https://github.com/microsoft/WindowsAppSDK/issues/6105) — How to change or force storage pickers to a specific language? (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: FileOpenPicker / FolderPicker Crashes with COMException When Running as Administrator

**Cause:** Calling `PickSingleFileAsync()` on a `FileOpenPicker` (or `FolderPicker`) while the app is running elevated (Run as Administrator) throws `COMException: Error HRESULT E_FAIL`. This is a known limitation of the Windows shell file picker COM infrastructure under elevated processes.
> Source: Issue [#2504](https://github.com/microsoft/WindowsAppSDK/issues/2504)

**Affected versions:** Windows App SDK 1.4.2+, .NET 7+, Windows 11

**Repro code that triggers the crash:**
```csharp
var picker = new FileOpenPicker();
picker.FileTypeFilter.Add("*");
InitializeWithWindow.Initialize(picker, App.MainWindow.GetHandle());
var file = await picker.PickSingleFileAsync(); // Crashes when elevated
```

**Fix:**
1. **Avoid running the app elevated** when file picker usage is required. Separate elevated operations into a background service or helper process.
2. **Use Win32 `GetOpenFileName` / `IFileDialog` COM APIs** directly instead of UWP pickers when elevation is required — these Win32 APIs work under Administrator.
3. If elevation is unavoidable, wrap the picker call in a try/catch and fall back to a Win32 file dialog:
```csharp
try
{
    var file = await picker.PickSingleFileAsync();
}
catch (COMException ex) when (ex.HResult == unchecked((int)0x80004005))
{
    // Fall back to Win32 IFileDialog
}
```

**Verify:** Launch the app as Administrator and confirm file picker opens without crash.

---

### Scenario 2: IInitializeWithWindow Boilerplate Required for Desktop Apps

**Cause:** UWP file pickers (`FileOpenPicker`, `FileSavePicker`, `FolderPicker`) require a parent window handle (HWND) when used in Win32/desktop apps. Without calling `IInitializeWithWindow.Initialize()`, the picker has no owner window and will fail.
> Source: Issue [#1063](https://github.com/microsoft/WindowsAppSDK/issues/1063)

**Fix (WinUI 3 / Windows App SDK 1.8+):**

Starting with Windows App SDK 1.8, new picker constructors accept a `WindowId` directly, eliminating the interop boilerplate:
```csharp
// New simplified approach (SDK 1.8+)
var picker = new FileOpenPicker(appWindow.Id);
picker.FileTypeFilter.Add("*");
var file = await picker.PickSingleFileAsync();
```

**Fix (Older SDK versions):**
```csharp
var picker = new FileOpenPicker();
picker.FileTypeFilter.Add("*");

// Required interop for desktop apps
var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

var file = await picker.PickSingleFileAsync();
```

**Verify:** Picker dialog opens with the correct parent window and does not throw.

---

### Scenario 3: FileSavePicker Ignores DefaultFileExtension — Always Uses First Sorted Entry

**Cause:** The `DefaultFileExtension` property is ignored. `FileTypeChoices` is always sorted alphabetically, and the first sorted item is selected by default regardless of `DefaultFileExtension` or insertion order.
> Source: Issue [#5975](https://github.com/microsoft/WindowsAppSDK/issues/5975)

**Affected versions:** Windows App SDK 1.8.2 (1.8.251003001), unpackaged and packaged

**Repro:**
```csharp
var saveDialog = new FileSavePicker(AppWindow.Id)
{
    DefaultFileExtension = ".xml",
};
saveDialog.FileTypeChoices.TryAdd("TXT", [".txt"]);
saveDialog.FileTypeChoices.TryAdd("JSON", [".json"]);
saveDialog.FileTypeChoices.TryAdd("XML", [".xml"]);

var picker = await saveDialog.PickSaveFileAsync();
// Result: ".json" is selected as default (alphabetical first), not ".xml"
```

**Fix / Workaround:**
1. **Order your `FileTypeChoices` so the desired default is alphabetically first**, or add only the desired default type initially.
2. Use `SuggestedFileName` with the desired extension to hint the dialog:
```csharp
saveDialog.SuggestedFileName = "document.xml";
```
3. If a single file type is needed, register only that type in `FileTypeChoices`.

> ⚠️ This is a confirmed bug. No official SDK-level fix is available at this time.

---

### Scenario 4: FolderPicker Does Not Show WSL (Linux) Folders

**Cause:** `FolderPicker` does not enumerate WSL network locations (`\\wsl$\...`), while `FileOpenPicker` does. The `FolderPicker` appears to handle virtual/network shell namespace locations differently than `FileOpenPicker`. Temporarily, after navigating to a WSL path via `FileOpenPicker`, `FolderPicker` may briefly show WSL folders (cache/state inheritance) but loses them on subsequent opens.
> Source: Issue [#6284](https://github.com/microsoft/WindowsAppSDK/issues/6284)

**Affected versions:** Windows App SDK 1.8 (1.8.250916003), Windows 11

**Fix / Workaround:**
1. **Use `FileOpenPicker` first** to navigate to a WSL path, then immediately open `FolderPicker` — WSL folders may appear transiently.
2. **Use Win32 `IFileDialog` with `FOS_PICKFOLDERS`** flag for reliable WSL folder browsing:
```cpp
IFileDialog *pfd;
CoCreateInstance(CLSID_FileOpenDialog, NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&pfd));
DWORD dwOptions;
pfd->GetOptions(&dwOptions);
pfd->SetOptions(dwOptions | FOS_PICKFOLDERS);
pfd->Show(hwnd);
```
3. Alternatively, allow the user to paste the WSL UNC path (`\\wsl$\Ubuntu\...`) directly if your UI supports text entry.

> ⚠️ This is a confirmed open bug — no SDK fix available yet.

---

### Scenario 5: Storage Pickers Inherit App Language / RTL Layout Override

**Cause:** Setting `Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride` changes the language and layout direction (LTR/RTL) of storage picker dialogs. Picker dialogs respect the app-wide language override, which is undesired when the app uses RTL languages (e.g., Persian `fa-IR`) but the picker should remain LTR.
> Source: Issue [#6105](https://github.com/microsoft/WindowsAppSDK/issues/6105)

**Affected versions:** Windows App SDK 1.8.3 (1.8.251106002), packaged and unpackaged

**Fix / Workaround:**
1. **Temporarily reset `PrimaryLanguageOverride`** before opening the picker, then restore it:
```csharp
var savedLang = ApplicationLanguages.PrimaryLanguageOverride;
ApplicationLanguages.PrimaryLanguageOverride = "en-US"; // Force LTR for picker
var file = await picker.PickSingleFileAsync();
ApplicationLanguages.PrimaryLanguageOverride = savedLang; // Restore
```
2. **Use `ResourceContext`** with a specific qualifier instead of the global language override to localize the app UI without affecting system dialogs.

> ⚠️ This is an open issue — no official fix from Microsoft yet.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- For the Administrator crash (#2504): Some users report that creating pickers on the UI thread specifically (not from a background task) may reduce crash frequency, but this is not a reliable fix.
- For WSL folders (#6284): The transient visibility behavior suggests monitoring `FileOpenPicker` navigation state may help, but no programmatic API exists for this.

---

## References

- [Windows App SDK Storage Pickers documentation](https://learn.microsoft.com/en-us/windows/apps/develop/files/file-pickers)
- [Display UI objects in WinUI 3 (IInitializeWithWindow)](https://docs.microsoft.com/en-us/windows/apps/develop/ui-input/display-ui-objects)
- [FileOpenPicker API reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.windows.storage.pickers.fileopenpicker)
- [FileSavePicker API reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.windows.storage.pickers.filesavepicker)

---

**Updated:** 2025-07-17 | **Confidence:** 0.7
**Sources:** #1063, #2504, #5975, #6284, #6105
