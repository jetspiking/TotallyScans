using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.Enums;
using TotallyScans.Misc;
using VirusTotalNet;
using VirusTotalNet.Objects;
using VirusTotalNet.ResponseCodes;
using VirusTotalNet.Results;
using WebViewControl;

namespace TotallyScans.Core;

public class VirusTotalHandler
{
    private const String VirusTotalRegisterUrl = "https://www.virustotal.com/gui/join-us";
    public VirusTotal? VirusTotal { get; set; }
    public Boolean IsKeyValid { get; set; } = false;

    public VirusTotalHandler(String apiKey)
    {
        try
        {
            this.VirusTotal = new VirusTotal(apiKey);
            this.VirusTotal.UseTLS = true;
            this.IsKeyValid = true;
        }
        catch (Exception e) {}
    }
    
    public async Task ScanFile(String file, MainWindow mainWindow)
    {
        ScanResult fileResult = await this.VirusTotal.ScanFileAsync(file);
        mainWindow.WebView.Address = fileResult.Permalink;
    }

    public async Task ScanUrl(String url, MainWindow mainWindow)
    { 
        UrlScanResult urlResult = await this.VirusTotal.ScanUrlAsync(url);
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