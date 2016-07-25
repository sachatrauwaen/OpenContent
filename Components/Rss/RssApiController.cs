using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using DotNetNuke.Web.Api;
using System.Net.Http.Headers;
using Satrabel.OpenContent.Components.Handlebars;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Lucene.Config;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Alpaca;

namespace Satrabel.OpenContent.Components.Rss
{
    public class RssApiController : DnnApiController
    {
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage GetFeed(int moduleId, int tabId)
        {
            return GetFeed(moduleId, tabId, "rss", "application/rss+xml");
        }
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage GetFeed(int moduleId, int tabId, string template, string mediaType)
        {
            ModuleController mc = new ModuleController();
            IEnumerable<IDataItem> dataList = new List<IDataItem>();
            var module = mc.GetModule(moduleId, tabId, false);
            OpenContentSettings settings = module.OpenContentSettings();
            var manifest = settings.Template.Manifest;
            var templateManifest = settings.Template;

            var rssTemplate = new FileUri(settings.TemplateDir, template + ".hbs");
            string source = File.ReadAllText(rssTemplate.PhysicalFilePath);

            //var ds = DataSourceManager.GetDataSource("OpenContent");
            //var dsContext = new DataSourceContext()
            //{
            //    ModuleId = moduleId,
            //    ActiveModuleId = module.ModuleID,
            //    TemplateFolder = settings.TemplateDir.FolderPath
            //};

            bool useLucene = settings.Template.Manifest.Index;
            if (useLucene)
            {
                var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);

                QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                if (!string.IsNullOrEmpty(settings.Query))
                {
                    var query = JObject.Parse(settings.Query);
                    queryBuilder.Build(query, PortalSettings.UserMode != PortalSettings.Mode.Edit, UserInfo.UserID);
                }
                else
                {
                    queryBuilder.BuildFilter(PortalSettings.UserMode != PortalSettings.Mode.Edit);
                }
                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    ModuleId = module.ModuleID,
                    UserId = UserInfo.UserID,
                    TemplateFolder = settings.TemplateDir.FolderPath,
                    Config = manifest.DataSourceConfig
                };
                var dsItems = ds.GetAll(dsContext, queryBuilder.Select);
                int mainTabId = settings.DetailTabId > 0 ? settings.DetailTabId : settings.TabId;
                //ModelFactory mf = new ModelFactory(dsItems.Items, ActiveModule, PortalSettings, mainTabId);
                //var model = mf.GetModelAsJson(false);

                dataList = dsItems.Items;
                /*
                var queryDef = new QueryDefinition(indexConfig);
                queryDef.BuildFilter(true);
                queryDef.BuildSort("");
                SearchResults docs = LuceneController.Instance.Search(moduleId.ToString(), queryDef);
                if (docs != null)
                {
                    int total = docs.TotalResults;
                    foreach (var item in docs.ids)
                    {
                        var dsItem = ds.Get(dsContext, item);
                        //var content = ctrl.GetContent(int.Parse(item));
                        if (dsItem != null)
                        {
                            dataList.Add(dsItem);
                        }
                    }
                }
                 */
            }

            ModelFactory mf = new ModelFactory(dataList, null, settings.TemplateDir.PhysicalFullDirectory, manifest, null, null, module, PortalSettings, tabId, moduleId);
            dynamic model = mf.GetModelAsDynamic(true);
            HandlebarsEngine hbEngine = new HandlebarsEngine();
            string res = hbEngine.Execute(source, model);
            var response = new HttpResponseMessage();
            response.Content = new StringContent(res);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            return response;

        }
    }
}