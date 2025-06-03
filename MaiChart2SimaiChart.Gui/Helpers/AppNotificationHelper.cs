using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace MaiChart2SimaiChart.Gui.Helpers;

public class AppNotificationHelper
{
    public static void ShowNotification(string title, string content)
    {
        var builder = new AppNotificationBuilder()
            .AddText(title)
            .AddText(content);
        AppNotificationManager.Default.Show(builder.BuildNotification());
    }
}