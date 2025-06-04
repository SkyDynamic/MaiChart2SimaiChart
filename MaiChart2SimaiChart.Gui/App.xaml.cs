using MaiChart2SimaiChart.Gui.Pages;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace MaiChart2SimaiChart.Gui;

public partial class App : Application
{
    private Window? _window;
    private AppWindow _appWindow;
    
    public App()
    {
        InitializeComponent();
    }

    public Window GetWindow()
    {
        return _window;
    }

    public void SetTheme(string selectedTheme)
    {
        if (_window.Content is FrameworkElement root)
        {
            switch (selectedTheme)
            {
                case "Light":
                    root.RequestedTheme = ElementTheme.Light;
                    break;
                case "Dark":
                    root.RequestedTheme = ElementTheme.Dark;
                    break;
                case "Default":
                    root.RequestedTheme = ElementTheme.Default;
                    break;
            }
        }
    }
    
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
        
        SetTheme(SettingsPage.GetUserTheme());

        _appWindow = _window.AppWindow;
        _appWindow.SetIcon("Assets/logo.ico");
    }
}
