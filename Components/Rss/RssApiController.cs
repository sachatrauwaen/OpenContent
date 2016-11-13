using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using System.Net.Http.Headers;
using Satrabel.OpenContent.Components.Handlebars;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json.Linq;
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
            var module = new OpenContentModuleInfo(moduleId, tabId);
            var manifest = module.Settings.Template.Manifest;
            var templateManifest = module.Settings.Template;

            var rssTemplate = new FileUri(module.Settings.TemplateDir, template + ".hbs");
            string source = File.ReadAllText(rssTemplate.PhysicalFilePath);

            //var ds = DataSourceManager.GetDataSource(AppConfig.FriendlyName());
            //var dsContext = new DataSourceContext()
            //{
            //    ModuleId = moduleId,
            //    ActiveModuleId = module.ViewModule.ModuleID,
            //    TemplateFolder = settings.TemplateDir.FolderPath
            //};

            bool useLucene = module.Settings.Template.Manifest.Index;
            if (useLucene)
            {
                var indexConfig = OpenContentUtils.GetIndexConfig(module.Settings.Template.Key.TemplateDir);

                QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                queryBuilder.Build(module.Settings.Query, PortalSettings.UserMode != PortalSettings.Mode.Edit, UserInfo.UserID, DnnLanguageUtils.GetCurrentCultureCode(), UserInfo.Social.Roles);

                var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                var dsContext = new DataSourceContext()
                {
                    ModuleId = module.DataModule.ModuleID,
                    UserId = UserInfo.UserID,
                    TemplateFolder = module.Settings.TemplateDir.FolderPath,
                    PortalId = module.DataModule.PortalID,
                    CurrentCultureCode = DnnLanguageUtils.GetCurrentCultureCode(),
                    Config = manifest.DataSourceConfig
                };
                var dsItems = ds.GetAll(dsContext, queryBuilder.Select);
                //int mainTabId = module.Settings.DetailTabId > 0 ? module.Settings.DetailTabId : module.Settings.TabId;
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

            ModelFactory mf = new ModelFactory(dataList, null, module.Settings.TemplateDir.PhysicalFullDirectory, manifest, null, null, module, PortalSettings);
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