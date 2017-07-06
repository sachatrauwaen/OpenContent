using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using System.Net.Http.Headers;
using Satrabel.OpenContent.Components.Handlebars;
using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Render;
using System.Net;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Querying;

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
            IEnumerable<IDataItem> dataList = new List<IDataItem>();
            var module = OpenContentModuleConfig.Create(moduleId, tabId, PortalSettings);
            var manifest = module.Settings.Template.Manifest;

            if (!module.HasAllUsersViewPermissions())
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var rssTemplate = new FileUri(module.Settings.TemplateDir, template + ".hbs");
            string source = File.ReadAllText(rssTemplate.PhysicalFilePath);

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
            HandlebarsEngine hbEngine = new HandlebarsEngine();
            string res = hbEngine.Execute(source, model);

            var fileBytes = ExcelUtils.OutputFile(res);
                        
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            
            //Create a file on the fly and get file data as a byte array and send back to client
            response.Content = new ByteArrayContent(fileBytes);//Use your byte array
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = fileName;//your file Name- text.xlsx
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            //response.Content.Headers.ContentType  = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentLength = fileBytes.Length;
            response.StatusCode = System.Net.HttpStatusCode.OK;
            return response;
            
        }
    }
}