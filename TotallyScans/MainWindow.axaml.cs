using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using TotallyScans.Core;
using TotallyScans.Misc;
using WebViewControl;
using MsBox.Avalonia.Base;

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
    private const String TextSettingsDialogRegisterKeyDescription = "Welcome, please register and receive an API key from VirusTotal";
    private const String TextErrorTitle = "Error";
    private const String TextConfigurationTitle = "Configuration";

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
            IMsBox<ButtonResult> buttonResult = MessageBoxManager.GetMessageBoxStandard(TextErrorTitle, this._virusTotalHandler.IsKeyValid ? e.Message : TextSettingsDialogSaveDescriptionError, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await buttonResult.ShowAsPopupAsync(this);
            OpenMainMenu(TextMenuRibbonSettings);
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
            IMsBox<ButtonResult> buttonResult = MessageBoxManager.GetMessageBoxStandard(TextErrorTitle, this._virusTotalHandler.IsKeyValid ? e.Message : TextSettingsDialogSaveDescriptionError, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await buttonResult.ShowAsPopupAsync(this);
            OpenMainMenu(TextMenuRibbonSettings);
            return;
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
                MenuRibbon.Select(0);
                break;
            case TextMenuRibbonSettings:
                ScanPanel.IsVisible = false;
                SettingsPanel.IsVisible = true;
                WebViewPanel.IsVisible = false;
                MenuRibbon.Select(1);
                break;
            case TextMenuRibbonWebView:
                ScanPanel.IsVisible = false;
                SettingsPanel.IsVisible = false;
                WebViewPanel.IsVisible = true;
                MenuRibbon.Select(-1);
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
    }

    private async Task<Boolean> InitializeVirusTotal()
    {
        this._virusTotalHandler = new VirusTotalHandler(this._settings.VirusTotalApiKey);

        if (this._virusTotalHandler.IsKeyValid) return true;

        IMsBox<ButtonResult> buttonResult = MessageBoxManager.GetMessageBoxStandard(TextSettingsDialogRegisterKeyTitle, TextSettingsDialogRegisterKeyDescription, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
        await buttonResult.ShowAsPopupAsync(this);
        OpenMainMenu(TextMenuRibbonSettings);
        return true;
    }

    private async void SaveSettings()
    {
        this._settings.VirusTotalApiKey = ApiTextBox.Text;

        Boolean isInitialized = await InitializeVirusTotal();
        
        if (!isInitialized)
        {
            IMsBox<ButtonResult> errorResult = MessageBoxManager.GetMessageBoxStandard(TextSettingsDialogSaveTitle, TextSettingsDialogSaveDescriptionError, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await errorResult.ShowAsPopupAsync(this);
            return;
        };

        String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SaveLocationFolder);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyComputer), SettingsFile);
        JSONManager.SerializeToFile(this._settings, path);

        IMsBox<ButtonResult> buttonResult = MessageBoxManager.GetMessageBoxStandard(TextSettingsDialogSaveTitle, TextSettingsDialogSaveDescriptionValid, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
        await buttonResult.ShowAsPopupAsync(this);
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

        try
        {
            await InitializeVirusTotal();
        } catch(Exception e)
        {
            IMsBox<ButtonResult> errorResult = MessageBoxManager.GetMessageBoxStandard(TextConfigurationTitle, TextSettingsDialogRegisterKeyDescription, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await errorResult.ShowAsPopupAsync(this);
            OpenMainMenu(TextMenuRibbonSettings);
        }
    }
}