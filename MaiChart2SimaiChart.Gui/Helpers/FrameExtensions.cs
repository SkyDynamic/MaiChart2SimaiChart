using Microsoft.UI.Xaml.Controls;

namespace MaiChart2SimaiChart.Gui.Helpers;

public static class FrameExtensions
{
    public static object? GetPageViewModel(this Frame frame) => frame?.Content?.GetType().GetProperty("ViewModels")?.GetValue(frame.Content, null);
}