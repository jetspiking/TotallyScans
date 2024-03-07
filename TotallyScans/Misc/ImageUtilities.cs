using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Platform;
using static TotallyScans.Misc.Image;

namespace TotallyScans.Misc
{
    /// <summary>
    /// Provides the ability to load an image from inside the project.
    /// </summary>
    public static class ImageUtilities
    {
        public static Stream? GetBitmapStream(Image.Images image, Folders folders)
        {
            return AssetLoader.Open(BuildImageUri(image, folders));
        }
        private static Uri BuildImageUri(Image.Images image, Folders folders)
        {
            return BuildPackUri(Path.Join("Resources", "Images", $"{folders}", $"{Image.GetImage(image)}"));
        }
        private static Uri BuildPackUri(String postFix)
        {
            return new Uri($"avares://{Assembly.GetExecutingAssembly().GetName().Name}/{postFix}");
        }
    }
}
