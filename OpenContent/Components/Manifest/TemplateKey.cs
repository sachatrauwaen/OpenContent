namespace Satrabel.OpenContent.Components.Manifest
{
    public class TemplateKey
    {
        private readonly string _folder;

        public TemplateKey(FileUri templateUri)
        {
            _folder = templateUri.FolderPath;
            ShortKey = templateUri.FileNameWithoutExtension;
            Extention = templateUri.Extension == "" ? "manifest" : templateUri.Extension;
        }
        public TemplateKey(TemplateKey templateKey, string shortKey, string extension = "")
        {
            _folder = templateKey.Folder;
            ShortKey = shortKey;
            Extention = extension == "" ? templateKey.Extention : extension;
        }
        public FolderUri TemplateDir => new FolderUri(_folder);
        public string ShortKey { get; private set; }
        public string Extention { get; private set; }
        public string Folder => _folder;

        public override string ToString()
        {
            if (Extention == "manifest")
            {
                return _folder + "/" + ShortKey;
            }
            return _folder + "/" + ShortKey + Extention;
        }
    }
}