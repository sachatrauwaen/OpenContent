
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Manifest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
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
                ModuleController mc = new ModuleController();
                var requestModule = mc.GetModule(req.ModuleId, req.TabId, false);
                bool index = false;
                OpenContentSettings settings = requestModule.OpenContentSettings();
                ModuleInfo module = requestModule;
                if (settings.ModuleId > 0)
                {
                    module = mc.GetModule(settings.ModuleId, settings.TabId, false);
                }
                var manifest = settings.Template.Manifest;
                TemplateManifest templateManifest = settings.Template;
                index = settings.Template.Manifest.Index;
                string editRole = manifest == null ? "" : manifest.EditRole;

                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                int createdByUserid = -1;
                OpenContentController ctrl = new OpenContentController();

                if (listMode)
                {
                    if (!OpenContentUtils.HasEditPermissions(PortalSettings, module, editRole, createdByUserid))
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized);
                    }
                    var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
                    OpenContentInfo content = new OpenContentInfo()
                        {
                            ModuleId = module.ModuleID,
                            Title = ActiveModule.ModuleTitle,
                            Json = req.json.ToString(),
                            CreatedByUserId = UserInfo.UserID,
                            CreatedOnDate = DateTime.Now,
                            LastModifiedByUserId = UserInfo.UserID,
                            LastModifiedOnDate = DateTime.Now,
                            Html = "",
                        };
                    ctrl.AddContent(content, index, indexConfig);
                    return Request.CreateResponse(HttpStatusCode.OK, "");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Its not a list mode module");
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
            public int TabId { get; set; }
            public JObject json { get; set; }
        }
    }
}