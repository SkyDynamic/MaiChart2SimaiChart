using Windows.ApplicationModel.Resources;
using Windows.Storage;

namespace MaiChart2SimaiChart.Gui.Helpers;

public static class TranslateUtil
{
    private static ResourceLoader _resourceLoader = ResourceLoader.GetForCurrentView();
    private const string LanguageSettingsKey = "AppLanguage";
    
    public static string Translate(string key)
    {
        return _resourceLoader.GetString(key);
    }
    
    public static string GetCurrentOrSavedLanguage()
    {
        var localSettings = ApplicationData.Current.LocalSettings;
        if (localSettings.Values.TryGetValue(LanguageSettingsKey, out var savedLanguage))
        {
            return savedLanguage as string;
        }
        
        return Windows.System.UserProfile.GlobalizationPreferences.Languages[0];
    }
}