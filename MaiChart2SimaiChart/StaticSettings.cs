namespace MaiChart2SimaiChart;

public static class StaticSettings
{
    public static readonly string TempPath = Path.Combine(Path.GetTempPath(), "TempExport");
    
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
}