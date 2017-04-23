using System;
using System.IO;
using System.Web.Hosting;

namespace Satrabel.OpenContent.Components
{
    public class FileUri : FolderUri
    {
        private readonly string _fileName;

        #region Constructors

        public FileUri(string pathToFile)
            : base(System.IO.Path.GetDirectoryName(pathToFile))
        {
            if (string.IsNullOrEmpty(pathToFile))
            {
                throw new ArgumentNullException(nameof(pathToFile));
            }
            _fileName = Path.GetFileName(NormalizePath(pathToFile));
            if (string.IsNullOrEmpty(_fileName))
            {
                throw new ArgumentNullException(nameof(pathToFile));
            }
        }
        public FileUri(string path, string filename)
            : base(path)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }
            _fileName = filename;
        }
        public FileUri(FolderUri path, string filename)
            : base(path.FolderPath)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }
            _fileName = filename;
        }

        #endregion


        /// <summary>
        /// Gets the file path relative to the Application. No leading /.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public string FilePath => base.FolderPath + "/" + FileName;

        public string UrlFilePath => base.UrlPath + "/" + FileName;

        /// <summary>
        /// Gets the full physical file path.
        /// </summary>
        /// <value>
        /// The physical file path.
        /// </value>
        public string PhysicalFilePath => HostingEnvironment.MapPath("~/" + FilePath);

        public string FileName => _fileName;

        public string FileNameWithoutExtension => System.IO.Path.GetFileNameWithoutExtension(FilePath);
        public string Extension => System.IO.Path.GetExtension(FilePath);

        public bool FileExists => File.Exists(PhysicalFilePath);

        #region Static Utils

        public static FileUri FromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            var appPath = HostingEnvironment.MapPath("~");
            if (appPath == null)
            {
                throw new Exception("Failed to create FileUri. Could not determine AppPath.");
            }
            var file = $"{path.Replace(appPath, "").Replace("\\", "/")}";
            return new FileUri(file);
        }

        #endregion

    }
}