using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components
{
    public class TemplateKey
    {
        private readonly string _folder;

        public TemplateKey(string templateFolder, string templateKey, string extention)
        {
            _folder = templateFolder;
            Key = templateKey;
            Extention = extention;
        }

        public FolderUri TemplateDir { get { return new FolderUri(_folder); } }
        public string Key { get; private set; }
        public string Extention { get; private set; }
    }
}