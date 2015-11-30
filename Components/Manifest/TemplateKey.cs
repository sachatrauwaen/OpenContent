namespace Satrabel.OpenContent.Components.Manifest
{
    public class TemplateKey
    {
        private readonly string _folder;

        public TemplateKey(FileUri templateUri)
        {
            _folder = templateUri.FolderPath;
            Key = templateUri.FileNameWithoutExtension;
            Extention = templateUri.Extension == "" ? "manifest" : templateUri.Extension;
        }
        public FolderUri TemplateDir { get { return new FolderUri(_folder); } }
        public string Key { get; private set; }
        public string Extention { get; private set; }
    }
}