using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;

namespace Satrabel.OpenContent.Components
{
    public class FileUri : FolderUri
    {
        #region Constructors

        public FileUri(string pathToFile) : base(Path.GetDirectoryName(pathToFile))
        {
            if (string.IsNullOrEmpty(pathToFile))
            {
                throw new ArgumentNullException("pathToFile is null");
            }
            FileName = Path.GetFileName(NormalizePath(pathToFile));
        }

        public FileUri(string path, string filename)
            : base(path)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename is null");
            }
            FileName = filename;
        }
        public FileUri(FolderUri path, string filename) : base(path.FolderPath)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename is null");
            }
            FileName = filename;
        }

        protected FileUri(int fileId) : base(GetFileInfo(fileId))
        {
            FileInfo = GetFileInfo(fileId);
            var filePath = FileManager.Instance.GetUrl(FileInfo);
            filePath = NormalizePath(filePath);
            FileName = Path.GetFileName(filePath);
        }

        private static IFileInfo GetFileInfo(int fileId)
        {
            var fileInfo = FileManager.Instance.GetFile(fileId);
            if (fileInfo == null)
                throw new ArgumentNullException(string.Format("iFileInfo not found for id [{0}]", fileId));

            return fileInfo;
        }

        #endregion

        /// <summary>
        /// Gets or sets the Dnn file information object.
        /// </summary>
        /// <value>
        /// The Dnn file information object.
        /// </value>
        /// <remarks>This is only available for files under the Dnn Portal Directory</remarks>
        public IFileInfo FileInfo { get; protected set; }

        /// <summary>
        /// Gets the file path relative to the Application. No leading /.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public string FilePath { get { return base.FolderPath + "/" + FileName; } }

        public string UrlFilePath { get { return base.UrlFolderPath + "/" + FileName; } }


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

        public string FileName { get; private set; }

        public string FileNameWithoutExtension
        {
            get
            {
                return Path.GetFileNameWithoutExtension(FilePath);
            }
        }
        public string Extension
        {
            get { return Path.GetExtension(FilePath); }
        }

        public bool FileExists
        {
            get
            {
                return File.Exists(PhysicalFilePath);
            }
        }

        #region Static Utils

        public static FileUri FromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path is null");
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

        #region Private Methods

        private string NormalizePath(string filePath)
        {
            filePath = filePath.Replace("\\", "/");
            filePath = filePath.Trim('~');
            filePath = filePath.TrimStart(NormalizedApplicationPath);
            filePath = filePath.Trim('/');
            return filePath;
        }

        #endregion
    }
}