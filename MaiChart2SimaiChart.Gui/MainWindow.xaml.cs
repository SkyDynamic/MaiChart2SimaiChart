using Windows.UI.ViewManagement;
using MaiChart2SimaiChart.Gui.Helpers;
using WinUIEx;

namespace MaiChart2SimaiChart.Gui;

public sealed partial class MainWindow : WindowEx
{
    private Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;
    private UISettings settings;
    
    public MainWindow()
    {
        InitializeComponent();
        
        Content = null;
        Title = "MaiChart2SimaiChart";
        
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        
        settings = new();
        settings.ColorValuesChanged += Settings_ColorValuesChanged;
    }
    
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }
}
