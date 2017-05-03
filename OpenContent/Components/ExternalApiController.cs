using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Manifest;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Dnn;

namespace Satrabel.OpenContent.Components
{
    public class ExternalApiController : DnnApiController
    {
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage Add(UpdateRequest req)
        {
            try
            {
                var module = OpenContentModuleInfo.Create(req.ModuleId, req.TabId, PortalSettings);
                string editRole = module.Settings.Template.Manifest.GetEditRole();

                var dataSource = new OpenContentDataSource();

                if (module.IsListMode())
                {
                    if (!DnnPermissionsUtils.HasEditPermissions(module, editRole, -1))
                    {
                        App.Services.Logger.Warn($"Failed the HasEditPermissions() check");
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, "Failed the HasEditPermissions() check");
                    }
                    var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);
                    dsContext.Collection = req.Collection;

                    JToken data = req.json;
                    data["Title"] = ActiveModule.ModuleTitle;
                    dataSource.Add(dsContext, data);

                    return Request.CreateResponse(HttpStatusCode.OK, "");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "It's not a list mode module");
                }
            }
            catch (Exception exc)
            {
                App.Services.Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
        public class UpdateRequest
        {
            public int ModuleId { get; set; }
            public string Collection { get; set; }
            public int TabId { get; set; }
            public JObject json { get; set; }
        }
    }
}