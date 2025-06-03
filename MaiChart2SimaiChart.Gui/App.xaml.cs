using Microsoft.UI.Xaml;

namespace MaiChart2SimaiChart.Gui;

public partial class App : Application
{
    private Window? _window;
    
    public App()
    {
        InitializeComponent();
    }

    public Window GetWindow()
    {
        return _window;
    }
    
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
