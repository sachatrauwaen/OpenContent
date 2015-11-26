using System;
using System.IO;
using System.Web.Hosting;
using DotNetNuke.Services.FileSystem;

namespace Satrabel.OpenContent.Components
{
    public class FileUri : FolderUri
    {
        #region Constructors

        public FileUri(string pathToFile) : base(System.IO.Path.GetDirectoryName(pathToFile))
        {
            if (string.IsNullOrEmpty(pathToFile))
            {
                throw new ArgumentNullException("pathToFile is null");
            }
            FileName = System.IO.Path.GetFileName(NormalizePath(pathToFile));
        }

        public FileUri(string path, string filename) : base(path)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename is null");
            }
            FileName = filename;
        }
        public FileUri(FolderUri path, string filename) : base(path.Path)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename is null");
            }
            FileName = filename;
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
        /// Gets the file path relative to the Application. No leading /.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public string FilePath { get { return base.Path + "/" + FileName; } }

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

        public string FileName { get; private set; }

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

    }
}