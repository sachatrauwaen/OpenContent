﻿using System;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;

namespace Satrabel.OpenContent.Components
{
    public class PortalFolderUri : FolderUri
    {
        #region Constructors

        public PortalFolderUri(int portalid, string pathToFolder)
            : base(pathToFolder)
        {
            FolderInfo = GetFolderInfo(portalid);
        }
        public PortalFolderUri(IFolderInfo folderInfo)
            : base(GetFolderPath(folderInfo))
        {
            FolderInfo = folderInfo;
        }
        public PortalFolderUri(int folderId)
            : base(GetFolderPath(folderId))
        {
            var folderInfo = FolderManager.Instance.GetFolder(folderId);
            if (folderInfo == null)
                throw new ArgumentNullException($"iFolderInfo not found for id [{folderId}]");

            FolderInfo = folderInfo;
        }
        private IFolderInfo GetFolderInfo(int portalid)
        {
            IFolderInfo folderRequested = null;
            //var portalid = PortalSettings.Current.PortalId;
            var pf = (new PortalController()).GetPortal(portalid).HomeDirectory;
            var pos = FolderPath.IndexOf(pf, StringComparison.InvariantCultureIgnoreCase);
            if (pos > -1)
            {
                folderRequested = FolderManager.Instance.GetFolder(portalid, FolderPath.Substring(pos + pf.Length + 1));
            }
            return folderRequested;
        }
        private static string GetFolderPath(int folderId)
        {
            IFolderInfo folderInfo = FolderManager.Instance.GetFolder(folderId);
            if (folderInfo == null)
                throw new ArgumentNullException($"iFolderInfo not found for id [{folderId}]");
            return GetFolderPath(folderInfo);
        }
        private static string GetFolderPath(IFolderInfo folderInfo)
        {
            if (folderInfo == null)
                throw new ArgumentNullException("folderInfo");
            return NormalizePath(folderInfo.FolderPath);
        }
        #endregion

        /// <summary>
        /// Gets or sets the Dnn folder information object.
        /// </summary>
        /// <value>
        /// The Dnn folder information object.
        /// </value>
        /// <remarks>This is only available for folders under the Dnn Portal Directory</remarks>
        public IFolderInfo FolderInfo { get; private set; }

        public int DnnFolderId => FolderInfo == null ? 0 : FolderInfo.FolderID;
    }
}