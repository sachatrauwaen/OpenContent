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
                retval = Convert.ToInt32(imageId) == 0 ? null : new ImageUri(Convert.ToInt32(imageId));
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorFormat("Error while trying to creaet ImageUri: ", ex);
            }
            return retval;
        }
    }
}