using System;
using System.IO;
using Newtonsoft.Json;

namespace TotallyScans.Misc;

/// <summary>
/// Provides a generic implementation for serializing and deserializing to and from JSON.
/// </summary>
public static class JSONManager
{
    public static void SerializeToFile<T>(T obj, String path)
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(obj));
    }

    public static T? DeserializeFromFile<T>(String path)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }
        catch
        {
            return default(T);
        }
    }
}