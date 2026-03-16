# AppContainer for Win32 Apps & Named Object Sharing Between Packaged/Win32

**Keywords:** AppContainer, Win32, PartialTrustApplication, partial trust, sandboxing, named objects, security descriptor, PackageFamilyName, PFN, Shell_NotifyIcon, tray icon, MSIX, runFullTrust, permissiveLearningMode

---

## Quick Match

**You're seeing this if:**
- You want to run a Win32/WinUI 3 MSIX app in an AppContainer sandbox
- `Shell_NotifyIcon` returns access denied in AppContainer
- You need to share named objects (mutexes, events) between packaged and Win32 apps
- You're trying to configure `PartialTrustApplication` in a `.wapproj` packaging project

→ Check scenarios below for your specific situation

---

## Related Issues

- [#219](https://github.com/microsoft/WindowsAppSDK/issues/219) — AppContainer for Win32 apps (Status: Closed — Not Planned, 62 comments)
- [#175](https://github.com/microsoft/WindowsAppSDK/issues/175) — Easing named object sharing between packaged and Win32 (Status: Closed/Fixed)

---

## Scenarios & Solutions

### Scenario 1: Running a Win32 MSIX App in AppContainer (Partial Trust)

**Background:** Win32 apps can be launched in an AppContainer by using `EntryPoint="Windows.PartialTrustApplication"` in the MSIX app manifest. This provides Low IL sandboxing similar to UWP apps, but the feature is largely undocumented and has limitations.
> Source: @sylveon (CONTRIBUTOR) in [#219](https://github.com/microsoft/WindowsAppSDK/issues/219)

**Current status:** Issue #219 was **closed as Not Planned**. Microsoft has not committed to officially supporting AppContainer for Win32 apps through Windows App SDK.
> Source: Issue closed without explanation; @riverar (CONTRIBUTOR) noted: "Always fun when Microsoft employees close issues without saying a word."

**How to configure partial trust (C# projects / .wapproj):**

1. Remove `runFullTrust` capability from `Package.appxmanifest`:
   ```xml
   <!-- DELETE this line -->
   <rescap:Capability Name="runFullTrust" />
   ```

2. Set `TrustLevel` on the project reference in the `.wapproj`:
   ```xml
   <ProjectReference Include="..\MyApp\MyApp.csproj">
       <TrustLevel>Partial</TrustLevel>
   </ProjectReference>
   ```
   > ✅ Confirmed by: @sbanni in [#219](https://github.com/microsoft/WindowsAppSDK/issues/219) — generates correct appxmanifest with `PartialTrustApplication` entry point

**How to configure partial trust (C++ projects / .wapproj):**

The Visual Studio properties panel does not expose the `Trust Level` dropdown for `.vcxproj` references. Manually add `<TrustLevel>Partial</TrustLevel>` to the `<ProjectReference>` in the `.wapproj` file. Ignore the green squiggles — it builds correctly.
> ✅ Confirmed by: @torarnv and @sbanni in [#219](https://github.com/microsoft/WindowsAppSDK/issues/219)

**Minimum platform version required:**
You may need to bump `TargetPlatformMinVersion` — the old platform versions may reject the partial trust entry point.
> Source: @torarnv in [#219](https://github.com/microsoft/WindowsAppSDK/issues/219)

---

### Scenario 2: Shell_NotifyIcon (Tray Icons) Fail in AppContainer

**Cause:** `Shell_NotifyIcon` sends messages to the Windows shell, which runs at a higher integrity level. The AppContainer sandbox blocks this cross-IL communication, returning access denied for all calls.
> Source: @ptorr-msft (CONTRIBUTOR) in [#219](https://github.com/microsoft/WindowsAppSDK/issues/219)

**Fix:** No fix available. `Shell_NotifyIcon` does not work from AppContainer by design.

**Workaround architecture:** Run GUI/logic in a low-IL partial trust process and broker privileged operations (tray icons, etc.) through a separate medium-IL full trust helper process declared in your package manifest.
> Source: @sylveon (CONTRIBUTOR) and @ptorr-msft (CONTRIBUTOR) in [#219](https://github.com/microsoft/WindowsAppSDK/issues/219)

```
┌─────────────────────────────┐     ┌──────────────────────────┐
│  Main App (Low IL/AppContainer) │──→│ Helper (Medium IL/Full Trust) │
│  - UI, business logic       │     │  - Shell_NotifyIcon      │
│  - WinUI 3 / XAML           │     │  - Other privileged APIs │
└─────────────────────────────┘     └──────────────────────────┘
```

---

### Scenario 3: .NET Apps Have High CPU in AppContainer

**Cause:** Running .NET Core / .NET Framework Win32 apps in AppContainer causes ~20% CPU usage even for a "Hello World" app. This does not reproduce with C++ apps.
> Source: @Felix-Dev in [#219](https://github.com/microsoft/WindowsAppSDK/issues/219)

**Explanation:** Some .NET API calls are denied by the sandbox, and the runtime may continuously retry them.
> Source: @sylveon (CONTRIBUTOR) in [#219](https://github.com/microsoft/WindowsAppSDK/issues/219)

**Fix:** No fix available. This is a .NET runtime behavior when sandboxed.

---

### Scenario 4: Debugging AppContainer with Permissive Learning Mode (Windows 11)

**Background:** Windows 11 introduced `permissiveLearningMode` capability for AppContainer tokens. When enabled, access checks that would normally be denied are instead allowed but logged via ETW tracing. This helps identify which capabilities your app needs.
> Source: @WildByDesign in [#219](https://github.com/microsoft/WindowsAppSDK/issues/219), referencing [Tyranid's blog](https://www.tiraniddo.dev/2021/09/lowbox-token-permissive-learning-mode.html)

**Usage:**
1. Add `permissiveLearningMode` capability to your AppContainer token.
2. Run the app — all sandbox-denied operations are logged but allowed.
3. Collect ETW traces to identify failed access checks.
4. Tools: [SilkETW](https://github.com/fireeye/SilkETW) can log permissive learning mode events to JSON.

> ⚠️ @WildByDesign confirmed that with `permissiveLearningMode`, the app has "full access to the file system, network, registry, etc." — so this effectively disables the sandbox for debugging purposes only.

---

### Scenario 5: Sharing Named Objects Between Packaged and Win32 Apps

**Cause:** Named objects (mutexes, events, waitable timers) must be ACL'd with proper security descriptors to be shared between packaged apps (running in AppContainers) and regular Win32 processes. The required code is [complex and error-prone](https://docs.microsoft.com/en-us/windows/win32/api/securityappcontainer/nf-securityappcontainer-getappcontainernamedobjectpath#examples).
> Source: @DefaultRyan (MEMBER) in [#175](https://github.com/microsoft/WindowsAppSDK/issues/175)

**Fix:** This was implemented in Windows App SDK via [PR #2005](https://github.com/microsoft/WindowsAppSDK/pull/2005).
> ✅ Confirmed by: @alexlamtest (CONTRIBUTOR) in [#175](https://github.com/microsoft/WindowsAppSDK/issues/175)

**API:** Use the WinAppSDK security descriptor helper that takes Package Family Names and an access mask:
```c
// Simplified API — replaces boilerplate security descriptor code
GetSecurityDescriptorForPackageFamilyNames(
    cCountOfPackageFamilyNames,
    pListOfPackageFamilyNames,
    accessMask,
    &ppSD
);
```

**For C# consumers:** Use P/Invoke to call the C API, or use .NET's built-in security descriptor model.
> Source: @DefaultRyan (MEMBER) and @smaillet-ms in [#175](https://github.com/microsoft/WindowsAppSDK/issues/175)

---

## ⚠️ Unverified / Community Suggestions

> The following are community suggestions that have NOT been officially confirmed.

- The [Win32 App Isolation](https://github.com/nicktarr/win32-app-isolation) project may provide an alternative path forward for sandboxing, but has had no releases in over two years (from @AkazaRenn in #219)
- PasswordVault isolation requires AppContainer — no alternative exists for non-AppContainer Win32 apps (from @AkazaRenn in #219)

---

## References

- [Named object sharing guidance](https://docs.microsoft.com/en-us/windows/uwp/communication/sharing-named-objects)
- [Trust/AppContainers guide by @nickrandolph](https://nicksnettravels.builttoroam.com/trust-appcontainers/)
- [Permissive Learning Mode blog](https://www.tiraniddo.dev/2021/09/lowbox-token-permissive-learning-mode.html)
- [WinAppSDK PR #2005 — Named object sharing API](https://github.com/microsoft/WindowsAppSDK/pull/2005)

---

**Updated:** 2026-03-13 | **Confidence:** 0.7
**Sources:** #219, #175
