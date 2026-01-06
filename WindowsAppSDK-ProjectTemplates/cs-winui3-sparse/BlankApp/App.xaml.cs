using System;
using System.Threading.Tasks;
using BlankApp.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BlankApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        private readonly IServiceProvider _services;
        private readonly ILogger<App> _logger;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            _services = ConfigureServices();
            _logger = _services.GetRequiredService<ILogger<App>>();

            // Global exception hooks to capture crashes and unobserved faults
            this.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }

        /// <summary>
        /// Retrieves a service from the application's dependency injection container.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>The requested service instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the application has not been initialized.</exception>
        /// <remarks>
        /// <para><strong>⚠️ Service Locator Pattern - Use with Caution</strong></para>
        /// <para>
        /// This method implements the Service Locator pattern, which is generally considered an anti-pattern
        /// because it hides dependencies and makes code harder to test and understand.
        /// </para>
        /// <para><strong>Prefer Constructor Injection:</strong></para>
        /// <para>
        /// - For ViewModels, Services, and Data classes: Always use constructor injection
        /// - Example: <c>public UserService(IUserRepository repository, ILogger&lt;UserService&gt; logger)</c>
        /// - Benefits: Explicit dependencies, better testability, compile-time safety
        /// </para>
        /// <para><strong>When This Pattern Is Acceptable:</strong></para>
        /// <para>
        /// - WinUI Pages/Controls that require parameterless constructors for XAML instantiation
        /// - Code-behind files where the framework controls object creation
        /// - Temporary bridge code during migration to full dependency injection
        /// </para>
        /// <para><strong>Limitations and Risks:</strong></para>
        /// <para>
        /// - Hidden dependencies: Classes using this don't declare what they need
        /// - Runtime failures: Dependencies resolved at runtime instead of compile-time
        /// - Harder testing: Must initialize full application to test components
        /// - Tight coupling: Direct dependency on App class throughout codebase
        /// - Initialization order: Fails if called before App constructor completes
        /// </para>
        /// <para><strong>Best Practice:</strong> Document why GetService is necessary each time you use it.</para>
        /// </remarks>
        public static T GetService<T>() where T : class
        {
            if (Current is not App app)
            {
                throw new InvalidOperationException("Application has not been initialized");
            }

            return app._services.GetRequiredService<T>();
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddAppLogging();

            // Register application services and dependencies for DI
#if INCLUDE_SAMPLES
            services.AddSingleton<BlankApp.Data.IUserRepository, BlankApp.Data.UserRepository>();
            services.AddSingleton<BlankApp.Services.IUserService, BlankApp.Services.UserService>();
            services.AddTransient<BlankApp.ViewModels.MainViewModel>();
#endif

            return services.BuildServiceProvider();
        }

        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            _logger.LogCritical(e.Exception, "Unhandled UI exception");
            e.Handled = true;
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger.LogCritical(e.Exception, "Unobserved task exception");
            e.SetObserved();
        }

        private void OnDomainUnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                _logger.LogCritical(ex, "Domain unhandled exception. Terminating={IsTerminating}", e.IsTerminating);
            }
            else
            {
                _logger.LogCritical("Domain unhandled non-Exception. Terminating={IsTerminating}", e.IsTerminating);
            }
        }
    }
}
