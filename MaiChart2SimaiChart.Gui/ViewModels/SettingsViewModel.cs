using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using MaiChart2SimaiChart.Gui.Contracts.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MaiChart2SimaiChart.Gui.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeSelectorService _themeSelectorService;

    [ObservableProperty]
    private ElementTheme _elementTheme;
    
    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
    }

    public async void SwitchThemeCommand(object sender, RoutedEventArgs e)
    {
        var themeComboBox = sender as ComboBox;
        
        var selectedTheme = ((ComboBoxItem)themeComboBox.SelectedItem)?.Tag?.ToString() ?? "Default";
        
        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        localSettings.Values["AppTheme"] = selectedTheme;

        var theme = ElementTheme.Default;
        switch (selectedTheme)
        {
            case "Default":
                ElementTheme = ElementTheme.Default;
                break;
            case "Light":
                theme = ElementTheme.Light;
                ElementTheme = ElementTheme.Light;
                break;
            case "Dark":
                theme = ElementTheme.Dark;
                ElementTheme = ElementTheme.Dark;
                break;
        }
        
        await _themeSelectorService.SetThemeAsync(theme);
    }
    
    public void InitializeThemeComboBox(object sender, RoutedEventArgs e)
    {
        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        var selectedTheme = localSettings.Values["AppTheme"]?.ToString() ?? "Default";
        
        var themeComboBox = sender as ComboBox;
        themeComboBox.SelectedItem = themeComboBox.Items.FirstOrDefault(item => ((ComboBoxItem)item).Tag.ToString() == selectedTheme);
        if (themeComboBox.SelectedItem == null)
        {
            themeComboBox.SelectedItem = themeComboBox.Items.FirstOrDefault(item => ((ComboBoxItem)item).Tag.ToString() == "Default");
        }
    }
}