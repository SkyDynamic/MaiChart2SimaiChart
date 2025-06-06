namespace MaiChart2SimaiChart.Gui.Helpers;

public static class TranslateUtil
{
    public static string GetLocalized(this string key, string? defaultValue = null)
    {
        if (defaultValue == null)
        {
            defaultValue = key;
        }
        
        return CommunityToolkit.WinUI.StringExtensions.GetLocalized(key) ?? defaultValue;
    }
}