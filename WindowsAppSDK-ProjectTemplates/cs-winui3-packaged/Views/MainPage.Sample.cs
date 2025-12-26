using BlankApp.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BlankApp.Views;

/// <summary>
/// Main page of the application.
/// Demonstrates MVVM pattern with minimal code-behind.
/// </summary>
public sealed partial class MainPage : Page
{
    /// <summary>
    /// Gets the ViewModel for this page.
    /// ViewModel is injected via dependency injection.
    /// </summary>
    public MainViewModel ViewModel { get; }

    /// <summary>
    /// Initializes a new instance of MainPage.
    /// </summary>
    public MainPage()
    {
        this.InitializeComponent();
        
        // Get ViewModel from DI container
        ViewModel = App.GetService<MainViewModel>();
        
        // Set DataContext for bindings
        DataContext = ViewModel;
    }

    /// <summary>
    /// Handles the page Loaded event.
    /// Only UI-specific initialization should be here.
    /// </summary>
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Load data when page is loaded
        // Business logic is in ViewModel, not here
        await ViewModel.LoadDataAsync();
    }

    /// <summary>
    /// Handles the Refresh button click.
    /// Command pattern is preferred, but events are acceptable for UI-specific actions.
    /// </summary>
    private async void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.RefreshAsync();
    }

    /// <summary>
    /// Handles the page Unloaded event.
    /// Clean up resources here if needed.
    /// </summary>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Clean up any UI-specific resources
        // ViewModel cleanup is handled by DI container
    }
}

/// <summary>
/// Example XAML structure for this page:
/// 
/// <Page
///     x:Class="BlankApp.Views.MainPage"
///     xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
///     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
///     Loaded="OnLoaded"
///     Unloaded="OnUnloaded">
///     
///     <Grid>
///         <StackPanel Spacing="16" Padding="24">
///             <TextBlock Text="{x:Bind ViewModel.Title, Mode=OneWay}" 
///                        Style="{StaticResource TitleTextBlockStyle}"/>
///             
///             <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}" />
///             
///             <ProgressRing IsActive="{x:Bind ViewModel.IsLoading, Mode=OneWay}" />
///             
///             <Button Content="Refresh" Click="OnRefreshClick" 
///                     IsEnabled="{x:Bind ViewModel.IsLoading, Mode=OneWay, Converter={StaticResource InverseBoolConverter}}" />
///         </StackPanel>
///     </Grid>
/// </Page>
/// 
/// </summary>
/// 