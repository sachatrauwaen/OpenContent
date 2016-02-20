using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Http;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Services.FileSystem;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class ImageHelper
    {
        /// <summary>
        /// Gets the image URL.
        /// </summary>
        /// <param name="columnWidth">Size of the image. In Bootstrap 12th</param>
        /// <param name="isMobile"></param>
        /// <param name="retina"></param>
        /// <returns></returns>
        public static int CalculateMaxPixels(float columnWidth, bool isMobile, bool retina = true)
        {
            if (columnWidth < 0 || columnWidth > 12) columnWidth = 1;
            if (isMobile && retina)
            {
                return Convert.ToInt32(2 * 768 * columnWidth);
            }
            if (isMobile && !retina)
            {
                return Convert.ToInt32(2 * 480 * columnWidth);
            }
            return Convert.ToInt32(2 * 1200 * columnWidth);
        }



        /// <summary>
        /// Gets an optimial image for facebook.
        /// Based on the Facebook best practices https://developers.facebook.com/docs/sharing/best-practices#images
        /// Prefereably 1200 x 630 or larger, minimal 600 x 315 and not smaller then 200 x 200
        /// </summary>
        /// <returns></returns>
        public static string GetFacebookImageUrl(IFileInfo file)
        {
            var ratio = new Ratio("120x63");
            ratio.SetWidth(1200);
            return GetImageUrl(file, ratio);
        }
        public static string GetImageUrl(string fileid, Ratio requestedCropRatio)
        {
            try
            {
                return GetImageUrl(int.Parse(fileid), requestedCropRatio);
            }
            catch (Exception)
            {

                return null;
            }
        }
        public static string GetImageUrl(int fileid, Ratio requestedCropRatio)
        {
            var file = FileManager.Instance.GetFile(fileid);
            if (file != null)
            {
                return GetImageUrl(file, requestedCropRatio);
            }
            return null;
        }

        public static string GetImageUrl(IFileInfo file, Ratio requestedCropRatio)
        {
            if (file == null)
                throw new NoNullAllowedException("FileInfo should not be null");
            
            if (ModuleDefinitionController.GetModuleDefinitionByFriendlyName("OpenFiles") == null) 
            {
                return file.ToUrl();
            } 
            var url = file.ToUrlWithoutLinkClick();
            if (url.Contains("LinkClick.aspx")) return url;
            url = url.RemoveQueryParams();

            if (file.ContentItemID > 0)
            {
                var contentItem = Util.GetContentController().GetContentItem(file.ContentItemID);
                if (contentItem != null && !string.IsNullOrEmpty(contentItem.Content))
                {
                    JObject content = JObject.Parse(contentItem.Content);
                    var crop = content["crop"];
                    if (crop is JObject && crop["croppers"] != null)
                    {
                        foreach (var cropperobj in crop["croppers"].Children())
                        {
                            var cropper = cropperobj.Children().First();
                            int left = int.Parse(cropper["x"].ToString());
                            int top = int.Parse(cropper["y"].ToString());
                            int w = int.Parse(cropper["width"].ToString());
                            int h = int.Parse(cropper["height"].ToString());
                            var definedCropRatio = new Ratio(w, h);

                            if (Math.Abs(definedCropRatio.AsFloat - requestedCropRatio.AsFloat) < 0.02) //allow 2% margin
                            {
                                //crop first then resize (order defined by the processors definition order in the config file)
                                return url + string.Format("?crop={0},{1},{2},{3}&width={4}&height={5}", left, top, w, h, requestedCropRatio.Width, requestedCropRatio.Height);
                            }
                        }
                    }
                    else
                    {
                        Log.Logger.Info(string.Format("Warning for page {0}. Can't find croppers in {1}. ", HttpContext.Current.Request.RawUrl, contentItem.Content));
                    }
                }
            }

            return url + string.Format("?width={0}&height={1}&mode=crop", requestedCropRatio.Width, requestedCropRatio.Height);
        }


        internal static Image Resize(Image image, int scaledWidth, int scaledHeight)
        {
            return new Bitmap(image, scaledWidth, scaledHeight);
        }

        internal static Image Crop(Image image, int x, int y, int width, int height)
        {
            var croppedBitmap = new Bitmap(width, height);

            using (var g = Graphics.FromImage(croppedBitmap))
            {
                g.DrawImage(image,
                    new Rectangle(0, 0, width, height),
                    new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
            }

            return croppedBitmap;
        }
        /*
        public static Image Center(Image image, int width, int height)
        {

            Bitmap sourceImage; = image.
            int targetWidth = 200;
            int targetHeight = 200;

            int x = image.Width / 2 - width / 2;
            int y = image.Height / 2 - height / 2;

            Rectangle cropArea = new Rectangle(x, y, targetWidth, targetHeight);

            Bitmap targetImage = image.Clone(cropArea, image.PixelFormat);

            return targetImage;
        }
         */
        internal static Image SaveCroppedImage(Image image, int targetWidth, int targetHeight, out int left, out int top, out int srcWidth, out int srcHeight)
        {
            ImageCodecInfo jpgInfo = ImageCodecInfo.GetImageEncoders().Where(codecInfo => codecInfo.MimeType == "image/jpeg").First();
            Image finalImage = image;
            System.Drawing.Bitmap bitmap = null;
            left = 0;
            top = 0;
            srcWidth = 0;
            srcHeight = 0;
            try
            {
                srcWidth = targetWidth;
                srcHeight = targetHeight;
                bitmap = new System.Drawing.Bitmap(targetWidth, targetHeight);
                double croppedHeightToWidth = (double)targetHeight / targetWidth;
                double croppedWidthToHeight = (double)targetWidth / targetHeight;

                if (image.Width > image.Height)
                {
                    srcWidth = (int)(Math.Round(image.Height * croppedWidthToHeight));
                    if (srcWidth < image.Width)
                    {
                        srcHeight = image.Height;
                        left = (image.Width - srcWidth) / 2;
                    }
                    else
                    {
                        srcHeight = (int)Math.Round(image.Height * ((double)image.Width / srcWidth));
                        srcWidth = image.Width;
                        top = (image.Height - srcHeight) / 2;
                    }
                }
                else
                {
                    srcHeight = (int)(Math.Round(image.Width * croppedHeightToWidth));
                    if (srcHeight < image.Height)
                    {
                        srcWidth = image.Width;
                        top = (image.Height - srcHeight) / 2;
                    }
                    else
                    {
                        srcWidth = (int)Math.Round(image.Width * ((double)image.Height / srcHeight));
                        srcHeight = image.Height;
                        left = (image.Width - srcWidth) / 2;
                    }
                }
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(image, new Rectangle(0, 0, bitmap.Width, bitmap.Height), new Rectangle(left, top, srcWidth, srcHeight), GraphicsUnit.Pixel);
                }
                finalImage = bitmap;
            }
            catch { }

            /*
            try
            {
                using (EncoderParameters encParams = new EncoderParameters(1))
                {
                    encParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)100);
                    //quality should be in the range [0..100] .. 100 for max, 0 for min (0 best compression)
                    finalImage.Save(filePath, jpgInfo, encParams);
                    return true;
                }
            }
            catch { }
             */
            if (bitmap != null)
            {
                //bitmap.Dispose();
            }
            //return false;
            return finalImage;
        }


    }
}