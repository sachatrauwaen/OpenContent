#region Usings

using System;
using System.Collections;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Satrabel.OpenContent.Components.Lucene.Mapping;
using Satrabel.OpenContent.Components.Lucene.Config;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Modules;
using Satrabel.OpenContent.Components.Dnn;
using Version = Lucene.Net.Util.Version;

#endregion

namespace Satrabel.OpenContent.Components.Lucene
{
    public class LuceneController : IDisposable
    {
        private static LuceneController _instance = new LuceneController();
        private LuceneService _serviceInstance;

        public static LuceneController Instance
        {
            get
            {
                return _instance;
            }
        }

        public LuceneService Store
        {
            get
            {
                if (_serviceInstance == null)
                    throw new Exception("LuceneController not initialized properly");
                return _serviceInstance;
            }
        }
        #region constructor
        private LuceneController()
        {
            _serviceInstance = new LuceneService(@"App_Data\OpenContent\lucene_index", JsonMappingUtils.GetAnalyser());
        }

        public static void ClearInstance()
        {
            if (_instance != null)
            {
                _instance.Dispose();
                _instance = null;
            }
            _instance = new LuceneController();
        }
        #endregion

        #region Search

        public SearchResults Search(string type, QueryDefinition def)
        {
            return Search(type, def.Filter, def.Query, def.Sort, def.PageSize, def.PageIndex);
        }

        public SearchResults Search(string type, Query filter, Query query, Sort sort, int pageSize, int pageIndex)
        {
            var luceneResults = new SearchResults();

            //validate whether index folder is exist and contains index files, otherwise return null.
            if (!Store.ValidateIndexFolder())
            {
                IndexAll();
                return luceneResults;
            }

            var searcher = Store.GetSearcher();
            TopDocs topDocs;
            if (filter == null)
                topDocs = searcher.Search(type, query, (pageIndex + 1) * pageSize, sort);
            else
                topDocs = searcher.Search(type, filter, query, (pageIndex + 1) * pageSize, sort);
            luceneResults.TotalResults = topDocs.TotalHits;
            luceneResults.ids = topDocs.ScoreDocs.Skip(pageIndex * pageSize).Select(d => searcher.Doc(d.Doc).GetField(JsonMappingUtils.FieldId).StringValue).ToArray();
            return luceneResults;
        }

        #endregion

        #region Index

        /// <summary>
        /// Reindex all portal files.
        /// </summary>
        private void IndexAll()
        {
            LuceneController.ClearInstance();
            try
            {
                using (var lc = LuceneController.Instance)
                {
                    ModuleController mc = new ModuleController();
                    foreach (PortalInfo portal in PortalController.Instance.GetPortals())
                    {
                        ArrayList modules = mc.GetModulesByDefinition(portal.PortalID, "OpenContent");
                        foreach (ModuleInfo module in modules.OfType<ModuleInfo>())
                        {
                            OpenContentSettings settings = new OpenContentSettings(module.ModuleSettings);
                            CheckOpenContentSettings(module, settings);

                            if (settings.Template != null && settings.Template.IsListTemplate && !settings.IsOtherModule)
                            {
                                //TemplateManifest template = null;

                                bool index = false;
                                if (settings.TemplateAvailable)
                                {
                                    index = settings.Manifest.Index;
                                }
                                FieldConfig indexConfig = null;
                                if (index)
                                {
                                    indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
                                }

                                //lc.DeleteAll();
                                //lc.Delete(new TermQuery(new Term("$type", ModuleId.ToString())));
                                OpenContentController occ = new OpenContentController();
                                foreach (var item in occ.GetContents(module.ModuleID))
                                {
                                    lc.Add(item, indexConfig);
                                }
                            }
                        }
                    }
                    lc.Store.Commit();
                    lc.Store.OptimizeSearchIndex(true);
                }
            }
            finally
            {
                LuceneController.ClearInstance();
            }
        }

        private void CheckOpenContentSettings(ModuleInfo module, OpenContentSettings settings)
        {
            if (!settings.TemplateKey.TemplateDir.FolderExists)
            {
                var template = module.ModuleSettings["template"] as string;    //templatepath+file  or  //manifestpath+key

                var url = DnnUrlUtils.NavigateUrl(module.TabID);
                Log.Logger.ErrorFormat("Error loading OpenContent Template on page [{1}]. Reason: Template not found [{0}]", settings.TemplateDir.PhysicalFullDirectory, url);
            }
            //if (!TemplateKey.TemplateDir.FolderExists)
            //{
            //    var url = HttpContext.Current == null ? "" : HttpContext.Current.Request.Url.AbsoluteUri;
            //    Log.Logger.ErrorFormat("Error loading OpenContent Template on page [{1}]. Reason: Template not found [{0}]", templateUri.PhysicalFilePath, url);
            //}
        }

        public void ReIndexModuleData(int moduleId, FieldConfig indexConfig)
        {
            try
            {
                using (LuceneController lc = LuceneController.Instance)
                {
                    lc.Store.Delete(new TermQuery(new Term("$type", moduleId.ToString())));
                    OpenContentController occ = new OpenContentController();
                    foreach (var item in occ.GetContents(moduleId))
                    {
                        lc.Add(item, indexConfig);
                    }
                    lc.Store.Commit();
                    lc.Store.OptimizeSearchIndex(true);
                }
            }
            finally
            {
                LuceneController.ClearInstance();
            }
        }

        #endregion

        #region Operations

        public void Add(OpenContentInfo data, FieldConfig config)
        {
            if (null == data)
            {
                throw new ArgumentNullException("data");
            }

            Store.Add(JsonMappingUtils.JsonToDocument(data.ModuleId.ToString(), data.ContentId.ToString(), data.Json, config));
        }

        public void Update(OpenContentInfo data, FieldConfig config)
        {
            if (null == data)
            {
                throw new ArgumentNullException("data");
            }
            Delete(data);
            Add(data, config);
        }

        /// <summary>
        /// Deletes the matching objects in the IndexWriter.
        /// </summary>
        /// <param name="data"></param>
        public void Delete(OpenContentInfo data)
        {
            if (null == data)
            {
                throw new ArgumentNullException("data");
            }

            var selection = new TermQuery(new Term(JsonMappingUtils.FieldId, data.ContentId.ToString()));

            Query deleteQuery = new FilteredQuery(selection, JsonMappingUtils.GetTypeFilter(data.ModuleId.ToString()));
            Store.Delete(deleteQuery);
        }

        #endregion

        #region Private

        internal static Query ParseQuery(string searchQuery, string defaultFieldName)
        {
            var parser = new QueryParser(Version.LUCENE_30, defaultFieldName, JsonMappingUtils.GetAnalyser());
            Query query;
            try
            {
                if (String.IsNullOrEmpty(searchQuery))
                {
                    query = new MatchAllDocsQuery();
                }
                else
                {
                    query = parser.Parse(searchQuery.Trim());
                }
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        #endregion

        public void Dispose()
        {
            if (_serviceInstance != null)
            {
                _serviceInstance.Dispose();
                _serviceInstance = null;
            }
        }
    }
}