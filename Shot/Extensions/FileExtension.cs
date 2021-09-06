using System;
using System.IO;

namespace Shot.Extensions
{
    public static class FileExtension
    {
        public static string GetFilePath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            var filePathDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "recordings");
            if (!File.Exists(filePathDir))
            {
                Directory.CreateDirectory(filePathDir);
            }
            var trimmedName = fileName.Replace(" ", "-");
            return Path.Combine(filePathDir, trimmedName + ".3gpp");
        }
    }
}
