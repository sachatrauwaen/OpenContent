
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Manifest;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

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
                var module = new OpenContentModuleInfo(req.ModuleId, req.TabId);
                var manifest2 = module.Settings.Manifest;
                var manifest = module.Settings.Template.Manifest;
                TemplateManifest templateManifest = module.Settings.Template;
                string editRole = manifest.GetEditRole();

                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                OpenContentController ctrl = new OpenContentController();

                if (listMode)
                {
                    if (!OpenContentUtils.HasEditPermissions(PortalSettings, module.ViewModule, editRole, -1))
                    {
                        Log.Logger.WarnFormat("Failed the HasEditPermissions() check for ");
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, "Failed the HasEditPermissions() check");
                    }
                    var index = module.Settings.Template.Manifest.Index;
                    var indexConfig = OpenContentUtils.GetIndexConfig(module.Settings.Template.Key.TemplateDir);
                    OpenContentInfo content = new OpenContentInfo()
                    {
                        ModuleId = module.DataModule.ModuleID,
                        Collection= req.Collection,
                        Title = ActiveModule.ModuleTitle,
                        Json = req.json.ToString(),
                        JsonAsJToken = req.json,
                        CreatedByUserId = UserInfo.UserID,
                        CreatedOnDate = DateTime.Now,
                        LastModifiedByUserId = UserInfo.UserID,
                        LastModifiedOnDate = DateTime.Now
                    };
                    ctrl.AddContent(content, index, indexConfig);
                    return Request.CreateResponse(HttpStatusCode.OK, "");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "It's not a list mode module");
                }
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc);
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