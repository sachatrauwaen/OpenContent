using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;

namespace Satrabel.OpenContent.Components
{
    public class FileUri
    {
        #region Constructors

        public FileUri(string pathToFile)
        {
            if (string.IsNullOrEmpty(pathToFile))
            {
                throw new ArgumentNullException("pathToFile is null");
            }
            FilePath = pathToFile;
            FilePath = NormalizePath(FilePath);
        }

        public FileUri(string path, string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("pathToFile is null");
            }
            FilePath = path + "/" + filename;
            FilePath = NormalizePath(FilePath);
        }

        protected FileUri(int fileId)
        {
            FileInfo = FileManager.Instance.GetFile(fileId);
            if (FileInfo == null)
                throw new ArgumentNullException(string.Format("iFileInfo not found for id [{0}]", fileId));
            FilePath = FileManager.Instance.GetUrl(FileInfo);
            FilePath = NormalizePath(FilePath);
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
        public string FilePath { get; private set; }

        public string UrlFilePath
        {
            get
            {
                if (NormalizedApplicationPath == "/" && FilePath.StartsWith("/")) return FilePath;
                return NormalizedApplicationPath + FilePath;
            }
        }

        /// <summary>
        /// Gets the URL directory relative to the root of the webserver. With leading /.
        /// </summary>
        /// <value>
        /// The URL directory.
        /// </value>
        public string UrlDirectory
        {
            get
            {
                return VirtualPathUtility.GetDirectory(UrlFilePath);
            }
        }
        public string PhysicalRelativeDirectory
        {
            get
            {
                return FilePath.Replace(Path.GetFileName(FilePath), "");
            }
        }
        public string PhysicalFullDirectory
        {
            get
            {
                return Path.GetDirectoryName(PhysicalFilePath);
            }
        }

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
            get
            {
                return Path.GetFileName(FilePath);
            }
        }
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

        /// <summary>
        /// Gets the normalized application path.
        /// </summary>
        /// <remarks>the return value of ApplicationVirtualPath doesn't always return a string that ends with /.</remarks>
        /// <returns></returns>
        public static string NormalizedApplicationPath
        {
            get
            {
                var path = "" + HostingEnvironment.ApplicationVirtualPath;
                if (!path.EndsWith("/")) path += "/";
                return path;
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

        public static string ReverseMapPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path is null");
            }
            string appPath = HostingEnvironment.MapPath("~");
            string file = string.Format("{0}", path.Replace(appPath, "").Replace("\\", "/"));
            if (!file.StartsWith("/")) file = "/" + file;
            return file;
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