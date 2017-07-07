using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Files
{
    public static class FileUriUtils
    {
        public static string ReadFileFromDisk(FileUri file)
        {
            if (file == null) return null;
            if (!file.FileExists) return null;
            string fileContent;
            try
            {
                fileContent = File.ReadAllText(file.PhysicalFilePath);
                if (string.IsNullOrWhiteSpace(fileContent)) return null;
            }
            catch (Exception ex)
            {
                var mess = $"Error reading file [{file.FilePath}]";
                Log.Logger.Error(mess, ex);
                throw new Exception(mess, ex);
            }
            return fileContent;
        }

        public static void WriteFileToDisk(FileUri file, string content)
        {
            File.WriteAllText(file.PhysicalFilePath, content);
        }
    }
}