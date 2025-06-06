using MaiChart2SimaiChart.Gui.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace MaiChart2SimaiChart.Gui.View;
public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }
    
    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }
}
