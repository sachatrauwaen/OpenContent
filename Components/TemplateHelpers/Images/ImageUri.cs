using System;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public class ImageUri : FileUri
    {
        #region Constructors

        public ImageUri(int fileId)
            : base(fileId)
        {
        }

        public ImageUri(string pathToFile, int portalid)
            : base(pathToFile)
        {
            FileInfo = ToIFileInfo(portalid);
        }

        private ImageUri(string pathToFile)
            : base(pathToFile)
        {
            //Don't use this constructor in this class
        }

        private ImageUri(string path, string filename)
            : base(path, filename)
        {
            //Don't use this constructor in this class
        }

        #endregion

        public string GetImageUrl(int width, int height)
        {
            var ratio = new Ratio(width, height);
            return ImageHelper.GetImageUrl(FileInfo, ratio);
        }

        public string GetImageUrl(float columnWidth, string ratioString, bool isMobile)
        {
            if (columnWidth < 0 || columnWidth > 1) columnWidth = 1;
            if (string.IsNullOrEmpty(ratioString)) ratioString = "1x1";
            var ratio = new Ratio(ratioString);
            var maxWidth = ImageHelper.CalculateMaxPixels(columnWidth, isMobile);
            ratio.SetWidth(maxWidth);
            return ImageHelper.GetImageUrl(FileInfo, ratio);
        }

        public string GetImageUrl(string ratioString, float columnHeight, bool isMobile)
        {
            if (columnHeight < 0 || columnHeight > 1) columnHeight = 1;
            if (string.IsNullOrEmpty(ratioString)) ratioString = "1x1";
            var ratio = new Ratio(ratioString);
            var maxHeight = ImageHelper.CalculateMaxPixels(columnHeight, isMobile);
            ratio.SetHeight(maxHeight);
            return ImageHelper.GetImageUrl(FileInfo, ratio);
        }


        public string EditLink(string urlFileManager, string culture)
        {
            var tabId = DnnUtils.GetDnnTabByUrl(urlFileManager, culture).TabID; //todo sacha
            return EditLink(tabId);
        }

        public string EditLink(int tabFileManager)
        {
            {
                if (tabFileManager <= 0) return "";
                var url = Globals.NavigateURL(tabFileManager);
                var dnnFileManagerModule = DnnUtils.GetDnnModulesByFriendlyName("filemanager", tabFileManager).FirstOrDefault(); //todo sacha
                //var modId = dnnFileManagerModule.ModuleControlId; 1420; //todo sacha
                var modId = 1420; //todo sacha
                return string.Format("javascript:dnnModal.show('{0}/ctl/FileProperties/mid/{2}?popUp=true&fileId={1}')", url, FileInfo.FileId, modId);
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