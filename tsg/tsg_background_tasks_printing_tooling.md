# Background Task Crashes, Print Preview Dark Theme & VS Tooling - Windows App SDK

**Keywords:** UniversalBGTask, STOWED_EXCEPTION, background task, crash, 0x80004005, 0x80004002, 0x800706ba, 0x80080204, 0xC00CE169, ACCESS_VIOLATION, PrintPreview, dark theme, RequestedTheme, empty preview, Visual Studio 2022, add page, new item

**Error Example:**
```
STOWED_EXCEPTION_80004005_Microsoft.Windows.ApplicationModel.Background.UniversalBGTask.dll
STOWED_EXCEPTION_80004002_Microsoft.Windows.ApplicationModel.Background.UniversalBGTask.dll
STOWED_EXCEPTION_800706ba_Microsoft.Windows.ApplicationModel.Background.UniversalBGTask.dll

// Print preview
PrintPreview window shows blank/empty content when RequestedTheme = Dark

// VS 2022 tooling
No option to add a new Page in VS 2022 Preview 3 with WinAppSDK 0.8
```

---

## Quick Match

**You're seeing this if:**
- Background task crashes with `STOWED_EXCEPTION` in `UniversalBGTask.dll`
- Partner Center shows thousands of daily crashes from background task execution
- Print preview window displays empty/blank content with dark theme
- Cannot add new WinUI Page items in Visual Studio 2022

→ Check scenarios below for your specific cause

---

## Related Issues

