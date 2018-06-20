using DotNetNuke.Services.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Satrabel.OpenContent.Components.FileIndexer
{
    public class TextFileIndexer : IFileIndexer
    {
        public bool CanIndex(string file)
        {
            int fileId = 0;
            if (int.TryParse(file, out fileId))
            {
                var f = FileManager.Instance.GetFile(fileId);
                if (f == null) return false;
                return f.Extension == "txt";
            }
            else
            {
                var f = FileUri.FromPath(file);
                if (f == null) return false;
                return f.Extension == ".txt";
            }
        }

        public string GetContent(string file)
        {
            int fileId = 0;
            if (int.TryParse(file, out fileId))
            {
                var f = FileManager.Instance.GetFile(fileId);
                if (f != null)
                {
                    var fileContent = FileManager.Instance.GetFileContent(f);
                    if (fileContent != null)
                    {
                        using (var reader = new StreamReader(fileContent, Encoding.UTF8))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
                return "";
            }
            else
            {
                var f = FileUri.FromPath(file);
                if (f.FileExists)
                {
                    return File.ReadAllText(f.PhysicalFilePath);
                }
                return "";
            }
        }
    }
}