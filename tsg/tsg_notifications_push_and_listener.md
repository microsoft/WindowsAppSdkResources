# Notification Issues — Push Notifications, Progress Data, and UserNotificationListener

**Keywords:** AppNotification, push notification, unpackaged, COMException, 0x80070490, Element not found, UserNotificationListener, NotificationChanged, AppNotificationProgressData, IsIndeterminate, indeterminate progress bar, toast notification

**Error Example:**
```
System.Runtime.InteropServices.COMException (0x80070490): Element not found
   at ABI.WinRT.Interop.EventSource`1.Subscribe(TDelegate handler)
   at Windows.UI.Notifications.Management.UserNotificationListener.add_NotificationChanged(...)
```

---

## Quick Match

**You're seeing this if:**
- Error contains `0x80070490` ("Element not found") when subscribing to notification events
- Push notifications fail in unpackaged or non-Store apps
- Cannot create an indeterminate progress bar in toast notifications via `AppNotificationProgressData`
- `UserNotificationListener.NotificationChanged` throws `COMException` in unpackaged apps
- Platform: Windows App SDK, WinUI 3, unpackaged or self-contained apps

→ Check scenarios below for your specific cause

---

## Related Issues

- [#334](https://github.com/microsoft/WindowsAppSDK/issues/334) — Push Notifications for Unpackaged Apps and Non-Store Apps (Status: Closed — implemented)
- [#2231](https://github.com/microsoft/WindowsAppSDK/issues/2231) — Add IsIndeterminate to AppNotificationProgressData (Status: Open — In PR)
- [#6172](https://github.com/microsoft/WindowsAppSDK/issues/6172) — COMException 0x80070490 when subscribing to UserNotificationListener.NotificationChanged (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: Push Notifications Not Available for Unpackaged / Non-Store Apps

**Cause:** Historically, only Microsoft Store-distributed UWP/MSIX packaged apps could use Windows Push Notifications due to an explicit dependency on Store identity. Non-store and unpackaged Win32 apps had no way to use the push notification infrastructure and had to maintain their own socket connections.
> Source: Issue [#334](https://github.com/microsoft/WindowsAppSDK/issues/334)

**Status:** ✅ **Resolved** — Windows App SDK now supports push notifications for all app types.

**Supported configurations:**
| Packaging | App Types |
|-----------|-----------|
| ✅ Packaged apps | ✅ WPF, Win32 (C++), WinForms, Console |
| ✅ Unpackaged apps | ⚠️ Cross-platform (Electron, MAUI, React Native) — partial |

**Fix — Using Push Notifications in Unpackaged Apps:**
1. **Register your app** with Azure Notification Hubs or WNS (Windows Notification Services) using the Windows App SDK push notification APIs.
2. **Use `AppNotificationManager`** for local and push notifications:
```csharp
// Register for push notifications (unpackaged app)
var manager = AppNotificationManager.Default;
manager.NotificationInvoked += OnNotificationInvoked;
manager.Register();

// Request a push notification channel
var channelResult = await PushNotificationManager.Default
    .CreateChannelAsync(yourAzureAppId);
```
3. For unpackaged apps, ensure you have the **Windows App SDK Runtime** installed or use a self-contained deployment.
4. Refer to the [Push notifications overview](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/notifications/push-notifications/) for Azure setup.

**Verify:** Send a test push notification and confirm it arrives in your unpackaged app.

---

### Scenario 2: Cannot Set Indeterminate Progress Bar in Toast Notifications

**Cause:** The `AppNotificationProgressData` API does not expose an `IsIndeterminate` property. In raw XML, setting `value="indeterminate"` on a `<progress>` element creates a loading animation, but this behavior cannot be achieved through the `AppNotificationProgressData` class — the `Value` property only accepts `double` values between 0.0 and 1.0.
> Source: Issue [#2231](https://github.com/microsoft/WindowsAppSDK/issues/2231)

**Status:** 🔄 **In PR** — A fix is being developed (labeled "Status: In PR").

**Current limitation:**
```csharp
// This API does NOT support indeterminate state
var progressData = new AppNotificationProgressData(1);
progressData.Value = 0.5; // Only accepts 0.0 - 1.0
progressData.Status = "Downloading...";
// No IsIndeterminate property available
```

**Workaround — Use raw XML notification:**
```csharp
var xml = @"
<toast>
  <visual>
    <binding template='ToastGeneric'>
      <text>Downloading...</text>
      <progress value='indeterminate' title='Please wait' status='Processing...'/>
    </binding>
  </visual>
