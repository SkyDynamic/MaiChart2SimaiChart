using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MaiChart2SimaiChart.Gui.ViewModels;

public partial class ExportChartViewModel : ObservableObject
{
    private string _a000Path = "";

    private string _outputPath = "";
    
    private bool _convertVideo = false;

    private bool _strictDecimal = false;

    private bool _number = false;

    private int _trackCategorizeMethodComboBoxIndex = 0;
    private int _threadComboBoxIndex = 0;
    
    public Task? ExportThread = null;
    public int CurrentTotal = 0;
    
    public CancellationTokenSource? TokenSource;
    public CancellationToken CancellationToken;
    
    public new event PropertyChangedEventHandler? PropertyChanged;
    
    public bool Exporting = false;

    public string A000Path
    {
        get => _a000Path;
        set
        {
            _a000Path = value;
            OnPropertyChanged();
        }
    }
    
    public string OutputPath
    {
        get => _outputPath;
        set
        {
            _outputPath = value;
            OnPropertyChanged();
        }
    }
    
    public bool ConvertVideo
    {
        get => _convertVideo;
        set
        {
            _convertVideo = value;
            OnPropertyChanged();
        }
    }
    
    public bool StrictDecimal
    {
        get => _strictDecimal;
        set
        {
            _strictDecimal = value;
            OnPropertyChanged();
        }
    }
    
    public bool Number
    {
        get => _number;
        set
        {
            _number = value;
            OnPropertyChanged();
        }
    }
    
    public int TrackCategorizeMethodComboBoxIndex
    {
        get => _trackCategorizeMethodComboBoxIndex;
        set
        {
            _trackCategorizeMethodComboBoxIndex = value;
            OnPropertyChanged();
        }
    }
    
    public int ThreadComboBoxIndex
    {
        get => _threadComboBoxIndex;
        set
        {
            _threadComboBoxIndex = value;
            OnPropertyChanged();
        }
    }
    
    private new void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}