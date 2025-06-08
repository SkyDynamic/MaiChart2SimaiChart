using System.Diagnostics;
using System.Reflection;

namespace MaiChart2SimaiChart.Util;

public static class VideoUtil
{
    public static Process UnpackUsm(string usmPath, string outputPath)
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        var wrapperPath = Path.Combine(assemblyDir, "Libs", "wannacri-wrapper.exe");
        if (!File.Exists(wrapperPath))
        {
            throw new FileNotFoundException("wannacri-wrapper.exe not found");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = wrapperPath,
            Arguments = $"{usmPath} {outputPath}",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var process = Process.Start(startInfo);
        
        if (process != null)
        {
            process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
        }

        return process;
    }
}