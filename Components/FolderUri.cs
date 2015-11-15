using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;

namespace Satrabel.OpenContent.Components
{
    public class FolderUri
    {
        #region Constructors

        public FolderUri(string pathToFolder)
        {
            if (string.IsNullOrEmpty(pathToFolder))
            {
                throw new ArgumentNullException("pathToFolder is null");
            }
            FolderPath = NormalizePath(pathToFolder);
        }

        protected FolderUri(IFileInfo pathToFolder)
        {
            if (pathToFolder == null)
            {
                throw new ArgumentNullException("pathToFolder is null");
            }
            FolderPath = NormalizePath(pathToFolder.Folder);
        }

        #endregion

        /// <summary>
        /// Gets or sets the Dnn file information object.
        /// </summary>
        /// <value>
        /// The Dnn file information object.
        /// </value>
        /// <remarks>This is only available for files under the Dnn Portal Directory</remarks>
        public IFolderInfo FolderInfo { get; protected set; }

        /// <summary>
        /// Gets the file path relative to the Application. No leading /.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public string FolderPath { get; private set; }

        protected string UrlFolderPath
        {
            get
            {
                if (NormalizedApplicationPath == "/" && FolderPath.StartsWith("/")) return FolderPath;
                return NormalizedApplicationPath + FolderPath;
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
                return UrlFolderPath + "/";
            }
        }
        public string PhysicalRelativeDirectory
        {
            get
            {
                return FolderPath.Replace(Path.GetFileName(FolderPath), "");
            }
        }
        public string PhysicalFullDirectory
        {
            get
            {
                return HostingEnvironment.MapPath("~/" + FolderPath);
            }
        }

        public bool FolderExists
        {
            get
            {
                return Directory.Exists(PhysicalFullDirectory);
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