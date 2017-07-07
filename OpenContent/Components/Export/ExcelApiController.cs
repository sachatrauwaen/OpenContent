using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using Satrabel.OpenContent.Components.Handlebars;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Render;
using System.Net;
using Satrabel.OpenContent.Components.Files;

namespace Satrabel.OpenContent.Components.Export
{
    public class ExcelApiController : DnnApiController
    {
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage GetExcel(int moduleId, int tabId)
        {
            return GetExcel(moduleId, tabId, "excel", "export.xlsx");
        }

        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage GetExcel(int moduleId, int tabId, string template, string fileName)
        {
            ModuleController mc = new ModuleController();
            IEnumerable<IDataItem> dataList = new List<IDataItem>();
            var module = new OpenContentModuleInfo(moduleId, tabId);
            var manifest = module.Settings.Template.Manifest;

            if (!OpenContentUtils.HasAllUsersViewPermissions(PortalSettings, module.ViewModule))
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            bool useLucene = module.Settings.Template.Manifest.Index;
            if (useLucene)
            {
                var indexConfig = OpenContentUtils.GetIndexConfig(module.Settings.Template);

                QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                queryBuilder.Build(module.Settings.Query, PortalSettings.UserMode != PortalSettings.Mode.Edit, UserInfo.UserID, DnnLanguageUtils.GetCurrentCultureCode(), UserInfo.Social.Roles);

                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                var dsItems = ds.GetAll(dsContext, queryBuilder.Select);
                dataList = dsItems.Items;
            }

            var mf = new ModelFactoryMultiple(dataList, null, module.Settings.TemplateDir.PhysicalFullDirectory, manifest, null, null, module, PortalSettings);
            dynamic model = mf.GetModelAsDictionary(true);

            var rssTemplate = new FileUri(module.Settings.TemplateDir, template + ".hbs");
            string source = rssTemplate.FileExists ? FileUriUtils.ReadFileFromDisk(rssTemplate) : GenerateCsvTemplateFromModel(model, rssTemplate);

            HandlebarsEngine hbEngine = new HandlebarsEngine();
            string res = hbEngine.Execute(source, model);

            var fileBytes = ExcelUtils.OutputFile(res);
            return ExcelUtils.CreateExcelResponseMessage(fileName, fileBytes);
        }

        private static string GenerateCsvTemplateFromModel(IDictionary<string, object> model, FileUri rssTemplate)
        {
            string retval = string.Empty;
            var fieldlist = new List<string>();
            dynamic items = model["Items"];
            foreach (var item in items[0])
            {
                if (item.Key != "Context")
                    fieldlist.Add(item.Key);
            }
            foreach (var field in fieldlist)
            {
                retval = retval + $"\"{field}\";";
            }
            retval = retval + "{{#each Items}}" + Environment.NewLine;
            foreach (var field in fieldlist)
            {
                retval = retval + $"\"#[[#{field}#]]#\";";
            }
            retval = retval.Replace("#[[#", "{{{").Replace("#]]#", "}}}") + "{{/ each}}" + Environment.NewLine;

            FileUriUtils.WriteFileToDisk(rssTemplate, retval);

            return retval;
        }
    }
}