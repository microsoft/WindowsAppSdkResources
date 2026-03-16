# API Gaps: Unpackaged App Support, Storage & Package Management - Windows App SDK

**Keywords:** ApplicationData, unpackaged, AppInfo, package identity, StorageFolder, PackageVolume, FindPackage, content type, file handler, file association, manifest, LOCALAPPDATA

**Error Example:**
```
// ApplicationData.Current fails for unpackaged apps
Windows.Storage.ApplicationData.Current → throws (no package identity)

// AppInfo unavailable for unpackaged
Windows.ApplicationModel.AppInfo → only works for packaged apps

// PackageVolume missing APIs
PackageVolume.FindPackages() → method not available in WASDK PackageVolume

// File handler declaration limitation
Can only declare file type extensions in manifest, not MIME content types
```

---

## Quick Match

**You're seeing this if:**
- `ApplicationData.Current` fails in an unpackaged (non-MSIX) app
- Need app metadata (display name, icon, etc.) in an unpackaged app
- WASDK `PackageVolume` is missing `FindPackage`-related APIs available in the platform type
- Want to register as a default file handler by MIME content type instead of individual file extensions

→ Check scenarios below for your specific cause

---

## Related Issues

- [#2639](https://github.com/microsoft/WindowsAppSDK/issues/2639) - Microsoft.Storage.ApplicationData proposal (Status: Open, In PR, area-ApplicationData)
- [#1083](https://github.com/microsoft/WindowsAppSDK/issues/1083) - AppInfo class for unpackaged apps (Status: Closed, feature proposal)
- [#27](https://github.com/microsoft/WindowsAppSDK/issues/27) - Specifying as default file handler with content type instead of filetype (Status: Open, area-Activation, feature proposal)
- [#6222](https://github.com/microsoft/WindowsAppSDK/issues/6222) - WASDK PackageVolume missing APIs related to FindPackageXXX (Status: Open, area-PackageManagement)

---

## Scenarios & Solutions

### Scenario 1: ApplicationData Not Available for Unpackaged Apps

**Cause:** `Windows.Storage.ApplicationData.Current` requires both package identity AND running in AppContainer. `Windows.Management.Core.ApplicationDataManager.CreateForPackageFamily` requires package identity AND MediumIL or higher. Unpackaged apps have no access to the ApplicationData API and must manually manage `%LOCALAPPDATA%` paths. Additionally, the existing API has performance overhead (requiring `StorageFolder` with no direct `.Path` access), deprecated features (e.g., `RoamingFolder`), and no synchronous alternatives to async methods.
> Source: Issue author in [#2639](https://github.com/microsoft/WindowsAppSDK/issues/2639)

**Current workaround for unpackaged apps:**
1. Use standard filesystem paths directly instead of `ApplicationData`:
```csharp
// Equivalent to ApplicationData.LocalFolder for unpackaged apps
string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
string appDataPath = Path.Combine(localAppData, "YourPublisher", "YourProduct");
Directory.CreateDirectory(appDataPath);

// Equivalent to ApplicationData.TemporaryFolder
string tempPath = Path.Combine(localAppData, "Temp");

// Equivalent to ApplicationData.LocalSettings (use registry)
// HKCU\SOFTWARE\YourPublisher\YourProduct
using var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\YourPublisher\YourProduct");
key.SetValue("Setting1", "Value1");
```

2. Reference mapping from the proposal (packaged → unpackaged equivalents):

| Packaged API | Unpackaged Equivalent |
|---|---|
| `ApplicationData.LocalCacheFolder` | `%LOCALAPPDATA%\<publisher>\<product>` |
| `ApplicationData.LocalFolder` | `%LOCALAPPDATA%\<publisher>\<product>` |
| `ApplicationData.MachineFolder` | `%ProgramData%\<publisher>\<product>` |
| `ApplicationData.TemporaryFolder` | `%LOCALAPPDATA%\Temp` |
| `ApplicationData.LocalSettings` | `HKCU\SOFTWARE\<publisher>\<product>` |

**Upcoming fix:** The `Microsoft.Storage.ApplicationData` API is being developed (Status: In PR) to provide a unified API for both packaged and unpackaged apps. The new API will:
- Support packaged and unpackaged applications with a single API surface
- Provide direct filesystem path access without `StorageFolder` overhead
- Offer synchronous equivalents to async methods (e.g., `Clear()` alongside `ClearAsync()`)
- Deprecate obsolete features like `RoamingFolder`
> Source: API proposal in [#2639](https://github.com/microsoft/WindowsAppSDK/issues/2639)

**Verify:** Check for the `Microsoft.Storage.ApplicationData` namespace in future WinAppSDK releases.

---

### Scenario 2: AppInfo Unavailable for Unpackaged Apps

**Cause:** The [Windows.ApplicationModel.AppInfo](https://docs.microsoft.com/en-us/uwp/api/Windows.ApplicationModel.AppInfo) class only works for packaged apps because it reads information from the app package manifest. Unpackaged apps have no manifest and therefore cannot use this API to get app metadata (display name, description, logo, etc.).
> Source: Issue author in [#1083](https://github.com/microsoft/WindowsAppSDK/issues/1083)

**Workaround:**
1. For unpackaged apps, read app information from the executable's file version info:
```csharp
using System.Diagnostics;

var exePath = Environment.ProcessPath;
var versionInfo = FileVersionInfo.GetVersionInfo(exePath);

string appName = versionInfo.ProductName ?? "Unknown";
string appVersion = versionInfo.ProductVersion ?? "0.0.0";
string company = versionInfo.CompanyName ?? "Unknown";
string description = versionInfo.FileDescription ?? "";
```
2. For icon retrieval in unpackaged apps, use Win32 APIs:
```csharp
[DllImport("shell32.dll", CharSet = CharSet.Auto)]
static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);
```
3. The feature proposal requests that WinAppSDK update `AppInfo` to pull from file details for unpackaged apps instead of requiring a manifest

**Status:** Closed — the feature request was acknowledged but the specific implementation status is unclear. Use the Win32/BCL workarounds above.

---

### Scenario 3: Cannot Declare Default File Handler by Content Type

**Cause:** The current app manifest system only supports declaring file type associations by specific file extensions (e.g., `.txt`, `.md`, `.json`). There is no way to declare an app as a handler for a MIME content type (e.g., `text/*`), which would automatically cover all file extensions of that type. This forces developers of text editors, media players, and similar broad-format apps to exhaustively list every possible file extension.
> Source: Issue author in [#27](https://github.com/microsoft/WindowsAppSDK/issues/27)

**Impact:**
- Text editor apps cannot easily become the default handler for all text files
- Declaring every possible file extension is impractical and incomplete
- `StorageFile` operations like renaming fail for undeclared file types
- `StorageFile.GetFileAsync()` fails for unsupported file types
- `Launcher.LaunchFileAsync()` fails if the target file type is undeclared
- Users cannot choose a UWP/WinUI app as default handler for undeclared file types (unlike Win32 apps)

**Workaround:**
1. Declare as many file extensions as practical in your app manifest:
```xml
<uap:Extension Category="windows.fileTypeAssociation">
    <uap:FileTypeAssociation Name="textfiles">
        <uap:SupportedFileTypes>
            <uap:FileType>.txt</uap:FileType>
            <uap:FileType>.md</uap:FileType>
            <uap:FileType>.log</uap:FileType>
            <uap:FileType>.csv</uap:FileType>
            <!-- Add more as needed -->
        </uap:SupportedFileTypes>
    </uap:FileTypeAssociation>
</uap:Extension>
```
2. For unpackaged Win32 apps, use the traditional registry-based file association which has no such limitation
3. Consider using a desktop extension / full-trust component for broader file handling capability

**Status:** Open — this is a long-standing feature proposal (filed May 2020). No confirmed timeline for content-type-based file handler declaration.

---

### Scenario 4: WASDK PackageVolume Missing FindPackage APIs

**Cause:** The WASDK `PackageVolume` type is missing APIs related to `FindPackageXXX` methods that are available on the platform's `Windows.Management.Deployment.PackageVolume` type. This means developers using WASDK's package management APIs cannot enumerate or search for packages on a volume.
> Source: Issue author in [#6222](https://github.com/microsoft/WindowsAppSDK/issues/6222)

**Workaround:**
1. Use the platform `Windows.Management.Deployment.PackageVolume` APIs directly instead of the WASDK equivalent:
```csharp
using Windows.Management.Deployment;

var packageManager = new PackageManager();
var volumes = packageManager.FindPackageVolumes();
foreach (var volume in volumes)
{
    // Platform API has FindPackages methods
    var packages = volume.FindPackages();
    // Process packages...
}
```
2. This bypasses the WASDK abstraction but provides the needed functionality

**Status:** Open — filed against WinAppSDK 2.0 Experimental 4 (2.0.0-experimental4). The WASDK team may add these APIs in a future release.

**Environment:**
- WinAppSDK 2.0 Experimental 4 (2.0.0-experimental4)
- Packaged (MSIX)
- Windows 11 24H2 LTSC (26100)

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- For ApplicationData (#2639): Some developers use a custom `ApplicationData`-like wrapper class that detects packaging state and routes to either the platform API or filesystem paths automatically (community pattern, not officially recommended)
- For file handler content type (#27): Using a Win32 desktop extension bridge to handle file associations for UWP/WinUI apps may provide broader file type coverage (from community discussion in #27)

---

## References

- [Windows.Storage.ApplicationData - UWP API](https://docs.microsoft.com/uwp/api/windows.storage.applicationdata)
- [Windows.ApplicationModel.AppInfo - UWP API](https://docs.microsoft.com/en-us/uwp/api/Windows.ApplicationModel.AppInfo)
- [File Type Associations - MSIX](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/desktop-to-uwp-extensions#file-type-associations)
- [PackageVolume - Windows.Management.Deployment](https://learn.microsoft.com/en-us/uwp/api/windows.management.deployment.packagevolume)
- [#2639 Proposal - Microsoft.Storage.ApplicationData](https://github.com/microsoft/WindowsAppSDK/issues/2639)

---

**Updated:** 2025-07-17 | **Confidence:** 0.7
**Sources:** #2639, #1083, #27, #6222, UWP/WinAppSDK API documentation
