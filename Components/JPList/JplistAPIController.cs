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
using Satrabel.OpenContent.Components.Manifest;
using Lucene.Net.Search;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;


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
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();
                //stopwatch.Stop();
                //Debug.WriteLine("List:" + stopwatch.ElapsedMilliseconds); 

                OpenContentSettings settings = new OpenContentSettings(ActiveModule.ModuleSettings);
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
                    string luceneFilter = "";
                    string luceneSort = "";
                    if (!string.IsNullOrEmpty(settings.Data))
                    {
                        var set = JObject.Parse(settings.Data);
                        if (set["LuceneFilter"] != null)
                        {
                            luceneFilter = set["LuceneFilter"].ToString();
                        }
                        if (set["LuceneSort"] != null)
                        {
                            luceneSort = set["LuceneSort"].ToString();
                        }
                    }
                    //JArray json = new JArray();
                    var jpListQuery = BuildJpListQuery(req.StatusLst);
                    //string luceneQuery = BuildLuceneQuery(jpListQuery);
                    Query luceneQuery = BuildLuceneQuery2(jpListQuery);
                    if (jpListQuery.Sorts.Any())
                    {
                        var sort = jpListQuery.Sorts.First();
                        luceneSort = sort.path + " " + sort.order;
                    }
                    var indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
                    SearchResults docs = LuceneController.Instance.Search(module.ModuleID.ToString(), "Title", luceneFilter, luceneQuery, luceneSort, jpListQuery.Pagination.number, jpListQuery.Pagination.currentPage, indexConfig);
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
                    ModelFactory mf = new ModelFactory(dataList, settings.Data, settings.Template.Uri().PhysicalFullDirectory, manifest, files, ActiveModule, PortalSettings);
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

        private Query BuildLuceneQuery2(JpListQueryDTO jpListQuery)
        {
            
            if (jpListQuery.Filters.Any())
            {
                BooleanQuery query = new BooleanQuery();
                foreach (FilterDTO f in jpListQuery.Filters)
                {
                    if (f.pathGroup != null && f.pathGroup.Any()) //group is bv multicheckbox, vb categories where(categy="" OR category="")
                    {
                        //string pathStr = "";
                        var groupQuery = new BooleanQuery();
                        foreach (var p in f.pathGroup)
                        {
                            //pathStr += (string.IsNullOrEmpty(pathStr) ? "" : " OR ") + f.name + ":" + p;
                            var termQuery = new TermQuery(new Term(f.name, p));
                            groupQuery.Add(termQuery, Occur.SHOULD); // or
                        }

                        //queryStr += "+" + "(" + pathStr + ")";
                        query.Add(groupQuery, Occur.MUST); //and
                    }
                    else
                    {
                        var names = f.names;
                        //string pathStr = "";
                        var groupQuery = new BooleanQuery();
                        foreach (var n in names)
                        {
                            if (!string.IsNullOrEmpty(f.path))
                            {
                                //pathStr += (string.IsNullOrEmpty(pathStr) ? "" : " OR ") + n + ":" + f.path;  //for dropdownlists; value is keyword => never partial search
                                var termQuery = new TermQuery(new Term(n, f.path));
                                groupQuery.Add(termQuery, Occur.SHOULD); // or
                            }
                            else
                            {
                                //pathStr += (string.IsNullOrEmpty(pathStr) ? "" : " OR ") + n + ":" + f.value + "*";   //textbox
                                var query1 = LuceneController.ParseQuery(n + ":" + f.value + "*", "Title");                                
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
