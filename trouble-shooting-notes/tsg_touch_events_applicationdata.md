# Error: "Touch events stop working in a WPF packaged app after using ApplicationData.Current"

**Keywords:** touch events, ApplicationData.Current, WPF, packaged app, MSIX, Windows App SDK

**Error Example:**
```
Touch events stop working in a WPF packaged app after using ApplicationData.Current
```

---

## Quick Match

**You're seeing this if:**
- Touch events stop working in your WPF packaged app
- You're using `ApplicationData.Current` or related APIs like `ApplicationData.GetForUser`, `ApplicationData.GetForPackageFamily`, or `ApplicationData.GetDefault`
- Platform: Windows 10 version 22H2 (19045, 2022 Update) or similar

→ Check scenarios below for your specific cause

---

## Related Issues

- [#5167](https://github.com/microsoft/WindowsAppSDK/issues/5167) - Touch events stop working in a WPF packaged app after using ApplicationData.Current (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: Accessing `ApplicationData.Current` too early in the application lifecycle

**Cause:** Accessing `ApplicationData.Current` or related APIs during the initialization phase (e.g., in the constructor of `App.xaml.cs` or `MainWindow`) can interfere with touch event handling in WPF packaged apps. This may be related to COM initialization issues on the thread.
> Source: @yeelam-gordon in [#5167](https://github.com/microsoft/WindowsAppSDK/issues/5167)

**Fix:**
1. Delay the call to `ApplicationData.Current` or related APIs until after the initialization phase.
   - For example, use `Task.Run` or access it in a later event or property.
2. Avoid calling `ApplicationData.Current` in the constructor of `App.xaml.cs` or `MainWindow`.

> ✅ Confirmed by: @yeelam-gordon, @asierpn in issue comments

**Verify:** Ensure touch events work as expected after delaying the call to `ApplicationData.Current`.

---

### Scenario 2: Potential COM initialization issue

**Cause:** Using COM-related APIs (like `ApplicationData.Current`) on a thread before COM is properly initialized can cause unexpected behavior, including issues with touch events.
> Source: @DrusTheAxe in [#5167](https://github.com/microsoft/WindowsAppSDK/issues/5167)

**Fix:**
1. Ensure that COM is properly initialized on the thread before calling `ApplicationData.Current`.
2. Refer to the related issue [dotnet/wpf#2393](https://github.com/dotnet/wpf/issues/2393) for additional context and potential workarounds.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- "Try the workaround mentioned in [dotnet/wpf#2393](https://github.com/dotnet/wpf/issues/2393)." (from @ekalchev in [#5167](https://github.com/microsoft/WindowsAppSDK/issues/5167))

---

## References

- [Official docs](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/preferences?view=net-maui-9.0&tabs=windows)
- [API docs](https://learn.microsoft.com/en-us/uwp/api/windows.storage.applicationdata)
- [dotnet/wpf#2393](https://github.com/dotnet/wpf/issues/2393)

---

**Updated:** 2026-03-17 | **Confidence:** 0.8
**Sources:** [#5167](https://github.com/microsoft/WindowsAppSDK/issues/5167), [dotnet/wpf#2393](https://github.com/dotnet/wpf/issues/2393)