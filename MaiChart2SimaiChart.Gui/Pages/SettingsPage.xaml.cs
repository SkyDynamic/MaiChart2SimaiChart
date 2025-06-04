using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources.Core;
using MaiChart2SimaiChart.Gui.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MaiChart2SimaiChart.Gui.Pages;
public sealed partial class SettingsPage : Page
{
    private List<ComboBoxItem> LangComboBoxItems = new();
    
    public SettingsPage()
    {
        InitializeComponent();
    }
    
    public static string GetUserTheme()
    {
        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        var savedTheme = localSettings.Values["AppTheme"] as string ?? "Default";
        
        return savedTheme;
    }
    
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        var savedTheme = localSettings.Values["AppTheme"] as string ?? "Default";
        
        switch (savedTheme)
        {
            case "Light":
                ThemeComboBox.SelectedIndex = 0;
                break;
            case "Dark":
                ThemeComboBox.SelectedIndex = 1;
                break;
            default:
                ThemeComboBox.SelectedIndex = 2;
                break;
        }
    }

    private void SwitchTheme(object sender, RoutedEventArgs e)
    {
        var selectedTheme = ((ComboBoxItem)ThemeComboBox.SelectedItem)?.Tag?.ToString() ?? "Default";
        var app = (App) App.Current;
        
        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        localSettings.Values["AppTheme"] = selectedTheme;
        
        app.SetTheme(selectedTheme);
    }
}
