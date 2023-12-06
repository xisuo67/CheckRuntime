using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string dotnetVersion = "6.0.25"; // 你想要安装的.NET版本

        if (IsDotNetInstalled(dotnetVersion))
        {
            Console.WriteLine($".NET {dotnetVersion} 已安装！");
        }
        else
        {
            Console.WriteLine($".NET {dotnetVersion} 未安装，正在下载并安装...");
            await DownloadAndInstallDotNet(dotnetVersion);
        }

        Console.ReadLine();
    }

    static bool IsDotNetInstalled(string version)
    {
        string osArchitecture = Environment.Is64BitOperatingSystem ? "x64" : "x86";
        if (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") == "ARM64")
        {
            osArchitecture = "Arm64";
        }

        string registryPath = GetDotNetRegistryPath(version, osArchitecture);

        using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(registryPath))
        {
            return registryKey != null;
        }
    }

    static async Task DownloadAndInstallDotNet(string version)
    {
        string osArchitecture = Environment.Is64BitOperatingSystem ? "x64" : "x86";
        if (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") == "ARM64")
        {
            osArchitecture = "Arm64";
        }

        string dotnetInstallerUrl = $"https://download.visualstudio.microsoft.com/download/pr/52d6ef78-d4ec-4713-9e01-eb8e77276381/e58f307cda1df61e930209b13ecb47a4/windowsdesktop-runtime-{version}-win-{osArchitecture}.exe";

        string installerFileName = $"dotnet-runtime-{version}-installer.exe";

        using (WebClient webClient = new WebClient())
        {
            webClient.DownloadProgressChanged += (sender, e) =>
            {
                Console.Write($"\r下载进度：{e.ProgressPercentage}%");
            };

            await webClient.DownloadFileTaskAsync(new Uri(dotnetInstallerUrl), installerFileName);
            Console.WriteLine(); // 换行，使输出更整洁
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = installerFileName,
            UseShellExecute = true,
            Verb = "runas" // Run as administrator
        };

        Console.WriteLine($"开始安装.NET {version}，请稍候...");
        Process installerProcess = Process.Start(startInfo);
        installerProcess.WaitForExit();

        File.Delete(installerFileName);

        if (IsDotNetInstalled(version))
        {
            Console.WriteLine($".NET {version} 安装完成！");
        }
        else
        {
            Console.WriteLine($".NET {version} 安装失败，请手动安装。");
        }
    }

    static string GetDotNetRegistryPath(string version, string osArchitecture)
    {
        return $"SOFTWARE\\dotnet\\Setup\\InstalledVersions\\{version}";
    }
}
