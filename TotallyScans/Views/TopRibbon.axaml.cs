using System;
using System.Collections.Generic;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using TotallyScans.Misc;
using Image = TotallyScans.Misc.Image;

namespace TotallyScans;

public partial class MenuRibbon : UserControl
{
    private ITopRibbonClick? _iTopRibbonClick;
    private Label? _selectedLabel;
    private readonly List<Label> _labels = new List<Label>();
    private int _fontSize = 18;

    public interface ITopRibbonClick
    {
        void Click(String button);
    }

    public MenuRibbon()
    {
        InitializeComponent();
    }

    public void SetProperties(ITopRibbonClick topRibbonClick, IImage? imageSource, int fontSize, params String[] buttons)
    {
        this._fontSize = fontSize;
        this._iTopRibbonClick = topRibbonClick;
        
        if (imageSource != null)
        {
            RibbonImage.Source = imageSource;
        }
        else RibbonImage.IsVisible = false;

        foreach (String button in buttons)
            AddRibbonButton(button);
    }

    public void Select(int index)
    {
        if (index != -1)
            SelectButton(this._labels[index]);
        else if (this._selectedLabel != null)
        {
            DeselectButton(this._selectedLabel);
            this._selectedLabel = null;
        }
    }

    private void SelectButton(Label label)
    {
        if (this._selectedLabel != null)
        {
            DeselectButton(this._selectedLabel);
            this._selectedLabel = null;
        }

        label.BorderBrush = new SolidColorBrush(Colors.White);
        this._selectedLabel = label;
    }

    private void DeselectButton(Label label)
    {
        label.BorderBrush = new SolidColorBrush(Colors.Transparent);
    }

    private void HoverButton(Label label)
    {
        label.BorderBrush = new SolidColorBrush(Colors.White);
    }

    private void ExitButton(Label label)
    {
        if (this._selectedLabel != label)
            DeselectButton(label);
    }

    private void AddRibbonButton(String button)
    {
        Label label = new()
        {
            Foreground = new SolidColorBrush(Colors.White),
            Content = button,
            FontSize = this._fontSize,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 30, 0),
            BorderBrush = new SolidColorBrush(Colors.Transparent),
            BorderThickness = new Thickness(0, 0, 0, 1)
        };
        this._labels.Add(label);
        this.RibbonPanel.Children.Add(label);
        label.PointerPressed += delegate { label.Opacity = .6; };
        label.PointerReleased += delegate
        {
            label.Opacity = .8;
            this._iTopRibbonClick?.Click(button);
            SelectButton(label);
        };
        label.PointerEnter += delegate
        {
            label.Opacity = .8;
            HoverButton(label);
        };
        label.PointerLeave += delegate
        {
            label.Opacity = 1.0;
            ExitButton(label);
        };
    }
}