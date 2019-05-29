using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;


namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public class ImageUri : PortalFileUri
    {
        #region Constructors

        internal ImageUri(int fileId)
            : base(fileId)
        {
            if (!this.FileInfo.IsImageFile())
                throw new ApplicationException($"File with {fileId} is not an image");

        }
        internal ImageUri(string fileFullPath)
            : base(PortalSettings.Current.PortalId, fileFullPath)
        {
            if (!this.FileInfo.IsImageFile())
                throw new ApplicationException($"File with path {fileFullPath} is not an image");

        }
        internal ImageUri(string path, string filename)
            : base(PortalSettings.Current.PortalId, path, filename)
        {
            if (!this.FileInfo.IsImageFile())
                throw new ApplicationException($"File with path {path} and name {filename} is not an image");

        }
        internal ImageUri(int portalid, string fileFullPath)
            : base(portalid, fileFullPath)
        {
            if (!this.FileInfo.IsImageFile())
                throw new ApplicationException($"File of portal {portalid} and path {fileFullPath} is not an image");
        }
        internal ImageUri(int portalid, string path, string filename)
            : base(portalid, path, filename)
        {
            if (!this.FileInfo.IsImageFile())
                throw new ApplicationException($"File of portal {portalid} and path {path} and name {filename} is not an image");
        }
        #endregion

        public int Width
        {
            get
            {
                if (FileInfo == null) return -1;

                return FileInfo.Width;
            }
        }

        public int Height
        {
            get
            {
                if (FileInfo == null) return -1;

                return FileInfo.Height;
            }
        }

        public string RawRatio
        {
            get
            {
                if (FileInfo == null) return "";

                return FileInfo.Width + "x" + FileInfo.Height;
            }
        }

        public bool IsSquare()
        {
            if (Width <= 0 || Height <= 0) return false;

            var ratio = Math.Round((decimal)Height / (decimal)Width, 1);
            return Math.Abs(1 - ratio) <= (decimal)0.1;
        }

        public bool IsPortrait()
        {
            if (Width <= 0 || Height <= 0) return false;
            if (IsSquare()) return false;

            return Height > Width;
        }

        public bool IsLandScape()
        {
            if (Width <= 0 || Height <= 0) return false;
            if (IsSquare()) return false;

            return Height < Width;
        }

        public string GetImageUrl(int width, int height)
        {
            var ratio = new Ratio(width, height);
            return ImageHelper.GetImageUrl(FileInfo, ratio);
        }

        /// <summary>
        /// Gets the image URL.
        /// </summary>
        /// <param name="columnWidth">Size of the image. In Bootstrap 12th</param>
        /// <param name="ratioString">The ratio string.</param>
        /// <param name="isMobile">if set to <c>true</c> [is mobile].</param>
        /// <returns></returns>
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
            return EditUrl(module);
        }

        [Obsolete("This method is obsolete since dec 2015; use public string EditUrl() instead")]
        public string EditImageUrl()
        {
            return EditUrl();
        }

    }
}