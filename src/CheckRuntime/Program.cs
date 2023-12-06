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
        //TODO：这里注册表判断不准确，抽空再看
        if (IsDotNetInstalled(dotnetVersion))
        {
            Console.WriteLine($".NET {dotnetVersion} 已安装！");
        }
        else
        {
            Console.WriteLine($"正在下载并安装...");
            await DownloadAndInstallDotNet(dotnetVersion);
        }
        //TODO：这里注册表判断不准确，抽空再看
        if (IsWebView2Installed())
        {
            Console.WriteLine("WebView2 已安装！");
        }
        else
        {
            Console.WriteLine("正在下载并安装...");
            await DownloadAndInstallWebView2();
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
                Console.Write($"\r.NET 下载进度：{e.ProgressPercentage}%");
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
        Console.WriteLine("安装完成");
    }

    static string GetDotNetRegistryPath(string version, string osArchitecture)
    {
        return $"SOFTWARE\\dotnet\\Setup\\InstalledVersions\\{version}";
    }

    static bool IsWebView2Installed()
    {
        const string webView2RegistryPath = @"SOFTWARE\Microsoft\EdgeUpdate\Clients\{A8A19650-6F6D-4CBF-A64F-8C3196A5151D}";

        using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(webView2RegistryPath))
        {
            return registryKey != null;
        }
    }

    static async Task DownloadAndInstallWebView2()
    {
        string osArchitecture = Environment.Is64BitOperatingSystem ? "x64" : "x86";
        if (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") == "ARM64")
        {
            osArchitecture = "Arm64";
        }

        string webView2InstallerUrl = $"https://go.microsoft.com/fwlink/p/?LinkId=2124703&os={Environment.OSVersion.Platform.ToString().ToLower()}&arch={osArchitecture}";

        string installerFileName = "WebView2RuntimeInstaller.exe";

        using (WebClient webClient = new WebClient())
        {
            webClient.DownloadProgressChanged += (sender, e) =>
            {
                Console.Write($"\rWebView2 下载进度：{e.ProgressPercentage}%");
            };

            await webClient.DownloadFileTaskAsync(new Uri(webView2InstallerUrl), installerFileName);
            Console.WriteLine(); // 换行，使输出更整洁
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = installerFileName,
            UseShellExecute = true,
            Verb = "runas" // Run as administrator
        };

        Console.WriteLine("开始安装 WebView2，请稍候...");
        Process installerProcess = Process.Start(startInfo);
        installerProcess.WaitForExit();

        File.Delete(installerFileName);
        Console.WriteLine("安装完成");
    }
}
