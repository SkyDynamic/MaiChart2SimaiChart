using System;
using MaiChart2SimaiChart.Gui.Activation;
using MaiChart2SimaiChart.Gui.Contracts.Services;
using MaiChart2SimaiChart.Gui.View;
using MaiChart2SimaiChart.Gui.Service;
using MaiChart2SimaiChart.Gui.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace MaiChart2SimaiChart.Gui;

public partial class App : Application
{
    public static WindowEx MainWindow
    {
        get;
    } = new MainWindow();
    
    public static UIElement? AppTitlebar { get; set; }
    
    private IHost Host
    {
        get;
    }
    
    public static T GetService<T>()
        where T : class
    {
        if ((Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }
    
    public App()
    {
        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) =>
            {
                // Default Activation Handler
                services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();
                
                // Service
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                services.AddTransient<INavigationViewService, NavigationViewService>();
                
                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<INavigationService, NavigationService>();
                
                services.AddSingleton<ExportProgressService>();
                
                // View & ViewModels
                services.AddSingleton<ExportChartViewModel>();
                services.AddTransient<ExportChartPage>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<SettingsPage>();
                services.AddTransient<ShellViewModel>();
                services.AddTransient<ShellPage>();
            })
            .Build();

        InitializeComponent();
    }
    
    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        
        await GetService<IActivationService>().ActivateAsync(args);
    }
}