</toast>";

var doc = new Windows.Data.Xml.Dom.XmlDocument();
doc.LoadXml(xml);
var notification = new ToastNotification(doc);
ToastNotificationManager.CreateToastNotifier().Show(notification);
```

Alternatively, use the **Windows Community Toolkit** notification builder which supports indeterminate state:
```csharp
// Using CommunityToolkit.WinUI.Notifications
new ToastContentBuilder()
    .AddText("Processing...")
    .AddProgressBar(title: "Please wait", status: "Working...",
        value: AdaptiveProgressBarValue.Indeterminate)
    .Show();
```

**Verify:** Toast notification shows an animated indeterminate progress bar.

---

### Scenario 3: COMException 0x80070490 When Subscribing to UserNotificationListener.NotificationChanged

**Cause:** In an unpackaged, self-contained WinUI 3 app, subscribing to `UserNotificationListener.NotificationChanged` throws `COMException` with HRESULT `0x80070490` ("Element not found"). Other `UserNotificationListener` APIs work correctly — `UserNotificationListener.Current` is accessible, `RequestAccessAsync()` returns `Allowed`, and `GetNotificationsAsync()` succeeds. Only the event subscription fails.
> Source: Issue [#6172](https://github.com/microsoft/WindowsAppSDK/issues/6172)

**Affected versions:** Windows App SDK 1.8.4 (1.8.260101001), Windows 11 24H2, unpackaged self-contained apps

**Repro:**
```csharp
var listener = UserNotificationListener.Current;
var access = await listener.RequestAccessAsync(); // Returns Allowed ✅
var notifications = await listener.GetNotificationsAsync(
    NotificationKinds.Toast); // Works ✅

listener.NotificationChanged += Listener_NotificationChanged; // Throws ❌
// COMException (0x80070490): Element not found
```

**Repro project:** https://github.com/koiseka/notificationchangedexample-repro

**Fix / Workaround:**
1. **Package the app as MSIX** — the `NotificationChanged` event subscription requires package identity to properly register the event handler with the notification platform.
2. **Use polling instead of events** as a workaround for unpackaged apps:
```csharp
// Poll for notification changes every N seconds
var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
timer.Tick += async (s, e) =>
{
    var listener = UserNotificationListener.Current;
    var notifications = await listener.GetNotificationsAsync(
        NotificationKinds.Toast);
    // Compare with previous list to detect changes
    ProcessNotificationChanges(notifications);
};
timer.Start();
```
3. If MSIX packaging is an option, add package identity using [Sparse Package](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/grant-identity-to-nonpackaged-apps) to enable event registration without full MSIX deployment.

> ⚠️ This is an open issue. The `NotificationChanged` event does not support unpackaged apps.

**Verify:** Confirm that `NotificationChanged` fires after packaging the app, or that the polling approach correctly detects notification changes.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- For #6172: Granting identity via External Location registration (`AddPackageByUriAsync` with `ExternalLocationUri`) may enable event subscription without full MSIX packaging, but this has not been tested with `UserNotificationListener`.
- For #2231: Until the official `IsIndeterminate` API ships, the raw XML approach is the most reliable workaround for indeterminate progress bars in toast notifications.

---

## References

- [Push notifications overview — Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/notifications/push-notifications/)
- [App notifications overview — Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/notifications/app-notifications/)
- [Toast content XML schema — progress element](https://learn.microsoft.com/en-us/windows/apps/design/shell/tiles-and-notifications/toast-schema#adaptiveprogressbarvalue)
- [UserNotificationListener API](https://learn.microsoft.com/en-us/uwp/api/windows.ui.notifications.management.usernotificationlistener)
- [Grant identity to non-packaged apps (Sparse Package)](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/grant-identity-to-nonpackaged-apps)

---

**Updated:** 2025-07-17 | **Confidence:** 0.7
**Sources:** #334, #2231, #6172
