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

public partial class ScanResultFragment : UserControl
{
    public ScanResultFragment()
    {
        InitializeComponent();
    }
}