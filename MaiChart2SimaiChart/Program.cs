using ManyConsole;

namespace MaiChart2SimaiChart;

class Program
{
    private static readonly string Home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    
    public static readonly string[] DefaultPaths =
    [
        $"{Home}/MaiAnalysis/A000/",
        $"{Home}/MaiAnalysis/Sound/",
        $"{Home}/MaiAnalysis/Image/Texture2D/",
        $"{Home}/MaiAnalysis/DXBGA_HEVC/",
        $"{Home}/MaiAnalysis/Output/"
    ];
    
    public static readonly string[] TrackCategorizeMethodSet =
        ["Genre", "Level", "Cabinet", "Composer", "BPM", "SD/DX Chart", "No Separate Folder"];
    
    public static string GlobalTrackCategorizeMethod = TrackCategorizeMethodSet[0];
    
    public static int NumberTotalTrackCompiled;
    
    public static Dictionary<int, string> CompiledTracks = [];
    public static List<string> CompiledChart = [];
    
    public static int Main(string[] args)
    {
        IEnumerable<ConsoleCommand> commands = GetCommands();
        
        return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
    }
    
    private static IEnumerable<ConsoleCommand> GetCommands()
    {
        return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
    }
    
    public static string CompensateZero(string intake)
    {
        try
        {
            string result = intake;
            while (result.Length < 6)
            {
                result = "0" + result;
            }

            return result;
        }
        catch (NullReferenceException ex)
        {
            return "Exception raised: " + ex.Message;
        }
    }
    
    public static string CompensateShortZero(string intake)
    {
        try
        {
            string result = intake;
            while (result.Length < 4 && intake != null)
            {
                result = "0" + result;
            }

            return result;
        }
        catch (NullReferenceException ex)
        {
            return "Exception raised: " + ex.Message;
        }
    }
}