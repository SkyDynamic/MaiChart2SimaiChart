using MaiChart2SimaiChart.Util;
using MaiChart2SimaiChart;
using ManyConsole;

namespace MaiChart2SimaiChartCli.Commands;

public class CompileDatabase : ConsoleCommand
{
    public const int Success = 0;
    private const int Failed = 2;
    private bool ConvertVideo { get; set; }
    private bool StrictDecimal { get; set; }
    private bool MusicIdFolderName { get; set; }
    private string? A000Location { get; set; }
    private int CategorizeIndex { get; set; }
    private string? Destination { get; set; }
    private string CategorizeMethods { get; set; }
    private int ThreadCount { get; set; }

    public CompileDatabase()
    {
        CategorizeMethods = "";
        for (int i = 0; i < StaticSettings.TrackCategorizeMethodSet.Length; i++)
        {
            CategorizeMethods += $"[{i}]{StaticSettings.TrackCategorizeMethodSet[i]}\n";
        }

        IsCommand("CompileDatabase", "Compile whole ma2 database to format assigned");

        HasLongDescription(
            "This function enables user to compile whole database to the format they want. By default is simai for ma2.");
        HasRequiredOption("p|path=", "REQUIRED: Folder of A000 to override - end with a path separator",
            aPath => A000Location = aPath);
        HasRequiredOption("o|output=", "REQUIRED: Export compiled chart to location specified",
            dest => Destination = dest);
        HasOption("g|genre=", "The preferred categorizing scheme, includes:\n" + CategorizeMethods,
            genre => CategorizeIndex = int.Parse(genre));
        HasOption("d|decimal:", "Force output chart to have levels rated by decimal", _ => StrictDecimal = true);
        HasOption("n|number:", "Use musicID as folder name instead of sort name", _ => MusicIdFolderName = true);
        HasOption("v|video:", "Convert with pv", _ => ConvertVideo = true);
        HasOption("t|thread=", "Number of threads to use for compiling, default is 1", threadCount =>
        {
            if (int.TryParse(threadCount, out var count) && count > 0)
                ThreadCount = count;
            else
                ThreadCount = 1;
        });
    }

    public override int Run(string[] remainingArguments)
    {
        try
        {
            var a000Location =
                A000Location ?? throw new FileNotFoundException("A000 location was not specified");

            var option = new CompileUtils.CompileDatabaseOption(
                ConvertVideo,
                StrictDecimal, 
                MusicIdFolderName,
                CategorizeIndex, 
                ThreadCount);

            return Task.Run(async () =>
                await CompileUtils.CompileDatabaseWithProgressBar(a000Location, Destination, option))
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Program cannot proceed because of following error returned: \n{0}", ex.GetType());
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
            Console.ReadKey();
            return Failed;
        }
    }
}