# Error: "Missing Windows versions in bug template" - Bug Template Issue

**Keywords:** bug template, missing Windows versions, Windows App SDK, 26100, 26120, 26200

**Error Example:**
```
The bug template target Windows version is missing 26100, 26120, 26200
```

---

## Quick Match

**You're seeing this if:**
- You notice that the bug template for reporting issues in the Windows App SDK is missing Windows versions 26100, 26120, and 26200.
- You are using the bug template provided in the Windows App SDK repository.

→ Check scenarios below for your specific cause.

---

## Related Issues

- [#5534](https://github.com/microsoft/WindowsAppSDK/issues/5534) - The bug template target Windows version is missing versions 26100, 26120, 26200 (Status: Closed, Fixed in PR)

---

## Scenarios & Solutions

### Scenario 1: Missing Windows versions in the bug template

**Cause:** The bug template in the Windows App SDK repository was outdated and did not include Windows versions 26100, 26120, and 26200.
> Source: @RDMacLachlan [MSFT] in [#5534](https://github.com/microsoft/WindowsAppSDK/issues/5534)

**Fix:**
1. Update the bug template in the repository to include the missing Windows versions.
2. Ensure the updated template is merged into the main branch.

> ✅ Confirmed by: @RDMacLachlan [MSFT] in issue comments.

**Verify:** Check the latest bug template in the repository to confirm that versions 26100, 26120, and 26200 are now listed.

---

# Error: "C# template missing file-scoped namespace changes" - Visual Studio Templates Issue

**Keywords:** C# template, file-scoped namespaces, Visual Studio 17.14 Preview 3, Windows App SDK templates

**Error Example:**
```
I just downloaded VS 17.14 Preview 3, I see some of the template changes, but not all of them. The file-scoped namespaces for instance aren't there...? But I see them in `main`.
```

---

## Quick Match

**You're seeing this if:**
- You are using Visual Studio 17.14 Preview 3 or later.
- You notice that the C# project templates for the Windows App SDK do not include file-scoped namespaces, even though they appear in the repository's main branch.

→ Check scenarios below for your specific cause.

---

## Related Issues

- [#5350](https://github.com/microsoft/WindowsAppSDK/issues/5350) - VS 17.14 Preview 3 C# template missing file-scoped namespace changes (Status: Open)

---

## Scenarios & Solutions

### Scenario 1: Template changes not fully shipped by Visual Studio

**Cause:** The updated C# templates with file-scoped namespaces were not yet shipped by the Visual Studio team, even though they appear in the repository's main branch.
> Source: @haonanttt [MSFT] in [#5350](https://github.com/microsoft/WindowsAppSDK/issues/5350)

**Fix:**
1. Verify the installed version of the Windows App SDK template extension:
   - Navigate to "Extensions -> Manage Extensions -> Installed."
   - Search for `template` and locate the Windows App SDK C# VS2022 Templates.
2. Update to the latest version of Visual Studio Preview and ensure the templates are updated.

> ✅ Confirmed by: @michael-hawker [MSFT] in issue comments.

**Verify:** Create a new project using the updated templates and check if file-scoped namespaces are included.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- Check the Visual Studio settings for block vs. file-scoped namespaces under "Options -> Text Editor -> C# -> Code Style settings."
  > Source: @michael-hawker [MSFT] in [#5350](https://github.com/microsoft/WindowsAppSDK/issues/5350)

---

# Error: "How to use XAML Island in Windows App SDK 1.7 for XAML hosting?" - XAML Island Usage

**Keywords:** XAML Island, Windows App SDK 1.7, XAML hosting, DesktopChildSiteBridge, XamlIsland Class

**Error Example:**
```
What’s the correct way to host WinUI 3 content (XAML) using XAML Islands with the Windows App SDK 1.7?
```

---

## Quick Match

**You're seeing this if:**
- You are using Windows App SDK 1.7 and want to host WinUI 3 content using XAML Islands.
- You are looking for examples or documentation on how to implement XAML hosting.

→ Check scenarios below for your specific cause.

---

## Related Issues

- [#5625](https://github.com/microsoft/WindowsAppSDK/issues/5625) - How to use XAML Island in Windows App SDK 1.7 for XAML hosting? Any sample code? (Status: Closed)

---

## Scenarios & Solutions

### Scenario 1: Using XAML Islands with DesktopChildSiteBridge

**Cause:** Developers may not be familiar with the updated process for hosting XAML content using XAML Islands in Windows App SDK 1.7.
> Source: @DarranRowe in [#5625](https://github.com/microsoft/WindowsAppSDK/issues/5625)

**Fix:**
1. Ensure your application creates an instance of `App` as a prerequisite.
2. Use the `DesktopChildSiteBridge` class to create a connection to the parent window:
   - Call `DesktopChildSiteBridge.CreateWithDispatcherQueue`.
   - Use the `Connect()` method to establish the connection.
3. Create a `XamlIsland` instance and configure it as needed.
4. Set the `ResizePolicy` to `ContentSizePolicy.ResizeContentToParentWindow`.
5. Call the `Show()` method to display the XAML content.

> ✅ Confirmed by: @castorix, @Ajith-GS in issue comments.

**Verify:** Run the application and confirm that the XAML content is hosted correctly using XAML Islands.

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- Explore the [DesktopWindowXamlSource Class](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.desktopwindowxamlsource?view=windows-app-sdk-1.7) as an alternative for XAML hosting.
  > Source: @castorix in [#5625](https://github.com/microsoft/WindowsAppSDK/issues/5625)

---

## References

- [Windows App SDK Samples - XAML Islands](https://github.com/microsoft/WindowsAppSDK-Samples/tree/main/Samples/Islands)
- [DesktopChildSiteBridge Class Documentation](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.content.desktopchildsitebridge?view=windows-app-sdk-1.7)
- [XamlIsland Class Documentation](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.xamlisland?view=windows-app-sdk-1.7)

---

**Updated:** 2026-03-17 | **Confidence:** 0.9
**Sources:** [#5534](https://github.com/microsoft/WindowsAppSDK/issues/5534), [#5350](https://github.com/microsoft/WindowsAppSDK/issues/5350), [#5625](https://github.com/microsoft/WindowsAppSDK/issues/5625)