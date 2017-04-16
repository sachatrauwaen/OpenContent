using System;
using DotNetNuke.Entities.Portals;

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
                Log.Logger.Error($"Error while trying to create PortalFileUri: portalFileId:{portalFileId}", ex);
            }
            return retval;
        }

        public static PortalFileUri CreatePortalFileUri(string portalFileId)
        {
            int fileId;
            if (int.TryParse(portalFileId, out fileId))
            {
                //the id holds an integer referring to a fileId
                return CreatePortalFileUri(fileId);
            }
            else
            {
                //the id holds a path to a file
                var portalId = DeterminePortalIdFromFilePath(portalFileId);
                return CreatePortalFileUri(portalId, portalFileId);
            }
        }

        private static int DeterminePortalIdFromFilePath(string portalFilePath)
        {
            //todo: first try to parse portal from portalFilePath, as it might hold a reference to another portal

            if (PortalSettings.Current == null) return -1;
            return PortalSettings.Current.PortalId;
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
                Log.Logger.Error($"Error while trying to create PortalFileUri: portalFileId:{portalFileId}", ex);
            }
            return retval;
        }
        public static PortalFileUri CreatePortalFileUri(int portalId, string filePath)
        {
            PortalFileUri retval = null;
            try
            {
                retval = string.IsNullOrEmpty(filePath) || portalId < 0 ? null : new PortalFileUri(portalId, filePath);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error while trying to create PortalFileUri: portalid:{portalId}, filepath:{filePath}", ex);
            }
            return retval;
        }
    }
}