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

        public void ReIndexModuleData(int moduleId, OpenContentSettings settings)
        {
            try
            {
                using (LuceneController lc = LuceneController.Instance)
                {
                    IndexModuleData(lc, moduleId, settings);
                    lc.Store.Commit();
                    lc.Store.OptimizeSearchIndex(true);
                }
            }
            finally
            {
                LuceneController.ClearInstance();
            }
        }

        /// <summary>
        /// Reindex all OpenContent modules of all portals.
        /// </summary>
        internal void IndexAll()
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
                            IndexModule(lc, module);
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

        private void IndexModule(LuceneController lc, ModuleInfo module)
        {
            OpenContentSettings settings = new OpenContentSettings(module.ModuleSettings);
            OpenContentUtils.CheckOpenContentSettings(module, settings);

            if (settings.Template != null && settings.Template.IsListTemplate && !settings.IsOtherModule)
            {
                IndexModuleData(lc, module.ModuleID, settings);
            }
        }

        private void IndexModuleData(LuceneController lc, int moduleId, OpenContentSettings settings)
        {
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

            if (settings.IsOtherModule)
            {
                moduleId = settings.ModuleId;
            }

            lc.Store.Delete(new TermQuery(new Term("$type", moduleId.ToString())));
            OpenContentController occ = new OpenContentController();
            foreach (var item in occ.GetContents(moduleId))
            {
                lc.Add(item, indexConfig);
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

            Store.Add(JsonMappingUtils.JsonToDocument(data.ModuleId.ToString(), data.ContentId.ToString(), data.JsonAsJToken, data.Json, config));
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