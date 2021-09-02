using System;
using System.Globalization;
using System.IO;

namespace Shot.Extensions
{
    public static class MediaExtension
    {
        public static string GetCreationTime(string filePath)
        {
            return File.GetCreationTime(filePath).ToString("ddd d MMM", CultureInfo.CurrentCulture);
        }
    }
}
