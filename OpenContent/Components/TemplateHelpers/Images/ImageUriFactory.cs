using System;
using System.Web;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class ImageUriFactory
    {
        public static ImageUri CreateImageUri(string path, string filename)
        {
            return new ImageUri(path, filename);
        }

        public static ImageUri CreateImageUri(dynamic imageInfo)
        {
            if (imageInfo == null) return null;
            ImageUri retval = null;
            try
            {
                if (imageInfo?["ImageId"] != null)
                {
                    retval = CreateImageUri(Convert.ToString(imageInfo["ImageId"])); // it might be an enhanced image object
                }
                else if (imageInfo?["url"] != null)
                {
                    retval = CreateImageUri(Convert.ToString(imageInfo["url"])); // it might be an imageX object
                }
                else if (imageInfo?["id"] != null)
                {
                    retval = CreateImageUri(Convert.ToString(imageInfo["id"])); // it might be an imageX object
                }
                else
                {
                    retval = CreateImageUri(Convert.ToString(imageInfo)); // it might be just the image Id
                }
            }
            catch (Exception ex)
            {
                App.Services.Logger.Error($"Error while trying to create ImageUri from dynamic string [{Convert.ToString(imageInfo)}].  Error: {ex}");
            }
            return retval;
        }

        public static ImageUri CreateImageUri(string imageId)
        {
            ImageUri retval = null;
            int imgId;
            if (int.TryParse(imageId, out imgId) && imgId > 0)
                retval = CreateImageUri(imgId);
            else if (!string.IsNullOrWhiteSpace(imageId))
            {
                try
                {
                    retval = new ImageUri(imageId);
                }
                catch (Exception)
                {
                    App.Services.Logger.Error($"Failed to create ImageUri with parameter {imageId}. See " + GetDebugInfo(HttpContext.Current));
                }
            }
            return retval;
        }

        private static string GetDebugInfo(HttpContext current)
        {
            if (current == null) return "No httpcontext; thus no extra info";
            return HttpContext.Current?.Request?.Url?.AbsoluteUri;
        }

        public static ImageUri CreateImageUri(int imageId)
        {
            ImageUri retval = null;
            try
            {
                retval = imageId == 0 ? null : new ImageUri(imageId);
            }
            catch (Exception ex)
            {
                App.Services.Logger.Error($"Error while trying to create ImageUri with id {imageId}: ", ex);
            }
            return retval;
        }
    }
}