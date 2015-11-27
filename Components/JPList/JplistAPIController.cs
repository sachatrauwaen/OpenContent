using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Lucene;
//using Satrabel.OpenDocument.Components.Lucene;
//using Satrabel.OpenDocument.Components.Template;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
//using Satrabel.OpenContent.Components;
//using Satrabel.OpenContent.Components.TemplateHelpers;
//using TemplateHelper = Satrabel.OpenDocument.Components.Template.TemplateHelper;

namespace Satrabel.OpenContent.Components.JPList
{
    [SupportedModules("OpenContent")]
    public class JplistAPIController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JplistAPIController));

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpPost]
        public HttpResponseMessage List(RequestDTO req)
        {
            try
            {
                OpenContentSettings settings = new OpenContentSettings(ActiveModule.ModuleSettings);
                ModuleInfo module = ActiveModule;
                if (settings.ModuleId > 0)
                {
                    module = ModuleController.Instance.GetModule(settings.ModuleId, settings.TabId, false);
                }
                var manifest = OpenContentUtils.GetManifest(settings.Template);
                TemplateManifest templateManifest = null;
                if (manifest != null)
                {
                    templateManifest = manifest.GetTemplateManifest(settings.Template);
                }
                string editRole = manifest == null ? "" : manifest.EditRole;
                bool listMode = templateManifest != null && templateManifest.IsListTemplate;

                if (listMode)
                {
                    JArray json = new JArray();
                    var jpListQuery = BuildJpListQuery(req.StatusLst);

                    string luceneQuery = BuildLuceneQuery(jpListQuery);
                    SearchResults docs = LuceneController.Instance.Search(module.ModuleID, "Title", luceneQuery, jpListQuery.Pagination.number, jpListQuery.Pagination.currentPage);
                    int total = docs.ToalResults;
                    //docs = docs.Skip(jpListQuery.Pagination.currentPage * jpListQuery.Pagination.number).Take(jpListQuery.Pagination.number);
                    OpenContentController ctrl = new OpenContentController();
                    foreach (var item in docs.ids)
                    {
                        var content = ctrl.GetContent(int.Parse(item), module.ModuleID);
                        if (content != null)
                        {
                            json.Add(content.Json.ToJObject("GetContent " + item));
                        }
                    }
                    var res = new ResultDTO()
                    {
                        data = json,
                        count = total
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "not supported because not in multi items template ");
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #region Private Methods

        private JpListQueryDTO BuildJpListQuery(List<StatusDTO> statuses)
        {
            var query = new JpListQueryDTO();
            foreach (StatusDTO status in statuses)
            {
                switch (status.action)
                {
                    case "paging":
                        {
                            int number = 100000;
                            //  string value (it could be number or "all")
                            int.TryParse(status.data.number, out number);
                            query.Pagination = new PaginationDTO()
                            {
                                number = number,
                                currentPage = status.data.currentPage
                            };
                            break;
                        }

                    case "filter":
                        {
                            if (status.type == "textbox" && status.data != null && !String.IsNullOrEmpty(status.name) && !String.IsNullOrEmpty(status.data.value))
                            {
                                query.Filters.Add(new FilterDTO()
                                {
                                    name = status.name,
                                    value = status.data.value
                                });
                            }

                            else if (status.type == "checkbox-group-filter" && status.data != null && !String.IsNullOrEmpty(status.name))
                            {
                                if (status.data.filterType == "pathGroup" && status.data.pathGroup != null && status.data.pathGroup.Count > 0)
                                {
                                    query.Filters.Add(new FilterDTO()
                                    {
                                        name = status.name,
                                        pathGroup = status.data.pathGroup

                                    });
                                }
                            }
                            else if (status.type == "filter-select" && status.data != null && !String.IsNullOrEmpty(status.name))
                            {
                                if (status.data.filterType == "path" && status.data.path != null)
                                {
                                    query.Filters.Add(new FilterDTO()
                                    {
                                        name = status.name,
                                        value = status.data.path,
                                    });
                                }
                            }
                            break;
                        }

                    case "sort":
                        {
                            query.Sorts.Add(new SortDTO()
                            {
                                path = status.data.path, // field name
                                order = status.data.order
                            });
                            break;
                        }
                }

            }
            return query;
        }

        private string BuildLuceneQuery(JpListQueryDTO jpListQuery)
        {

            string queryStr = "";
            if (jpListQuery.Filters.Any())
            {
                foreach (FilterDTO f in jpListQuery.Filters)
                {
                    if (f.pathGroup != null && f.pathGroup.Any()) //group is bv multicheckbox, vb categories where(categy="" OR category="")
                    {
                        string pathStr = "";
                        foreach (var p in f.pathGroup)
                        {
                            pathStr += (string.IsNullOrEmpty(pathStr) ? "" : " OR ") + f.name + ":" + p;
                        }

                        queryStr += "+" + "(" + pathStr + ")";
                    }
                    else
                    {
                        var names = f.names;
                        string pathStr = "";
                        foreach (var n in names)
                        {
                            if (!string.IsNullOrEmpty(f.path))
                            {
                                pathStr += (string.IsNullOrEmpty(pathStr) ? "" : " OR ") + n + ":" + f.path;  //for dropdownlists; value is keyword => never partial search
                            }
                            else
                            {
                                pathStr += (string.IsNullOrEmpty(pathStr) ? "" : " OR ") + n + ":" + f.value + "*";   //textbox
                            }
                        }
                        queryStr += "+" + "(" + pathStr + ")";
                    }
                }
            }
            return queryStr;
        }

        #endregion
    }
}
