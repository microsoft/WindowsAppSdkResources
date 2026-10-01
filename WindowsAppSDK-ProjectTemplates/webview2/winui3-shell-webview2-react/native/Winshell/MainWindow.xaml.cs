using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.Web.WebView2.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Winshell;

public sealed partial class MainWindow : Window
{
    private readonly BridgeRouter _router;

    public MainWindow()
    {
        // Resolve router from DI if available
        _router = App.Services?.GetService<BridgeRouter>() ?? throw new InvalidOperationException("BridgeRouter not available - ensure App.Services is initialized");

        InitializeComponent();
        _ = EnsureWebViewAsync();
    }

    private bool _webViewReady;

    private async Task EnsureWebViewAsync()
    {
        if (_webViewReady)
            return;

        _webViewReady = true;

        await ShellWebView.EnsureCoreWebView2Async();

        ShellWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        ShellWebView.CoreWebView2.Settings.AreDevToolsEnabled = true;

        var webRoot = Path.Combine(AppContext.BaseDirectory, "Web");
        ShellWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            hostName: "app",
            folderPath: webRoot,
            accessKind: CoreWebView2HostResourceAccessKind.Allow);

        ShellWebView.CoreWebView2.WebMessageReceived += async (_, e) =>
        {
            var json = e.WebMessageAsJson;
            var response = await _router.HandleAsync(json);
            ShellWebView.CoreWebView2.PostWebMessageAsJson(response);
        };

        // Navigate: prefer explicit env (for tooling in Debug), else packaged content via virtual host
#if DEBUG
        var envUrl = Environment.GetEnvironmentVariable("WINSHELL_DEV_URL");
        var target = !string.IsNullOrWhiteSpace(envUrl)
            ? envUrl!
            : "https://app/index.html";
#else
        var target = "https://app/index.html";
#endif

        ShellWebView.CoreWebView2.Navigate(target);
    }
}
