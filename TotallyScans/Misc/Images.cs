using System;

namespace TotallyScans.Misc
{
    /// <summary>
    /// Provides a list of images that can be used in this project.
    /// </summary>
    public class Image
    {
        private const String Icon = "Icon.png";

        public static String? GetImage(Images image)
        {
            switch (image)
            {
                case Images.Icon:
                    return Icon;
            }
            return null;
        }

        public enum Images
        {
            Icon,
            Check,
            Cross
        }
    }
}
