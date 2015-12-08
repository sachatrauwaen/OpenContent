using System;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.UI.Modules;


namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public class ImageUri : PortalFileUri
    {
        #region Constructors

        public ImageUri(int fileId) : base(fileId)
        {
        }
        
        private ImageUri(string pathToFile) : base(pathToFile)
        {
            //Don't use this constructor in this class
        }

        private ImageUri(string path, string filename) : base(path, filename)
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
            if (columnWidth < 0) columnWidth = 12;

            if (string.IsNullOrEmpty(ratioString)) ratioString = "1x1";
            var ratio = new Ratio(ratioString);

            int maxWidth;
            if (columnWidth > 12)
                maxWidth = (int)Math.Round(columnWidth);
            else
                maxWidth = ImageHelper.CalculateMaxPixels(columnWidth, isMobile);
            ratio.SetWidth(maxWidth);
            return ImageHelper.GetImageUrl(FileInfo, ratio);
        }

        public string GetImageUrl(string ratioString, float columnHeight, bool isMobile)
        {
            if (columnHeight < 0 || columnHeight > 12) columnHeight = 12;
            if (string.IsNullOrEmpty(ratioString)) ratioString = "1x1";
            var ratio = new Ratio(ratioString);
            var maxHeight = ImageHelper.CalculateMaxPixels(columnHeight, isMobile);
            ratio.SetHeight(maxHeight);
            return ImageHelper.GetImageUrl(FileInfo, ratio);
        }

        /// <summary>
        /// Gets an optimial image for facebook.
        /// Based on the Facebook best practices https://developers.facebook.com/docs/sharing/best-practices#images
        /// Prefereably 1200 x 630 or larger, minimal 600 x 315 and not smaller then 200 x 200
        /// </summary>
        /// <returns></returns>
        public string GetFacebookImageUrl()
        {
            return ImageHelper.GetFacebookImageUrl(FileInfo);
        }


        [Obsolete("This method is obsolete since dec 2015; use public string EditUrl(ModuleInfo module) instead")]
        public string EditImageUrl(ModuleInfo module)
        {
            if (module == null) return string.Empty;
            var mc = new ModuleInstanceContext { Configuration = module };
            if (!mc.IsEditable) return string.Empty;
            return EditImageUrl();
        }

        [Obsolete("This method is obsolete since dec 2015; use public string EditUrl() instead")]
        public string EditImageUrl()
        {
            //var url = Globals.NavigateURL(tabFileManager);
            //var dnnFileManagerModule = DnnUtils.GetDnnModulesByFriendlyName("filemanager", tabFileManager).OrderByDescending(m=> m.ModuleID).FirstOrDefault();
            var dnnFileManagerModule = DnnUtils.GetLastModuleByFriendlyName("Digital Asset Management");
            var modId = dnnFileManagerModule.ModuleID;
            //var modId = 1420; 
            var url = Globals.NavigateURL(dnnFileManagerModule.TabID, "FileProperties", "mid=" + modId, "popUp=true", "fileId=" + FileInfo.FileId);
            return string.Format("javascript:dnnModal.show('{0}',/*showReturn*/false,550,950,true,'')", url);
            //javascript:dnnModal.show('http://localhost:54068/en-us/OpenFiles/ctl/Module/ModuleId/487/view/gridview/pageSize/10?ReturnURL=/en-us/OpenFiles?folderId=42&popUp=true',/*showReturn*/false,550,950,true,'')
            //return string.Format("javascript:dnnModal.show('{0}/ctl/FileProperties/mid/{2}?popUp=true&fileId={1}')", url, FileInfo.FileId, modId);
        }

    }
}