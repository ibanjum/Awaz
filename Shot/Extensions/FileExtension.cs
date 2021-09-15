using System;
using System.Globalization;
using System.IO;

namespace Shot.Extensions
{
    public static class FileExtension
    {
        public static string GetFilePath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            var filePathDir = CreateRecordingDirectory();
            var trimmedName = fileName.Replace(" ", "-");
            return Path.Combine(filePathDir, trimmedName + ".3gpp");
        }

        public static string CreateRecordingDirectory()
        {
            var filePathDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "recordings");
            if (!File.Exists(filePathDir))
            {
                Directory.CreateDirectory(filePathDir);
            }
            return filePathDir;
        }

        public static DateTime GetCreationTime(string filePath)
        {
            return File.GetCreationTime(filePath);
        }

        public static string GetFileSize(string fileName)
        {
            var sizeInBytes = new FileInfo(fileName).Length;

            return string.Format("{0} MB", (sizeInBytes / 1024f / 1024f).ToString("0.00"));
        }

        public static string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName).Remove(0, 1);
        }

        public static string GetEnteredName(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var split = fileName.Split('.');
            var val = split[0].Replace("-", " ");
            char[] letters = val.ToCharArray();
            letters[0] = char.ToUpper(letters[0]);
            return new string(letters);
        }

        public static void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }
    }
}
