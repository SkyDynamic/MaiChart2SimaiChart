using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MaiChart2SimaiChart.Gui.Helpers;
using MaiChart2SimaiChart.Gui.Service;
using MaiChart2SimaiChart.Gui.ViewModels;
using MaiChart2SimaiChart.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MaiChart2SimaiChart.Gui.View;

public sealed partial class ExportChartPage : Page
{
    private readonly List<int> _supportThreadsNumber = Enumerable.Range(1, 16).ToList();
    private readonly string[] _trackCategorizeMethodSet = StaticSettings.TrackCategorizeMethodSet;

    public ExportChartViewModel ViewModel
    {
        get;
    }
    
    private readonly ExportProgressService _progressService;
    
    public ExportChartPage()
    {
        ViewModel = App.GetService<ExportChartViewModel>();
        _progressService = App.GetService<ExportProgressService>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (ViewModel.Exporting & ViewModel.ExportThread != null)
        {
            ExportButton.IsEnabled = false;
            ExportProgressPanel.Visibility = Visibility.Visible;
        }
    }
    
    private async void A000PathChooseFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await FolderHelper.ChooseFolder();
            if (result != null)
            {
                A000PathTextBox.Text = result;
                ViewModel.A000Path = result;
            }
            else
            {
                A000PathTextBox.Text = "";
                ViewModel.A000Path = "";
            }
        } catch (Exception ex)
        {
            // Ignore
        }
    }
    
    private async void OutputPathChooseFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await FolderHelper.ChooseFolder();

            if (result != null)
            {
                OutputPathTextBox.Text = result;
                ViewModel.OutputPath = result;
            }
            else
            {
                OutputPathTextBox.Text = "";
                ViewModel.OutputPath = "";
            }
        } catch (Exception ex)
        {
            // Ignore
        }
    }

    private async void ExportButton_OnClick(object sender, RoutedEventArgs e)
    {
        ExportButton.IsEnabled = false;
        var dialogTitle = "ExportPage_ConfirmDialogTitle".GetLocalized("Export Finish");
        var dialogContent = "ExportPage_ConfirmDialogContent".GetLocalized("Check your arguments again: ") + " \n" +
                            $"A000 Location: {ViewModel.A000Path}\n" +
                            $"Output Location: {ViewModel.OutputPath}\n" +
                            $"Strict Decimal: {ViewModel.StrictDecimal}\n" +
                            $"NumberIDFolderName: {ViewModel.Number}\n" +
                            $"Categorize Method: {_trackCategorizeMethodSet[ViewModel.TrackCategorizeMethodComboBoxIndex]}\n" +
                            $"Thread Count: {ViewModel.ThreadComboBoxIndex + 1}";

        var result = await DialogHelper.ConfirmDialog(
            dialogTitle, 
            dialogContent, 
            "Confirm".GetLocalized(), 
            "Cancel".GetLocalized(),
            XamlRoot
            );

        switch (result)
        {
            case ContentDialogResult.Primary:
                if (ViewModel.A000Path == "" || ViewModel.OutputPath == "")
                {
                    await DialogHelper.NotifyDialog(
                        "Warning".GetLocalized("Warning"), 
                        "ExportPage_A0PathNullNotifyText".GetLocalized("Export Finish"), 
                        "OK", 
                        XamlRoot
                        );
                    ExportButton.IsEnabled = true;
                    return;
                }
                
                ExportProgressPanel.Visibility = Visibility.Visible;
                var option = new CompileUtils.CompileDatabaseOption(
                    ViewModel.StrictDecimal,
                    ViewModel.Number,
                    TrackCategorizeMethodComboBox.SelectedIndex,
                    ThreadsNumberComboBox.SelectedIndex + 1
                );
                
                ViewModel.TokenSource = new CancellationTokenSource();
                ViewModel.CancellationToken = ViewModel.TokenSource.Token;
                
                ViewModel.ExportThread = Task.Run(() =>
                {
                    CompileUtils.CompileDatabase(
                        ViewModel.A000Path,
                        ViewModel.OutputPath,
                        option,
                        ViewModel.CancellationToken,
                        i => ViewModel.CurrentTotal = i,
                        i =>
                        {
                            if (DispatcherQueue.HasThreadAccess)
                            {
                                var total = ViewModel.CurrentTotal;
                                var percent = total == 0 ? 0 : ((double)i / total) * 100;

                                _progressService.ProgressValue = percent;
                                _progressService.ProgressPercent = $"{percent:F1}%";
                                _progressService.ProgressCount = $"{i}/{total}";
                            }
                            else
                            {
                                DispatcherQueue.TryEnqueue(() =>
                                {
                                    var total = ViewModel.CurrentTotal;
                                    var percent = total == 0 ? 0 : ((double)i / total) * 100;

                                    _progressService.ProgressValue = percent;
                                    _progressService.ProgressPercent = $"{percent:F1}%";
                                    _progressService.ProgressCount = $"{i}/{total}";
                                });
                            }
                        },
                        () =>
                        {
                            if (DispatcherQueue.HasThreadAccess)
                            {
                                ProgressBar.Visibility = Visibility.Collapsed;
                                ExportButton.IsEnabled = true;
                            }
                            else
                            {
                                DispatcherQueue.TryEnqueue(() =>
                                {
                                    ProgressBar.Visibility = Visibility.Collapsed;
                                    ExportButton.IsEnabled = true;
                                });
                            }
                        }
                    );
                    ViewModel.ExportThread = null;
                    AppNotificationHelper.ShowNotification(
                        "ExportPage_ExportFinishNotify",
                        "");
                }, ViewModel.CancellationToken);
                ViewModel.Exporting = true;
                break;
            case ContentDialogResult.None:
                ExportButton.IsEnabled = true;
                break;
        }
    }

    private void StopExport(object sender, RoutedEventArgs e)
    {
        if (ViewModel.ExportThread != null)
        {
            if (ViewModel.TokenSource != null) ViewModel.TokenSource.Cancel();
            ViewModel.ExportThread = null;
            ExportProgressPanel.Visibility = Visibility.Collapsed;
            ExportButton.IsEnabled = true;
        }
        AppNotificationHelper.ShowNotification(
            "ExportPage_StopExportNotifyTitle".GetLocalized("Stop Export"), 
            "ExportPage_StopExportNotifyContent".GetLocalized("Export stopped by user."));
    }
}
