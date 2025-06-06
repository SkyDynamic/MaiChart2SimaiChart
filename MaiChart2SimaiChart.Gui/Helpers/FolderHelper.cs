using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace MaiChart2SimaiChart.Gui.Helpers;

public class FolderHelper
{
    public static async Task<string?> ChooseFolder()
    {
        var dialog = new FolderPicker();
        dialog.FileTypeFilter.Add("*");
        dialog.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        
        WinRT.Interop.InitializeWithWindow.Initialize(dialog, hwnd);
        
        var result = await dialog.PickSingleFolderAsync();

        return result.Path;
    }
}