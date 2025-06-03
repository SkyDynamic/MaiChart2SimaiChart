using MaiChart2SimaiChart.Util;
using MaiChart2SimaiChart;
using ManyConsole;

namespace MaiChart2SimaiChartCli.Commands;

public class CompileDatabase : ConsoleCommand
{
    public const int Success = 0;
    public const int Failed = 2;
    public bool StrictDecimal { get; set; }
    public bool MusicIdFolderName { get; set; }
    public string? A000Location { get; set; }
    public int CategorizeIndex { get; set; }
    public string? Destination { get; set; }
    public string CategorizeMethods { get; set; }
    public int ThreadCount { get; set; }

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
            string a000Location =
                A000Location ?? throw new FileNotFoundException("A000 location was not specified");

            var option = new CompileUtils.CompileDatabaseOption(
                StrictDecimal, 
                MusicIdFolderName,
                CategorizeIndex, 
                ThreadCount);
            
            return CompileUtils.CompileDatabaseWithProgressBar(a000Location, Destination, option);
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