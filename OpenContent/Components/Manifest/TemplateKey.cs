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
        public FolderUri TemplateDir { get { return new FolderUri(_folder); } }
        public string ShortKey { get; private set; }
        public string Extention { get; private set; }

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