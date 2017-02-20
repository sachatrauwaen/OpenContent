using System;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class ImageFactory
    {
        public static ImageUri CreateImage(dynamic imageInfo)
        {
            ImageUri retval = null;
            try
            {
                if (imageInfo?["ImageId"] != null)
                {
                    retval = CreateImage(Convert.ToString(imageInfo["ImageId"])); //it might be an enhanced image object
                }
                else
                {
                    retval = CreateImage(Convert.ToString(imageInfo)); //it might be just the image Id
                }
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorFormat("Error while trying to create ImageUri from dynamic: ", ex);
            }
            return retval;
        }

        public static ImageUri CreateImage(string imageId)
        {
            ImageUri retval = null;
            int imgId;
            if (int.TryParse(imageId, out imgId) && imgId > 0)
                retval = CreateImage(imgId);
            else if (!string.IsNullOrWhiteSpace(imageId))
            {
                try
                {
                    retval = new ImageUri(imageId);
                }
                catch (Exception)
                {
                    Log.Logger.ErrorFormat("Failed to create ImageUri with parameter {0}", imageId);
                }
            }
            return retval;
        }

        public static ImageUri CreateImage(int imageId)
        {
            ImageUri retval = null;
            try
            {
                retval = imageId == 0 ? null : new ImageUri(imageId);
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorFormat("Error while trying to create ImageUri: ", ex);
            }
            return retval;
        }
    }
}