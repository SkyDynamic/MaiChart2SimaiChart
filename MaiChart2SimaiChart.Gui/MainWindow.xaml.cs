using MaiChart2SimaiChart.Gui.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MaiChart2SimaiChart.Gui;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        ExtendsContentIntoTitleBar = true;
    }

    private void Nv_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var selectedItem = (NavigationViewItem)args.SelectedItem;

        if ((string)selectedItem.Tag == "Ex") contentFrame.Navigate(typeof(ExportChartPage));
    }

    private void Nv_OnLoaded(object sender, RoutedEventArgs e)
    {
        nv.SelectedItem = nv.MenuItems[0];

        contentFrame.Navigate(typeof(ExportChartPage));
    }
}
