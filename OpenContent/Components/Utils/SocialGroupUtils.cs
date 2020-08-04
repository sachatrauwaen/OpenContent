using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.TemplateHelpers;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Lucene.Config;
using UserRoleInfo = Satrabel.OpenContent.Components.Querying.UserRoleInfo;
using DotNetNuke.UI.Skins;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Web;

namespace Satrabel.OpenContent.Components
{
    public static class SocialGroupUtils
    {
        #region View fitering

        /// <summary>
        /// check for correct social group conditions
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static bool CheckSocialGroupConditions(Manifest.Manifest manifest, NameValueCollection queryString, OpenContentModuleConfig moduleInstance)
        {
            if (manifest.CheckSocialGroupFilter())
            {
                if (queryString?["groupid"] != null)
                {
                    int roleid = -1;
                    Int32.TryParse(queryString?["groupid"], out roleid);
                    var role = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(moduleInstance.PortalId, roleid);
                    if (role != null)
                    {
                        return moduleInstance.IsInRole(role.RoleName);
                    }
                }
                return false;
            }
            // not in social group context
            return true;
        }

        /// <summary>
        /// get the group for extending the details en edit url's
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static string GetSocialGroupParameter(Manifest.Manifest manifest, NameValueCollection queryString)
        {
            string groupquery = null;
            if (manifest.CheckSocialGroupFilter())
            {
                if (queryString?["groupid"] != null)
                {
                    groupquery = "groupid=" + queryString?["groupid"];
                }
            }
            return groupquery;
        }

        /// <summary>
        /// create filter additional for groups, filter on groupid (userroles)
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="filterQuery"></param>
        /// <param name="queryString"></param>
        public static void AddSocialGroupQueryFilter(Manifest.Manifest manifest, JObject filterQuery, NameValueCollection queryString)
        {
            if (manifest.CheckSocialGroupFilter())
            {
                var UserRolesFilter = filterQuery["Filter"] as JObject;
                JArray UserRoles = (JArray)UserRolesFilter["userroles"];
                if (UserRoles == null)
                {
                    UserRoles = new JArray();
                }
                if (queryString?["groupid"] != null)
                {
                    UserRoles.Add(queryString?["groupid"]);
                }
                else
                {
                    // in the context of social groups, we don't want to see any items when there is no groupid, so we mock an non-existing userroles value
                    var dummy = Guid.NewGuid().ToString();
                    UserRoles.Add(dummy);
                }
            }
        }


        #endregion

        #region Edit functions
        /// <summary>
        /// if in social group context add groupid querystring variable to url
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public static string CreateSocialGroupReturnUrl (Manifest.Manifest manifest, string returnUrl)
        {
            string socialGroupQs = SocialGroupUtils.GetSocialGroupParameter(manifest, HttpContext.Current.Request.QueryString);
            if (socialGroupQs != null)
            {
                if (socialGroupQs.Contains("?"))
                {
                    socialGroupQs = "&" + socialGroupQs;
                }
                else
                {
                    socialGroupQs = "?" + socialGroupQs;
                }
                return returnUrl + socialGroupQs;
            }
            return returnUrl;
        }

        public static bool AllowEditAdd(OpenContentModuleConfig config)
        {
            if (config.Settings.Manifest.CheckSocialGroupFilter())
            {
                NameValueCollection queryString = HttpContext.Current.Request.QueryString;
                if (queryString?["groupid"] != null)
                {
                    if (config.Settings.Manifest.CheckSocialGroupFilter())
                    {
                        int roleid = -1;
                        Int32.TryParse(queryString?["groupid"], out roleid);
                        var role = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(config.PortalId, roleid);
                        return config.IsInRole(role.RoleName);
                    }
                }
                return false;
            }
            return true;
        }

        internal static bool HasSocialGroupEditPermissions(int portalId, string groupId, int userId)
        {
            int roleid = -1;
            Int32.TryParse(groupId, out roleid);

            var role = DotNetNuke.Security.Roles.RoleController.Instance.GetUserRole(portalId, userId, roleid);

            if (role != null)
            {
                return true;
            }
            return false;

        }


        /// <summary>
        /// checks wether the user is member and thus can post in the social group
        /// </summary>
        /// <param name="dsContext"></param>
        /// <param name="manifest"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        internal static bool HasSocialGroupCreateItemPermissions(DataSourceContext dsContext, Manifest.Manifest manifest, JObject json)
        {
            if (manifest.CheckSocialGroupFilter())
            {
                JObject formdata = (JObject)json["form"];
                int roleid = -1;
                if (Int32.TryParse(formdata["userroles"].ToString(), out roleid))
                {
                    var role = DotNetNuke.Security.Roles.RoleController.Instance.GetUserRole(dsContext.PortalId, dsContext.UserId, roleid);

                    if (role != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Generate group url when in list mode
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="item"></param>
        /// <param name="editUrl"></param>
        /// <returns></returns>
        internal static string GenerateSocialGroupEditUrl(Manifest.Manifest manifest, IDataItem item, string editUrl)
        {

            if (ManifestUtils.CheckSocialGroupFilter(manifest))
            {
                if (!editUrl.ToLower().Contains("groupid"))
                {
                    string groupId = ""; 
                    if (item.Data["userroles"] !=null)
                    {
                        groupId = item.Data["userroles"].ToString();
                    }
                    if (groupId != null || groupId != "")
                    {
                        if (editUrl.Contains("popUp=true"))
                        {
                            editUrl = editUrl.Replace("popUp=true", "popUp=true&groupId=" + groupId);
                        }
                        else if (editUrl.Contains("?"))
                        {
                            editUrl = editUrl + "&groupid=" + groupId;
                        }
                        else
                        {
                            editUrl = editUrl + "?groupid=" + groupId;
                        }
                    }
                }
            }
            return editUrl;

        }

        internal static string GetSocialGroupNoItemsMessage(Manifest.Manifest manifest)
        {
            var message = ManifestUtils.GetSocialGroupNoItemsMessage(manifest);
            if (message != null)
            {
                return message;
            }
            return "";
        }

        #endregion
    }
}