- [#5870](https://github.com/microsoft/WindowsAppSDK/issues/5870) - UniversalBGTask crashes with STOWED_EXCEPTION (Status: Open, area-BackgroundTask, 36 comments)
- [#6086](https://github.com/microsoft/WindowsAppSDK/issues/6086) - PrintPreview displays empty value when RequestedTheme is dark (Status: Open)
- [#1236](https://github.com/microsoft/WindowsAppSDK/issues/1236) - Unable to Create new Pages in VS 2022 Preview 3 (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: UniversalBGTask Crashes with STOWED_EXCEPTION (High Volume)

**Cause:** Background tasks implemented using the WinAppSDK `UniversalBGTask` infrastructure crash with `STOWED_EXCEPTION` errors during `IBackgroundTask.Run`. The crashes occur in `Microsoft.Windows.ApplicationModel.Background.UniversalBGTask.dll` and affect Store-published apps at massive scale (10,000+ crashes per day reported). In WinAppSDK 1.7, similar crashes manifested as `ACCESS_VIOLATION` errors (discussed in [microsoft-ui-xaml#10769](https://github.com/microsoft/microsoft-ui-xaml/issues/10769)). After upgrading to WinAppSDK 1.8, the error code changed to `STOWED_EXCEPTION` but the crash volume remained similar.
> Source: Issue author in [#5870](https://github.com/microsoft/WindowsAppSDK/issues/5870)

**Error codes observed:**
- `STOWED_EXCEPTION_80004005` (E_FAIL) — General failure during task execution
- `STOWED_EXCEPTION_80004002` (E_NOINTERFACE) — `hresult_no_interface` thrown, suggesting COM interface query failure
- `STOWED_EXCEPTION_800706ba` (RPC_S_SERVER_UNAVAILABLE) — RPC server unavailable during task run
- `0x80080204` and `0xC00CE169` — Additional error codes referenced in issue metadata

**Common stack trace pattern:**
```
UniversalBGTask.dll!winrt::hresult_error::originate
UniversalBGTask.dll!...::Task::Run
biwinrt.dll!CBackgroundTaskInstance::RunInternal
biwinrt.dll!CBackgroundTaskInstance::Run
twinapi.appcore.dll!BackgroundTaskWrapper::ThreadProc
```

**Key observations:**
- Crashes happen across multiple Windows versions: Windows 11 10.0.26100, 10.0.22631, Windows 10 10.0.19045
- The crash count exceeds the "devices affected" count, indicating repeated crashes on the same machines
- Affects all apps published in the Microsoft Store that use background tasks
- Issue persisted across WinAppSDK 1.7 (ACCESS_VIOLATION) and 1.8 (STOWED_EXCEPTION)

**Workaround (limited):**
1. Wrap your background task `Run` implementation in a comprehensive try-catch to prevent unhandled exceptions:
```csharp
public sealed class MyBackgroundTask : IBackgroundTask
{
    public void Run(IBackgroundTaskInstance taskInstance)
    {
        var deferral = taskInstance.GetDeferral();
        try
        {
            // Your task logic here
        }
        catch (Exception ex)
        {
            // Log the error - don't let it propagate
            Debug.WriteLine($"Background task error: {ex.HResult:X} - {ex.Message}");
        }
        finally
        {
            deferral.Complete();
        }
    }
}
```
2. Note: The `E_NOINTERFACE` (0x80004002) crash occurs inside `UniversalBGTask.dll` itself (not user code), suggesting a platform-level COM activation failure that cannot be caught by user exception handlers
3. The `RPC_S_SERVER_UNAVAILABLE` (0x800706ba) error suggests the background task host process may lose connection to the main application or a required system service
4. Monitor [#5870](https://github.com/microsoft/WindowsAppSDK/issues/5870) for updates — this is actively discussed with 36 comments

**Status:** Open — no confirmed fix. This is a high-impact issue affecting Store-published apps.

**Environment:**
- WinAppSDK 1.8.1 (1.8.250916003)
- Packaged (MSIX)
- Multiple Windows versions (10 and 11)

---

### Scenario 2: Print Preview Shows Empty Content with Dark Theme

**Cause:** When `RequestedTheme` is set to `ApplicationTheme.Dark` in a WinUI application, the print preview window displays blank/empty content. The same printing code works correctly when the theme is set to Light. The issue is that the print preview rendering inherits the dark theme, causing text (likely white/light colored) to be rendered on a white print preview background, making it invisible.
> Source: Issue author in [#6086](https://github.com/microsoft/WindowsAppSDK/issues/6086)

**Steps to reproduce:**
1. Set `this.RequestedTheme = ApplicationTheme.Dark` in `App.xaml.cs`
2. Implement printing with `PrintDocument`
3. Create preview pages using `StackPanel` with `TextBlock` elements
4. Open print preview — text is invisible

**Reproduction code pattern:**
```csharp
// In App.xaml.cs
public App()
{
    this.RequestedTheme = ApplicationTheme.Dark; // Causes issue
    InitializeComponent();
}

// In printing handler
private void OnPaginate(object sender, PaginateEventArgs e)
{
    var page = new StackPanel { Margin = new Thickness(20) };
    page.Children.Add(new TextBlock
    {
        Text = "Hello, this is a print test!",
        FontSize = 24
    });
    _printDoc.SetPreviewPage(1, page);
}
```

**Fix:**
1. Explicitly set the print preview page elements to use Light theme and dark text colors:
```csharp
private void OnPaginate(object sender, PaginateEventArgs e)
{
    var page = new StackPanel
    {
        Margin = new Thickness(20),
        RequestedTheme = ElementTheme.Light,  // Force light theme for print
        Background = new SolidColorBrush(Colors.White)
    };
    page.Children.Add(new TextBlock
    {
        Text = "Hello, this is a print test!",
        FontSize = 24,
        Foreground = new SolidColorBrush(Colors.Black)  // Explicit dark text
    });
    _printDoc.SetPreviewPage(1, page);
}
```
2. Alternatively, set `RequestedTheme = ElementTheme.Light` on the root element of each print preview page to ensure print content is always rendered with light theme colors regardless of the app theme

**Verify:** Open print preview with dark theme active — text should now be visible on the white preview background.

**Status:** Open — no official fix from WinAppSDK team yet.

---

### Scenario 3: Cannot Add New WinUI Pages in Visual Studio 2022

**Cause:** In Visual Studio 2022 Preview 3 with WinAppSDK 0.8, the "Add New Item" dialog did not include an option to add a new WinUI Page. The WinUI item templates were not properly registered in early VS 2022 previews.
> Source: Issue author in [#1236](https://github.com/microsoft/WindowsAppSDK/issues/1236)

**Workaround (historical):**
1. Use Visual Studio 2019 to add the new page, then continue working in VS 2022
2. Manually create the `.xaml` and `.xaml.cs` files following the WinUI page pattern:

```xml
<!-- NewPage.xaml -->
<Page
    x:Class="YourApp.NewPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
    </Grid>
</Page>
```

```csharp
// NewPage.xaml.cs
namespace YourApp
{
    public sealed partial class NewPage : Page
    {
        public NewPage()
        {
            this.InitializeComponent();
        }
    }
}
```

**Fix:**
1. **Update Visual Studio 2022** to a recent stable release — this issue was specific to early Preview 3 builds with WinAppSDK 0.8
2. Ensure the **Windows App SDK** extension/workload is installed in VS 2022
3. Modern versions of VS 2022 with WinAppSDK 1.x+ include proper WinUI item templates

> ✅ Confirmed resolved: Issue is closed, indicating this was fixed in subsequent VS 2022 updates

**Environment:**
- Visual Studio 2022 Preview 3 (historical)
- WinAppSDK 0.8

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- For background task crashes (#5870): Some developers report that reducing background task frequency or adding retry logic with exponential backoff reduces the visible crash rate, though the underlying platform issue remains (community reports in #5870)
- For background task crashes (#5870): Disabling background tasks entirely as a temporary measure while waiting for a fix may be appropriate if the crashes impact app store ratings (from community discussion)
- For print preview (#6086): Some developers report that creating the print preview content in a separate `Frame` with explicit Light theme avoids the issue (not officially confirmed)

---

## References

- [Background Tasks - WinAppSDK Documentation](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/applifecycle/background-tasks)
- [PrintDocument Class - WinUI](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.printing.printdocument)
- [WinUI Item Templates - Visual Studio](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/winui-project-templates-in-visual-studio)
- [Related: microsoft-ui-xaml#10769 - Background task ACCESS_VIOLATION](https://github.com/microsoft/microsoft-ui-xaml/issues/10769)

---

**Updated:** 2025-07-17 | **Confidence:** 0.6
**Sources:** #5870, #6086, #1236, Microsoft Learn documentation
