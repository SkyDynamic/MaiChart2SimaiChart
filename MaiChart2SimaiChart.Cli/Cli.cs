using ManyConsole;

namespace MaiChart2SimaiChartCli;

class Cli
{
    public static int Main(string[] args)
    {
        IEnumerable<ConsoleCommand> commands = GetCommands();
        
        return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
    }
    
    private static IEnumerable<ConsoleCommand> GetCommands()
    {
        return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Cli));
    }
}