using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MaiChart2SimaiChart.Gui.Service;

public class ExportProgressService : INotifyPropertyChanged
{
    private double _progressValue;
    public double ProgressValue
    {
        get => _progressValue;
        set { _progressValue = value; OnPropertyChanged(); }
    }

    private string _progressPercent = "0%";
    public string ProgressPercent
    {
        get => _progressPercent;
        set { _progressPercent = value; OnPropertyChanged(); }
    }

    private string _progressCount = "0/0";
    public string ProgressCount
    {
        get => _progressCount;
        set { _progressCount = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}