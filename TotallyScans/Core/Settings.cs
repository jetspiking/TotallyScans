using System;

namespace TotallyScans.Core;

/// <summary>
/// Application settings.
/// </summary>
public class Settings
{
    public String VirusTotalApiKey { get; set; }

    public Settings(string virusTotalApiKey)
    {
        this.VirusTotalApiKey = virusTotalApiKey;
    }
}