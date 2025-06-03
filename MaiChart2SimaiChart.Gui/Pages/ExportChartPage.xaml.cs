using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using MaiChart2SimaiChart.Gui.Helpers;
using MaiChart2SimaiChart.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MaiChart2SimaiChart.Gui.Pages;

public sealed partial class ExportChartPage : Page
{
    private readonly List<int> _supportThreadsNumber = Enumerable.Range(1, 16).ToList();
    
    private string _a000Path = "";
    private string _outputPath = "";
    private bool _strictDecimal = false;
    private readonly string[] _trackCategorizeMethodSet = StaticSettings.TrackCategorizeMethodSet;
    private bool _number = false;
    private Task? _exportThread = null;
    private int _currentTotal = 0;
    private double _progressValue;
    private string _progressPercent = "0%";
    private string _progressCount = "0/0";
    private static CancellationTokenSource? tokenSource;
    private static CancellationToken cancellationToken;
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public ExportChartPage()
    {
        InitializeComponent();
    }
    
    public double ProgressValue
    {
        get => _progressValue;
        private set
        {
            _progressValue = value;
            OnPropertyChanged();
        }
    }

    public string ProgressPercent
    {
        get => _progressPercent;
        private set
        {
            _progressPercent = value;
            OnPropertyChanged();
        }
    }

    public string ProgressCount
    {
        get => _progressCount;
        private set
        {
            _progressCount = value;
            OnPropertyChanged();
        }
    }
    
    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private async Task<string?> ChooseFolder()
    {
        var dialog = new FolderPicker();
        dialog.FileTypeFilter.Add("*");
        dialog.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((App)App.Current).GetWindow());
        
        WinRT.Interop.InitializeWithWindow.Initialize(dialog, hwnd);
        
        var result = await dialog.PickSingleFolderAsync();

        return result.Path;
    }

    private async void A000PathChooseFolderButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await ChooseFolder();
            if (result != null)
            {
                A000PathTextBox.Text = result;
                _a000Path = result;
            }
            else
            {
                A000PathTextBox.Text = "";
                _a000Path = "";
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
            var result = await ChooseFolder();

            if (result != null)
            {
                OutputPathTextBox.Text = result;
                _outputPath = result;
            }
            else
            {
                OutputPathTextBox.Text = "";
                _outputPath = "";
            }
        } catch (Exception ex)
        {
            // Ignore
        }
    }
    
    private void A000PathTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _a000Path = A000PathTextBox.Text;
    }
    
    private void OutputPathTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _outputPath = OutputPathTextBox.Text;
    }

    private async void ExportButton_OnClick(object sender, RoutedEventArgs e)
    {
        ExportButton.IsEnabled = false;
        var dialogTitle = "Confirm Export";
        var dialogContent = "Check your arguments again: \n" +
                            $"A000 Location: {_a000Path}\n" +
                            $"Output Location: {_outputPath}\n" +
                            $"Strict Decimal: {_strictDecimal}\n" +
                            $"NumberIDFolderName: {_number}\n" +
                            $"Categorize Method: {_trackCategorizeMethodSet[TrackCategorizeMethodComboBox.SelectedIndex]}\n" +
                            $"Thread Count: {ThreadsNumberComboBox.SelectedItem}";

        var result = await DialogHelper.ConfirmDialog(dialogTitle, dialogContent, "Confirm", "Cancel", XamlRoot);

        switch (result)
        {
            case ContentDialogResult.Primary:
                if (_a000Path == "" || _outputPath == "")
                {
                    DialogHelper.NotifyDialog("Warning", "Please choose a valid path.", "OK", XamlRoot);
                    ExportButton.IsEnabled = true;
                    return;
                }
                
                ExportProgressPanel.Visibility = Visibility.Visible;
                var option = new CompileUtils.CompileDatabaseOption(
                    _strictDecimal,
                    _number,
                    TrackCategorizeMethodComboBox.SelectedIndex,
                    (int)ThreadsNumberComboBox.SelectedItem
                );
                
                tokenSource = new CancellationTokenSource();
                cancellationToken = tokenSource.Token;
                
                _exportThread = Task.Run(() =>
                {
                    while (true)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        CompileUtils.CompileDatabase(
                            _a000Path,
                            _outputPath,
                            option,
                            i => _currentTotal = i,
                            i =>
                            {
                                if (DispatcherQueue.HasThreadAccess)
                                {
                                    var total = _currentTotal;
                                    var percent = total == 0 ? 0 : ((double)i / total) * 100;

                                    ProgressBar.Value = percent;
                                    ProgressValue = percent;
                                    ProgressPercent = $"{percent:F1}%";
                                    ProgressPercentText.Text = $"{percent:F1}%";
                                    ProgressCount = $"{i}/{total}";
                                    ProgressCountText.Text = $"{i}/{total}";
                                }
                                else
                                {
                                    DispatcherQueue.TryEnqueue(() =>
                                    {
                                        var total = _currentTotal;
                                        var percent = total == 0 ? 0 : ((double)i / total) * 100;

                                        ProgressBar.Value = percent;
                                        ProgressValue = percent;
                                        ProgressPercent = $"{percent:F1}%";
                                        ProgressPercentText.Text = $"{percent:F1}%";
                                        ProgressCount = $"{i}/{total}";
                                        ProgressCountText.Text = $"{i}/{total}";
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
                        _exportThread = null;
                        AppNotificationHelper.ShowNotification("Export Completed", "");
                    }
                }, cancellationToken);
                break;
            case ContentDialogResult.None:
                ExportButton.IsEnabled = true;
                break;
        }
    }

    private void StopExport(object sender, RoutedEventArgs e)
    {
        if (_exportThread != null)
        {
            if (tokenSource != null) tokenSource.Cancel();
            _exportThread = null;
            ExportProgressPanel.Visibility = Visibility.Collapsed;
            ExportButton.IsEnabled = true;
        }
        AppNotificationHelper.ShowNotification("Export Stopped", "Export stopped by user.");
    }
}
