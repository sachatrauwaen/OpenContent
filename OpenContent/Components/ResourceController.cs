#region Copyright

// 
// Copyright (c) 2015-2017
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using Newtonsoft.Json.Linq;
using System.IO;
using DotNetNuke.Security;
using Satrabel.OpenContent.Components.Json;
using DotNetNuke.Entities.Modules;
using System.Collections.Generic;
using DotNetNuke.Services.Localization;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Dnn;
using DotNetNuke.Entities.Tabs;
using System.Web.Hosting;
using Satrabel.OpenContent.Components.Logging;
using System.Text;
using ClientDependency.Core.CompositeFiles;
using System.Net.Http.Headers;

#endregion

namespace Satrabel.OpenContent.Components
{
    public class ResourceController : DnnApiController
    {
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage Css(int tabid, int portalid)
        {
            List<string> templates = new List<string>();
            StringBuilder css = new StringBuilder();
            var tab = TabController.Instance.GetTab(tabid, portalid);
            foreach (ModuleInfo module in tab.Modules.Cast<ModuleInfo>().Where(m => m.ModuleDefinition.DefinitionName == App.Config.Opencontent && !m.IsDeleted))
            {
                var moduleSettings = module.OpenContentSettings();
                if (moduleSettings.Template != null)
                {
                    var filePath = moduleSettings.Template.MainTemplateUri().FilePath;
                    if (!templates.Contains(filePath))
                    {
                        var cssfilename = new FileUri(Path.ChangeExtension(filePath, "css"));
                        if (cssfilename.FileExists)
                        {
                            if (UserInfo.IsSuperUser)
                            {
                                css.Append("/*").Append((cssfilename.FilePath)).AppendLine("*/");
                            }
                            css.AppendLine(CssMin.CompressCSS(File.ReadAllText(cssfilename.PhysicalFilePath)));
                        }
                        templates.Add(filePath);
                    }
                }
            }
            var res = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(css.ToString(), Encoding.UTF8, "text/css")
            };
            res.Headers.CacheControl = new CacheControlHeaderValue { MaxAge = new TimeSpan(365, 0, 0, 0), Public=true, Private=false };
            return res;
        }
    }
}