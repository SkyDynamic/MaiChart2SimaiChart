using System.Diagnostics;
using System.Reflection;

namespace MaiChart2SimaiChart.Installer;

class Program
{
    public static void Main()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            foreach (var resource in assembly.GetManifestResourceNames())
            {
                Console.WriteLine(resource);
            }
            
            var certPath = ExtractToFile(assembly, "MaiChart2SimaiChart.Installer.Resources.app.cer");
            Console.WriteLine("正在安装证书...");
            Process.Start("certutil", $"-addstore Root \"{certPath}\"").WaitForExit();
            
            var msixPath = ExtractToFile(assembly, "MaiChart2SimaiChart.Installer.Resources.app.msix");
            Console.WriteLine("正在启动 MSIX 安装程序，请按照提示完成安装...");
            
            Process.Start(new ProcessStartInfo
            {
                FileName = msixPath,
                UseShellExecute = true
            });

            Console.WriteLine("请完成安装流程以继续。");
        }
        catch (Exception ex)
        {
            Console.WriteLine("安装失败：" + ex.Message);
        }
    }

    static string ExtractToFile(Assembly assembly, string resourceName)
    {
        var originalFileName = Path.GetFileNameWithoutExtension(resourceName);
        var extension = Path.GetExtension(resourceName);
        
        if (string.IsNullOrEmpty(extension) || extension.Length > 5)
        {
            extension = ".tmp";
        }

        var fileNameWithExt = $"{originalFileName}{extension}";
        
        var tempFile = Path.Combine(Path.GetTempPath(), fileNameWithExt);

        using (Stream resource = assembly.GetManifestResourceStream(resourceName))
        using (FileStream file = File.Create(tempFile))
        {
            resource.CopyTo(file);
        }

        return tempFile;
    }
}