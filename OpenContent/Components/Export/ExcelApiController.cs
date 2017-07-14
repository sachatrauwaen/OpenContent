using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using Satrabel.OpenContent.Components.Handlebars;
using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Render;
using System.Net;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Files;
using Satrabel.OpenContent.Components.Querying;
using System.Linq;
using Satrabel.OpenContent.Components.Rest;
using Newtonsoft.Json;

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
        public HttpResponseMessage GetExcelByQuery(int moduleId, int tabId, string queryName, string filter = null, string sort = null)
        {
            RestSelect restSelect = new RestSelect()
            {
                PageIndex = 0,
                PageSize = 100000
            };
            if (!string.IsNullOrEmpty(filter))
            {
                restSelect.Query = JsonConvert.DeserializeObject<RestGroup>(filter);
            }
            if (!string.IsNullOrEmpty(sort))
            {
                restSelect.Sort = JsonConvert.DeserializeObject<List<RestSort>>(sort);
            }
            IEnumerable<IDataItem> dataList = new List<IDataItem>();
            var module = OpenContentModuleConfig.Create(moduleId, tabId, PortalSettings);
            var manifest = module.Settings.Template.Manifest;

            if (!module.HasAllUsersViewPermissions())
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            string filename = queryName;
            bool useLucene = module.Settings.Template.Manifest.Index;
            if (useLucene)
            {
                var indexConfig = OpenContentUtils.GetIndexConfig(module.Settings.Template);

                QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                queryBuilder.Build(module.Settings.Query, true, UserInfo.UserID, DnnLanguageUtils.GetCurrentCultureCode(), UserInfo.Social.Roles.FromDnnRoles());
                RestQueryBuilder.MergeQuery(indexConfig, queryBuilder.Select, restSelect, DnnLanguageUtils.GetCurrentCultureCode());
                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);
                
                if (string.IsNullOrEmpty(queryName))
                {
                    var dsItems = ds.GetAll(dsContext, queryBuilder.Select);
                    dataList = dsItems.Items;
                    filename = dsContext.Collection;
                }
                else
                {
                    var qds = ds as IDataQueries;
                    if (qds != null)
                    {

                        var query = qds.GetQueries(dsContext).SingleOrDefault(q => q.Name == queryName);
                        if (query != null)
                        {
                            var dsItems = query.GetAll(dsContext, queryBuilder.Select);
                            dataList = dsItems.Items;
                        }

                    }
                }
            }

            var mf = new ModelFactoryMultiple(dataList, null, manifest, null, null, module);
            dynamic model = mf.GetModelAsDictionary(true);

            

            var rssTemplate = new FileUri(module.Settings.TemplateDir, filename + "-excel.hbs");
            string source = rssTemplate.FileExists ? FileUriUtils.ReadFileFromDisk(rssTemplate) : GenerateTemplateFromModel(model, rssTemplate);

            HandlebarsEngine hbEngine = new HandlebarsEngine();
            string res = hbEngine.Execute(source, model);
            
            var fileBytes = ExcelUtils.CreateExcel(res);
            return ExcelUtils.CreateExcelResponseMessage(filename + ".xlsx", fileBytes);

        }

        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage GetExcel(int moduleId, int tabId, string template, string fileName)
        {
            IEnumerable<IDataItem> dataList = new List<IDataItem>();
            var module = OpenContentModuleConfig.Create(moduleId, tabId, PortalSettings);
            var manifest = module.Settings.Template.Manifest;

            if (!module.HasAllUsersViewPermissions())
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            bool useLucene = module.Settings.Template.Manifest.Index;
            if (useLucene)
            {
                var indexConfig = OpenContentUtils.GetIndexConfig(module.Settings.Template);

                QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                queryBuilder.Build(module.Settings.Query, PortalSettings.UserMode != PortalSettings.Mode.Edit, UserInfo.UserID, DnnLanguageUtils.GetCurrentCultureCode(), UserInfo.Social.Roles.FromDnnRoles());
                IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);

                var dsItems = ds.GetAll(dsContext, queryBuilder.Select);
                dataList = dsItems.Items;
            }

            var mf = new ModelFactoryMultiple(dataList, null, manifest, null, null, module);
            dynamic model = mf.GetModelAsDictionary(true);

            var rssTemplate = new FileUri(module.Settings.TemplateDir, template + ".hbs");
            string source = rssTemplate.FileExists ? FileUriUtils.ReadFileFromDisk(rssTemplate) : GenerateTemplateFromModel(model, rssTemplate);

            HandlebarsEngine hbEngine = new HandlebarsEngine();
            string res = hbEngine.Execute(source, model);

            var fileBytes = ExcelUtils.CreateExcel(res);
            return ExcelUtils.CreateExcelResponseMessage(fileName, fileBytes);
        }

        private static string GenerateTemplateFromModel(IDictionary<string, object> model, FileUri rssTemplate)
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