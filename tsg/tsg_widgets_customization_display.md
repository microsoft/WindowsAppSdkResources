# Widget Customization and Display Failures - Windows Widgets Platform

**Keywords:** widget customization, template JSON, OnCustomizationRequested, widget board, add widget, widget list, Widgets panel

**Error Example:**
```
Widget customization card grows but template does not render.
OnCustomizationRequested callback never gets executed.
Widget "Add" button remains grayed out / inactive for first item in list.
```

---

## Quick Match

**You're seeing this if:**
- Widget customization panel opens but shows no content
- `OnCustomizationRequested` callback is never invoked by widget host
- First widget in alphabetical list cannot be added to widget board
- Widget "Add" button stays inactive until switching to another widget and back

→ Check scenarios below for your specific cause

---

## Related Issues

- [#3926](https://github.com/microsoft/WindowsAppSDK/issues/3926) - Widget Customization does not show template JSON (Status: Closed, area-Widgets)
- [#6140](https://github.com/microsoft/WindowsAppSDK/issues/6140) - Widget at top of list cannot be added until switching away and back (Status: Open, area-External)

---

## Scenarios & Solutions

### Scenario 1: Widget Customization Template Not Rendering

**Cause:** When selecting "✏️ Customize Widget" on a widget provider, the customization card area grows/expands but the customization template JSON is never displayed. Debugging reveals that the `OnCustomizationRequested` callback is never executed by the widget host. This means the Windows Widget host is not properly invoking the customization flow defined in the widget provider.
> Source: Issue author in [#3926](https://github.com/microsoft/WindowsAppSDK/issues/3926)

**Symptoms:**
- Following the official Microsoft documentation for [Implementing widget customization](https://learn.microsoft.com/en-us/windows/apps/develop/widgets/implement-widget-provider-cs#implementing-widget-customization)
- The customization card area expands when "Customize Widget" is selected
- The `OnCustomizationRequested` callback in the widget provider code is never triggered
- No template JSON content is rendered in the customization area

**Steps to reproduce:**
1. Implement a widget provider following the official Microsoft Learning documentation for widget customization
2. Register and deploy the widget
3. Open the Widgets panel and select "✏️ Customize Widget"
4. Observe: the card grows but no customization template appears

**Fix:**
1. Verify your widget provider correctly implements the `IWidgetProvider2` interface which includes `OnCustomizationRequested`
2. Ensure your widget manifest declares customization support correctly
3. Confirm the widget host version supports the customization API — this issue was observed with WinAppSDK 1.4-era widget platform
4. Check that you are returning a valid Adaptive Card JSON template from your customization handler
5. If the callback is still not invoked, this may be a platform-level bug in the Widget host. Monitor the issue for updates from Microsoft

**Environment:**
- Reported against the widget platform with WinAppSDK widget provider APIs
- Behavior observed in Windows 11 widget board

**Verify:**
Confirm `OnCustomizationRequested` is hit by adding a breakpoint or debug log:
```csharp
public void OnCustomizationRequested(WidgetCustomizationRequestedArgs args)
{
    Debug.WriteLine($"Customization requested for widget: {args.WidgetContext.Id}");
    // Return your customization template JSON
}
```

---

### Scenario 2: First Widget in Alphabetical List Cannot Be Added

**Cause:** On Windows 11 25H2 (Build 26220.7535), when a widget whose name starts with the letter "A" appears at the very top of the widget list, the "Add" button remains grayed out / inactive. The widget can only be added after switching to a different widget and then switching back. This is a UI state initialization bug in the Windows Widgets Board panel.
> Source: Issue author in [#6140](https://github.com/microsoft/WindowsAppSDK/issues/6140)

**Steps to reproduce:**
1. Open the Widgets panel on Windows 11 25H2
2. Click the "+" button to add a new widget
3. The first widget in the list (alphabetically, starting with "A") is automatically selected
4. Attempt to add the widget — the Add button stays gray/inactive
5. Select a different widget in the list
6. Return to the original widget
7. Now the Add button becomes active and the widget can be added

**Fix:**
1. This is a Windows Widgets Board platform bug (labeled `area-External`), not a widget provider issue
2. **User workaround:** Select any other widget in the list first, then switch back to the desired widget — the Add button will activate
3. **Developer impact:** If your widget name starts with "A" or appears first alphabetically, users may report inability to add it. Document this workaround in your widget's support materials
4. No code-level fix is available on the widget provider side — this requires a platform fix from Microsoft

**Environment:**
- OS: Windows 11 25H2 (Build 26220.7535)
- Widget Platform: Windows Widgets Board
- Reproducibility: Always

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- For Scenario 1: Ensure your widget provider DLL is correctly registered and the COM server is running. Some developers report that re-registering the package resolves callback issues (community reports, not confirmed in #3926)
- For Scenario 2: Renaming your widget to not start with "A" is a theoretical workaround, but this should not be necessary and the platform bug should be fixed (from #6140 discussion)

---

## References

- [Implementing Widget Customization - Microsoft Learn](https://learn.microsoft.com/en-us/windows/apps/develop/widgets/implement-widget-provider-cs#implementing-widget-customization)
- [Widget Provider Development - Microsoft Learn](https://learn.microsoft.com/en-us/windows/apps/develop/widgets/)
- [#3926 - Widget Customization does not show template JSON](https://github.com/microsoft/WindowsAppSDK/issues/3926)
- [#6140 - Widget at top of list cannot be added](https://github.com/microsoft/WindowsAppSDK/issues/6140)

---

**Updated:** 2025-07-17 | **Confidence:** 0.6
**Sources:** #3926, #6140, Microsoft Learn widget documentation
