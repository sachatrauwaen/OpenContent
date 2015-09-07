using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

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
        }

        public FileUri(string path, string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("pathToFile is null");
            }
            FilePath = path + "/" + filename;
        }

        #endregion

        public string FilePath { get; private set; }

        public string Directory
        {
            get
            {
                return VirtualPathUtility.GetDirectory(FilePath);
                //return Path.GetDirectoryName(FilePath);
            }
        }
        public string PhysicalDirectoryName
        {
            get
            {

                return Path.GetDirectoryName(HostingEnvironment.MapPath("~" + FilePath));
            }
        }

        public string PhysicalFilePath
        {
            get
            {
                return HostingEnvironment.MapPath("~" + FilePath);
            }
        }
        public bool FileExists
        {
            get
            {
                return File.Exists(HostingEnvironment.MapPath("~" + FilePath));
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

        public string UrlPath
        {
            get
            {
                if (NormalizedApplicationPath == "/") return FilePath;
                return NormalizedApplicationPath + FilePath;
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

        public static FileUri FromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path is null");
            }
            string appPath = HostingEnvironment.MapPath("~");
            string res = String.Format("{0}", path.Replace(appPath, "").Replace("\\", "/"));
            if (!res.StartsWith("/")) res = "/" + res;
            if (res != null)
            {
                return new FileUri(res);
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
            string res = String.Format("{0}", path.Replace(appPath, "").Replace("\\", "/"));
            if (!res.StartsWith("/")) res = "/" + res;
            return res;
        }
    }
}