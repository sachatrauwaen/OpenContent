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

        //private static string SanitizeFilenameKeepingTheOptionalVersion(string fileName)
        //{
        //    if (string.IsNullOrEmpty(fileName)) return fileName;
        //    var version = fileName.TrimStart("?ver=");
        //    var cleanfile = SanitizeFilename(fileName.TrimEnd("?ver="));

        //    return cleanfile + version;
        //}

        //private static string SanitizeFilename(string fileName)
        //{
        //    if (string.IsNullOrEmpty(fileName)) return fileName;
        //    var cleanfile = fileName;
        //    foreach (var c in Path.GetInvalidFileNameChars())
        //    {
        //        cleanfile = cleanfile.Replace(c, '-');
        //    }
        //    if (!fileName.Equals(cleanfile, StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        Log.Logger.InfoFormat("Original filename [{0}] was Sanitized into [{1}]", fileName, cleanfile);
        //    }
        //    return cleanfile;
        //}

        #region Static Utils

        public static FileUri FromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            string appPath = HostingEnvironment.MapPath("~");
            string file = string.Format("{0}", path.Replace(appPath, "").Replace("\\", "/"));
            //if (!res.StartsWith("/")) res = "/" + res;
            if (file != null)
            {
                return new FileUri(file);
            }
            return null;
        }

        #endregion

    }
}