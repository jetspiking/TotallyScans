using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using VirusTotalNet;
using VirusTotalNet.Results;

namespace TotallyScans.Core;

public class VirusTotalHandler
{
    private const String VirusTotalRegisterUrl = "https://www.virustotal.com/gui/join-us";
    public VirusTotal? VirusTotal { get; set; }
    public Boolean IsKeyValid { get; set; } = false;

    public VirusTotalHandler(String apiKey)
    {
        this.VirusTotal = new VirusTotal(apiKey);
        this.VirusTotal.UseTLS = true;
        this.IsKeyValid = true;
    }

    public async Task ScanFile(String file, MainWindow mainWindow, int count = 0)
    {
        String? resultUrl = String.Empty;
        FileReport fileResult = await this.VirusTotal.GetFileReportAsync(new FileInfo(file));
        if (fileResult == null)
        {
            ScanResult rescanResult = await this.VirusTotal.ScanFileAsync(file);
            resultUrl = rescanResult.Permalink;
        }
        else resultUrl = fileResult.Permalink;
        if (resultUrl != String.Empty && resultUrl != null)
        {
            mainWindow.WebView.Address = resultUrl;
            return;
        }
        if (count >= 1)
        {
            mainWindow.WebView.LoadHtml("<!DOCTYPE html><html><head><title>Processing File</title></head><body><p>File is now being processed, please retrieve the result later by rescanning the file.</p></body></html>");
            return;
        }
        await Task.Delay(1000);
        await ScanFile(file, mainWindow, ++count);
    }

    public async Task ScanUrl(String url, MainWindow mainWindow)
    {
        UrlReport urlResult = await this.VirusTotal.GetUrlReportAsync(url, true);
        mainWindow.WebView.Address = urlResult.Permalink;
    }

    public async void ScanIp(String ip, MainWindow mainWindow)
    {
        UrlScanResult ipResult = await this.VirusTotal.ScanUrlAsync(ip);
        mainWindow.WebView.Address = ipResult.Permalink;
    }

    private static void OpenUrlInWebView(String url, MainWindow mainWindow)
    {
        mainWindow.WebView.Address = url;
    }

    private static void OpenUrl(String url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                String tempUrl = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(tempUrl) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }

    public static void OpenRegisterUrl(MainWindow mainWindow)
    {
        OpenUrlInWebView(VirusTotalRegisterUrl, mainWindow);
    }
}