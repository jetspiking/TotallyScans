using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.Enums;
using TotallyScans.Core;
using TotallyScans.Misc;
using WebViewControl;

namespace TotallyScans;

public partial class MainWindow : Window, MenuRibbon.ITopRibbonClick
{
    private const String TextMenuRibbonScan = "SCAN";
    private const String TextMenuRibbonSettings = "SETTINGS";
    private const String TextMenuRibbonWebView = "WEBVIEW";
    private const String TextScanRibbonFile = "FILE";
    private const String TextScanRibbonUrl = "URL/IP";
    private const String TextSettingsDialogSaveTitle = "Settings adjusted";
    private const String TextSettingsDialogSaveDescriptionValid = "Your API key was saved successfully!";
    private const String TextSettingsDialogSaveDescriptionError = "Your API key does not seem valid!";
    private const String TextSettingsDialogRegisterKeyTitle = "No valid API key";
    private const String TextSettingsDialogRegisterKeyDescription = "Please register for a valid API key in \"SETTINGS\" menu!";
    private const String TextErrorTitle = "Error";

    private const String SaveLocationFolder = "TotallyScans";
    private const String SettingsFile = "Settings.json";
    
    private Settings _settings = new Settings(String.Empty);
    private VirusTotalHandler _virusTotalHandler;

    public WebView WebView;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        Initialize();
    }

    private void Initialize()
    {
        ImageUtilities.Initialize();

        this.Icon = new WindowIcon(ImageUtilities.GetBitmapStream(Misc.Image.Images.Icon, Folders.Default));
        this.FontFamily = new FontFamily("Lucida Console");
        this.CanResize = false;
        
        WebView.Settings.LogFile = String.Empty;
        WebView.Settings.OsrEnabled = false;
        WebView.Settings.PersistCache = false;
        this.WebView = new();
        WebViewPanel.Children.Add(this.WebView);
        this.WebView.HorizontalAlignment = HorizontalAlignment.Stretch;
        this.WebView.VerticalAlignment = VerticalAlignment.Stretch;
        
        LoadSettings();

        MenuRibbon.SetProperties(this, null, 19, TextMenuRibbonScan, TextMenuRibbonSettings);
        MenuRibbon.Select(0);

        Bitmap bitmap = new Bitmap(ImageUtilities.GetBitmapStream(Misc.Image.Images.Icon, Folders.Default));
        GreetIcon.Source = bitmap;
        GreetIcon.Stretch = Stretch.UniformToFill;

        ScanRibbon.SetProperties(this, null, 16, TextScanRibbonFile, TextScanRibbonUrl);
        ScanRibbon.Background = new SolidColorBrush(Colors.Transparent);

        BrowseDialogButton.PointerPressed += delegate { OpenBrowseFileDialog(); };

        ApiKeyRegisterButton.PointerPressed += delegate
        {
            VirusTotalHandler.OpenRegisterUrl(this);
            OpenMainMenu(TextMenuRibbonWebView);
        };

        SaveSettingsButton.PointerPressed += delegate
        {
            SaveSettings();
        };

        ScanUrlButton.PointerPressed += delegate
        {
            HandleUrlClick();
        };
    }

    private async void HandleUrlClick()
    {
        OpenMainMenu(TextMenuRibbonWebView);
            
        BrowseDialog.IsVisible = false;

        try
        {
            await this._virusTotalHandler.ScanUrl(UrlBox.Text, this);
        }
        catch (Exception e)
        {
            IMsBoxWindow<ButtonResult> buttonResult = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(TextErrorTitle,
                e.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Success);

            await buttonResult.ShowDialog(this);
        }
    }

    private async void OpenBrowseFileDialog()
    {
        OpenFileDialog fileDialog = new()
        {
            AllowMultiple = false
        };

        String[]? result = await fileDialog.ShowAsync(this);
        if (result == null || result.Length<1) return;

        BrowseDialog.IsVisible = false;

        try
        {
            await this._virusTotalHandler.ScanFile(result[0], this);
        }
        catch (Exception e)
        {
            IMsBoxWindow<ButtonResult> buttonResult = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(TextErrorTitle,
                e.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Success);

            await buttonResult.ShowDialog(this);
        }
        
        OpenMainMenu(TextMenuRibbonWebView);
    }

    void MenuRibbon.ITopRibbonClick.Click(String button)
    {
        OpenMainMenu(button);
        
        switch (button)
        {
            case TextScanRibbonFile:
                OpenFileMenu();
                break;
            case TextScanRibbonUrl:
                OpenUrlMenu();
                break;
        }
    }

    private void OpenMainMenu(String menu)
    {
        switch (menu)
        {
            case TextMenuRibbonScan:
                ScanPanel.IsVisible = true;
                SettingsPanel.IsVisible = false;
                WebViewPanel.IsVisible = false;
                OpenGreetMenu();
                break;
            case TextMenuRibbonSettings:
                ScanPanel.IsVisible = false;
                SettingsPanel.IsVisible = true;
                WebViewPanel.IsVisible = false;
                break;
            case TextMenuRibbonWebView:
                ScanPanel.IsVisible = false;
                SettingsPanel.IsVisible = false;
                WebViewPanel.IsVisible = true;
                break;
        }
    }

    private void OpenFileMenu()
    {
        ScanGreetPanel.IsVisible = false;
        ScanFilePanel.IsVisible = true;
        ScanUrlPanel.IsVisible = false;

        BrowseDialog.IsVisible = true;
    }

    private void OpenUrlMenu()
    {
        ScanGreetPanel.IsVisible = false;
        ScanFilePanel.IsVisible = false;
        ScanUrlPanel.IsVisible = true;
    }

    private void OpenGreetMenu()
    {
        ScanGreetPanel.IsVisible = true;
        ScanFilePanel.IsVisible = false;
        ScanUrlPanel.IsVisible = false;
        ScanRibbon.Select(-1);
    }

    private async Task<Boolean> InitializeVirusTotal()
    {
        this._virusTotalHandler = new VirusTotalHandler(this._settings.VirusTotalApiKey);

        if (this._virusTotalHandler.IsKeyValid) return true;
        
        IMsBoxWindow<ButtonResult> buttonResult = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(TextSettingsDialogRegisterKeyTitle,
            TextSettingsDialogRegisterKeyDescription, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Info);

        await buttonResult.ShowDialog(this);
        return true;
    }

    private async void SaveSettings()
    {
        this._settings.VirusTotalApiKey = ApiTextBox.Text;
        
        if (!await InitializeVirusTotal())
        {
            IMsBoxWindow<ButtonResult> errorResult = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(TextSettingsDialogSaveTitle,
            TextSettingsDialogSaveDescriptionError, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
            await errorResult.Show(this);
            return;
        };

        String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SaveLocationFolder);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyComputer), SettingsFile);
        JSONManager.SerializeToFile(this._settings, path);

        IMsBoxWindow<ButtonResult> buttonResult = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(TextSettingsDialogSaveTitle,
            TextSettingsDialogSaveDescriptionValid, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Success);

        await buttonResult.Show(this);
    }

    private async void LoadSettings()
    {
        String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            SaveLocationFolder);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return;
        }

        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyComputer), SettingsFile);
        this._settings = JSONManager.DeserializeFromFile<Settings>(path) ?? new Settings(String.Empty);

        ApiTextBox.Text = this._settings.VirusTotalApiKey;
        await InitializeVirusTotal();
    }
}