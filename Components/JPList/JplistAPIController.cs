using DotNetNuke.Entities.Modules;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Lucene;
using System;
using System.Collections.Generic;

using System.Diagnostics;
using System.IO;

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using Satrabel.OpenContent.Components.Manifest;
using Lucene.Net.Search;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Satrabel.OpenContent.Components.Lucene.Config;
using Lucene.Net.Documents;


namespace Satrabel.OpenContent.Components.JPList
{
    [SupportedModules("OpenContent")]
    public class JplistAPIController : DnnApiController
    {
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [HttpPost]
        public HttpResponseMessage List(RequestDTO req)
        {
            try
            {
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();
                //stopwatch.Stop();
                //Debug.WriteLine("List:" + stopwatch.ElapsedMilliseconds); 

                OpenContentSettings settings = ActiveModule.OpenContentSettings();
                ModuleInfo module = ActiveModule;
                if (settings.ModuleId > 0)
                {
                    ModuleController mc = new ModuleController();
                    module = mc.GetModule(settings.ModuleId, settings.TabId, false);
                }
                var manifest = settings.Template.Manifest;
                var templateManifest = settings.Template;

                TemplateFiles files = null;
                if (templateManifest != null)
                {
                    files = templateManifest.Main;
                    // detail not traited !!!
                }

                string editRole = manifest == null ? "" : manifest.EditRole;
                bool listMode = templateManifest != null && templateManifest.IsListTemplate;
                if (listMode)
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
                    QueryDefinition def = new QueryDefinition(indexConfig);
                    //BooleanQuery luceneFilter = null;
                    
                    if (!string.IsNullOrEmpty(settings.Query))
                    {
                        var query = JObject.Parse(settings.Query);
                        def.Build(query, PortalSettings.UserMode != PortalSettings.Mode.Edit);
                    }
                    else
                    {
                        def.BuildFilter(PortalSettings.UserMode != PortalSettings.Mode.Edit);
                    }
                    
                    var jpListQuery = BuildJpListQuery(req.StatusLst);
                    def.Query = BuildLuceneQuery2(jpListQuery, indexConfig);
                    if (jpListQuery.Sorts.Any())
                    {
                        var sort = jpListQuery.Sorts.First();
                        string luceneSort = sort.path + " " + sort.order;
                        def.BuildSort(luceneSort);
                    }
                    if (jpListQuery.Pagination.number > 0)
                        def.PageSize = jpListQuery.Pagination.number;
                    def.PageIndex = jpListQuery.Pagination.currentPage;

                    SearchResults docs = LuceneController.Instance.Search(module.ModuleID.ToString(), "Title", def);
                    int total = docs.ToalResults;
                    OpenContentController ctrl = new OpenContentController();
                    var dataList = new List<OpenContentInfo>();
                    foreach (var item in docs.ids)
                    {
                        var content = ctrl.GetContent(int.Parse(item));
                        if (content != null)
                        {
                            dataList.Add(content);
                        }
                    }
                    ModelFactory mf = new ModelFactory(dataList, settings.Data, settings.Template.Uri().PhysicalFullDirectory, manifest, templateManifest, files, ActiveModule, PortalSettings, settings.TabId, settings.ModuleId);
                    var model = mf.GetModelAsJson(true);
                    var res = new ResultDTO()
                    {
                        data = model,
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
                Log.Logger.Error(exc);
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

        private DateTime? ParseIsoDateTime(string date)
        {
            // "2010-08-20T15:00:00Z"
            DateTime dt;
            if (DateTime.TryParse(date, null, System.Globalization.DateTimeStyles.RoundtripKind, out dt))
                return dt;
            else
                return null;
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

        private Query BuildLuceneQuery2(JpListQueryDTO jpListQuery, FieldConfig indexConfig)
        {
            if (jpListQuery.Filters.Any())
            {
                BooleanQuery query = new BooleanQuery();
                foreach (FilterDTO f in jpListQuery.Filters)
                {
                    if (f.pathGroup != null && f.pathGroup.Any()) //group is bv multicheckbox, vb categories where(categy="" OR category="")
                    {
                        var groupQuery = new BooleanQuery();
                        foreach (var p in f.pathGroup)
                        {
                            var termQuery = new TermQuery(new Term(f.name, p));
                            groupQuery.Add(termQuery, Occur.SHOULD); // or
                        }
                        query.Add(groupQuery, Occur.MUST); //and
                    }
                    else
                    {
                        var names = f.names;
                        var groupQuery = new BooleanQuery();
                        foreach (var n in names)
                        {

                            if (!string.IsNullOrEmpty(f.path))
                            {
                                //for dropdownlists; value is keyword => never partial search
                                var termQuery = new TermQuery(new Term(n, f.path));
                                groupQuery.Add(termQuery, Occur.SHOULD); // or
                            }
                            else
                            {
                                //textbox
                                Query query1;
                                var field = indexConfig.Fields[n];
                                bool ml = field != null && field.MultiLanguage;
                                
                                if (field != null &&
                                    (field.IndexType == "key" || (field.Items != null && field.Items.IndexType == "key")))
                                {                                    
                                    query1 = new WildcardQuery(new Term(n, f.value));
                                }
                                else
                                {
                                    string fieldName = ml ? n + "." + DnnUtils.GetCurrentCultureCode() : n;
                                    query1 = LuceneController.ParseQuery(fieldName + ":" + f.value + "*", "Title");    
                                }
                                
                                groupQuery.Add(query1, Occur.SHOULD); // or
                            }
                        }
                        query.Add(groupQuery, Occur.MUST); //and
                    }
                }
                return query;
            }
            else
            {
                Query query = new MatchAllDocsQuery();
                return query;
            }
        }
        #endregion
    }
}
