using System;
using System.IO;
using System.Web.Hosting;
using DotNetNuke.Services.FileSystem;

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
                throw new ArgumentNullException("pathToFile");
            }
            _fileName = Path.GetFileName(NormalizePath(pathToFile));
            if (string.IsNullOrEmpty(_fileName))
            {
                throw new ArgumentNullException("pathToFile");
            }
            var cleanfile = SanitizeFilename(_fileName);
            if (!_fileName.Equals(cleanfile, StringComparison.InvariantCultureIgnoreCase))
            {
                Log.Logger.WarnFormat("Original filename [{0}] was Sanitized into [{1}]", _fileName, cleanfile);
                _fileName = cleanfile;
            }
        }

        public FileUri(string path, string filename)
            : base(path)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }
            _fileName = SanitizeFilename(filename);
        }
        public FileUri(FolderUri path, string filename)
            : base(path.FolderPath)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }
            _fileName = SanitizeFilename(filename);
        }

        #endregion


        /// <summary>
        /// Gets the file path relative to the Application. No leading /.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public string FilePath { get { return base.FolderPath + "/" + FileName; } }

        public string UrlFilePath { get { return base.UrlPath + "/" + FileName; } }


        /// <summary>
        /// Gets the full physical file path.
        /// </summary>
        /// <value>
        /// The physical file path.
        /// </value>
        public string PhysicalFilePath
        {
            get
            {
                return HostingEnvironment.MapPath("~/" + FilePath);
            }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public string FileNameWithoutExtension
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(FilePath);
            }
        }
        public string Extension
        {
            get { return System.IO.Path.GetExtension(FilePath); }
        }

        public bool FileExists
        {
            get
            {
                return File.Exists(PhysicalFilePath);
            }
        }

        private static string SanitizeFilename(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '-');
            }
            return fileName; //string.Concat(fileName.Split(Path.GetInvalidFileNameChars()));
        }

        #region Static Utils

        public static FileUri FromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
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