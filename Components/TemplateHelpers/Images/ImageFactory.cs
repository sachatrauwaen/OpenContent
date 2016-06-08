using System;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class ImageFactory
    {
        public static ImageUri CreateImage(dynamic imageId)
        {
            ImageUri retval = null;
            try
            {
                retval = CreateImage(Convert.ToInt32(imageId));
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorFormat("Error while trying to create ImageUri: ", ex);
            }
            return retval;
        }

        public static ImageUri CreateImage(string imageId)
        {
            return CreateImage(Convert.ToInt32(imageId));
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