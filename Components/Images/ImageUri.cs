using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;

namespace Satrabel.OpenContent.Components.Images
{
    public class ImageUri : FileUri
    {
        #region Constructors

        private ImageUri(string pathToFile)
            : base(pathToFile)
        {
            //Don't use this constructor in this class
        }

        private ImageUri(string path, string filename)
            : base(path, filename)
        {
        }

        public ImageUri(int fileId)
            : base(fileId)
        {
        }

        public ImageUri(string filename, int portalid)
            : base(filename)
        {
            FileInfo = ToIFileInfo(portalid);
        }

        #endregion

        public string GetImageUrl(float columnWidth, string ratioString, bool isMobile)
        {
            if (columnWidth < 0 || columnWidth > 1) columnWidth = 1;
            if (string.IsNullOrEmpty(ratioString)) ratioString = "1x1";
            var ratio = new Ratio(ratioString);
            var maxWidth = ImageUtils.CalculateMaxPixels(columnWidth, isMobile);
            ratio.SetWidth(maxWidth);
            return ImageUtils.GetImageUrl(FileInfo, ratio);
        }

        public string GetImageUrl(string ratioString, float columnHeight, bool isMobile)
        {
            if (columnHeight < 0 || columnHeight > 1) columnHeight = 1;
            if (string.IsNullOrEmpty(ratioString)) ratioString = "1x1";
            var ratio = new Ratio(ratioString);
            var maxHeight = ImageUtils.CalculateMaxPixels(columnHeight, isMobile);
            ratio.SetHeight(maxHeight);
            return ImageUtils.GetImageUrl(FileInfo, ratio);
        }


        public string EditLink(int tabFileManager)
        {
            {
                if (tabFileManager <= 0) return "";
                var url = Globals.NavigateURL(tabFileManager);
                return string.Format("javascript:dnnModal.show('{0}/ctl/FileProperties/mid/1420?popUp=true&fileId={1}')",url, FileInfo.FileId);
            }
        }

        #region Private Methods

        private IFileInfo ToIFileInfo(int portalid)
        {
            IFileInfo fileRequested = null;
            var pf = (new PortalController()).GetPortal(portalid).HomeDirectory;
            var pos = FilePath.IndexOf("/" + pf, StringComparison.Ordinal);
            if (pos > -1)
            {
                fileRequested = FileManager.Instance.GetFile(portalid, FilePath.Substring(pos + pf.Length + 2));
            }
            return fileRequested;
        }

        #endregion

        //public static string FileUrl(int fileId)
        //{
        //    if (fileId < 0) return "";
        //    var fileManager = FileManager.Instance;
        //    IFileInfo iFile = fileManager.GetFile(fileId);
        //    if (iFile == null) throw new NoNullAllowedException(string.Format("iFileInfo not found for id [{0}]", fileId));
        //    return fileManager.GetUrl(iFile);
        //}

        //public static IFileInfo FileInfo(int fileid)
        //{
        //    return FileManager.Instance.GetFile(fileid);
        //}


    }
}