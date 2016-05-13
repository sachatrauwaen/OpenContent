using System;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class ImageFactory
    {
        public static ImageUri CreateImage(dynamic imageId)
        {
            return CreateImage(Convert.ToInt32(imageId));
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
                retval = imageId == 0 ? null : new ImageUri(Convert.ToInt32(imageId));
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorFormat("Error while trying to creaet ImageUri: ", ex);
            }
            return retval;
        }
    }
}