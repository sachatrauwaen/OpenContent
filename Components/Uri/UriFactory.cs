using System;
using Satrabel.OpenContent.Components.TemplateHelpers;

namespace Satrabel.OpenContent.Components
{
    public static class UriFactory
    {
        public static PortalFileUri CreatePortalFileUri(dynamic portalFileId)
        {
            PortalFileUri retval = null;
            try
            {
                retval = CreatePortalFileUri(Convert.ToInt32(portalFileId));
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorFormat("Error while trying to create PortalFileUri: ", ex);
            }
            return retval;
        }

        public static PortalFileUri CreatePortalFileUri(string portalFileId)
        {
            return CreatePortalFileUri(Convert.ToInt32(portalFileId));
        }

        public static PortalFileUri CreatePortalFileUri(int portalFileId)
        {
            PortalFileUri retval = null;
            try
            {
                retval = portalFileId == 0 ? null : new PortalFileUri(portalFileId);
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorFormat("Error while trying to create PortalFileUri: ", ex);
            }
            return retval;
        }
    }
}