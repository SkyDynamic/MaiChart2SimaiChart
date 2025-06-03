using System;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MaiChart2SimaiChart.Gui.Helpers;

public class DialogHelper
{
    public static async Task<ContentDialogResult> ConfirmDialog(
        string title,
        string content,
        string primaryButtonText,
        string closeButtonText,
        XamlRoot root)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = closeButtonText,
            DefaultButton = ContentDialogButton.Primary
        };
        
        if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
        {
            dialog.XamlRoot = root;
        }
        
        return await dialog.ShowAsync();
        
    }
    
    public static async Task<ContentDialogResult> NotifyDialog(
        string title,
        string content,
        string closeButtonText,
        XamlRoot root)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = closeButtonText
        };
        
        if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
        {
            dialog.XamlRoot = root;
        }
        
        return await dialog.ShowAsync();
    }
}