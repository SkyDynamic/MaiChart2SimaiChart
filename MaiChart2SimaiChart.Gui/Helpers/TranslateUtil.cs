using Windows.ApplicationModel.Resources;
using Windows.Storage;

namespace MaiChart2SimaiChart.Gui.Helpers;

public static class TranslateUtil
{
    private static ResourceLoader _resourceLoader = ResourceLoader.GetForViewIndependentUse();
    private const string LanguageSettingsKey = "AppLanguage";
    
    public static string Translate(string key)
    {
        if (_resourceLoader == null)
        {
            throw new System.NullReferenceException("ResourceLoader is not initialized.");
        }
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