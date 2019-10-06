﻿using System;
using System.IO;
using System.Web.Hosting;

namespace Satrabel.OpenContent.Components
{
    public class FolderUri
    {
        #region Constructors

        public FolderUri(string pathToFolder)
        {
            if (string.IsNullOrEmpty(pathToFolder))
            {
                throw new ArgumentNullException(nameof(pathToFolder));
            }
            FolderPath = NormalizePath(pathToFolder);
        }

        #endregion

        /// <summary>
        /// Gets the folder path relative to the Application. No leading /.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public string FolderPath { get; private set; }

        protected string UrlPath => NormalizedApplicationPath + FolderPath;

        /// <summary>
        /// Gets the URL directory relative to the root of the webserver. With leading / and trailing /.
        /// </summary>
        /// <value>
        /// The URL folder.
        /// </value>
        public string UrlFolder => UrlPath.TrimEnd('/') + "/";

        public string PhysicalFullDirectory => HostingEnvironment.MapPath("~/" + FolderPath);

        public bool FolderExists => Directory.Exists(PhysicalFullDirectory);

        public FolderUri ParentFolder
        {
            get
            {
                try
                {
                    var pos = FolderPath.LastIndexOf('/');
                    if (pos == -1) return null;
                    return new FolderUri(FolderPath.Substring(0, pos + 1));
                }
                catch (Exception ex)
                {
                   App.Services.Logger.Error($"Can't get ParentFolder from {FolderPath}", ex);
                }
                return null;
            }
        }

        public FolderUri Append(string path)
        {
            return new FolderUri(FolderPath + "/" + path.Trim('/'));
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
                throw new ArgumentNullException("path");
            }
            string appPath = HostingEnvironment.MapPath("~");
            string file = $"{path.Replace(appPath, "").Replace("\\", "/")}";
            if (!file.StartsWith("/")) file = "/" + file;
            return file;
        }

        #endregion

        #region Private Methods

        protected static string NormalizePath(string filePath)
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