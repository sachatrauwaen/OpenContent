#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Search.Entities;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Vectorhighlight;
using Lucene.Net.Store;
using DotNetNuke.Instrumentation;
using System.Web;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Search;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using Lucene.Net.QueryParsers;
using Satrabel.OpenContent.Components.Lucene.Mapping;
using Satrabel.OpenContent.Components.Lucene.Config;

#endregion

namespace Satrabel.OpenContent.Components.Lucene
{
    public class LuceneController : IDisposable
    {

        #region Constants
        private const string DefaultSearchFolder = @"App_Data\OpenContent\lucene_index";
        private const string WriteLockFile = "write.lock";
        internal const int DefaultRereadTimeSpan = 10; // in seconds (initialy 30sec)
        private const int DISPOSED = 1;
        private const int UNDISPOSED = 0;
        #endregion

        #region Private Properties

        internal string IndexFolder { get; private set; }

        private IndexWriter _writer;
        private IndexReader _idxReader;
        private CachedReader _reader;
        private readonly object _writerLock = new object();
        private readonly double _readerTimeSpan; // in seconds
        private readonly List<CachedReader> _oldReaders = new List<CachedReader>();
        private int _isDisposed = UNDISPOSED;

        private static LuceneController _instance = new LuceneController();
        public static LuceneController Instance
        {
            get
            {
                return _instance;
            }
        }
        public static void ClearInstance()
        {
            _instance.Dispose();
            _instance = null;
            _instance = new LuceneController();
        }

        #region constructor
        private LuceneController()
        {
            //var hostController = HostController.Instance;

            var folder = DefaultSearchFolder; // hostController.GetString(Constants.SearchIndexFolderKey, DefaultSearchFolder);

            if (string.IsNullOrEmpty(folder)) folder = DefaultSearchFolder;
            IndexFolder = Path.Combine(Globals.ApplicationMapPath, folder);
            _readerTimeSpan = DefaultRereadTimeSpan; //  hostController.GetDouble(Constants.SearchReaderRefreshTimeKey, DefaultRereadTimeSpan);
        }

        private void CheckDisposed()
        {
            if (Thread.VolatileRead(ref _isDisposed) == DISPOSED)
                throw new ObjectDisposedException(Localization.GetExceptionMessage("LuceneControlerIsDisposed", "LuceneController is disposed and cannot be used anymore"));
        }
        #endregion

        private IndexWriter Writer
        {
            get
            {
                if (_writer == null)
                {
                    lock (_writerLock)
                    {
                        if (_writer == null)
                        {
                            var lockFile = Path.Combine(IndexFolder, WriteLockFile);
                            if (File.Exists(lockFile))
                            {
                                try
                                {
                                    // if we successd in deleting the file, move on and create a new writer; otherwise,
                                    // the writer is locked by another instance (e.g., another server in a webfarm).
                                    File.Delete(lockFile);
                                }
                                catch (IOException e)
                                {
#pragma warning disable 0618
                                    throw new SearchException(
                                        Localization.GetExceptionMessage("UnableToCreateLuceneWriter", "Unable to create Lucene writer (lock file is in use). Please recycle AppPool in IIS to release lock."),
                                        e, new SearchItemInfo());
#pragma warning restore 0618
                                }
                            }

                            CheckDisposed();
                            var writer = new IndexWriter(FSDirectory.Open(IndexFolder),
                                JsonMappingUtils.GetAnalyser(), IndexWriter.MaxFieldLength.UNLIMITED);
                            _idxReader = writer.GetReader();
                            Thread.MemoryBarrier();
                            _writer = writer;
                        }
                    }
                }
                return _writer;
            }
        }

        // made internal to be used in unit tests only; otherwise could be made private
        internal IndexSearcher GetSearcher()
        {
            if (_reader == null || MustRereadIndex)
            {
                CheckValidIndexFolder();
                UpdateLastAccessTimes();
                InstantiateReader();
            }

            return _reader.GetSearcher();
        }

        private void InstantiateReader()
        {
            IndexSearcher searcher;
            if (_idxReader != null)
            {
                //use the Reopen() method for better near-realtime when the _writer ins't null
                var newReader = _idxReader.Reopen();
                if (_idxReader != newReader)
                {
                    //_idxReader.Dispose(); -- will get disposed upon disposing the searcher
                    Interlocked.Exchange(ref _idxReader, newReader);
                }

                searcher = new IndexSearcher(_idxReader);
            }
            else
            {
                // Note: disposing the IndexSearcher instance obtained from the next
                // statement will not close the underlying reader on dispose.
                searcher = new IndexSearcher(FSDirectory.Open(IndexFolder));
            }

            var reader = new CachedReader(searcher);
            var cutoffTime = DateTime.Now - TimeSpan.FromSeconds(_readerTimeSpan * 10);
            lock (((ICollection)_oldReaders).SyncRoot)
            {
                CheckDisposed();
                _oldReaders.RemoveAll(r => r.LastUsed <= cutoffTime);
                _oldReaders.Add(reader);
                Interlocked.Exchange(ref _reader, reader);
            }
        }

        private DateTime _lastReadTimeUtc;
        private DateTime _lastDirModifyTimeUtc;

        private bool MustRereadIndex
        {
            get
            {
                return (DateTime.UtcNow - _lastReadTimeUtc).TotalSeconds >= _readerTimeSpan &&
                    System.IO.Directory.Exists(IndexFolder) &&
                    System.IO.Directory.GetLastWriteTimeUtc(IndexFolder) != _lastDirModifyTimeUtc;
            }
        }

        private void UpdateLastAccessTimes()
        {
            _lastReadTimeUtc = DateTime.UtcNow;
            if (System.IO.Directory.Exists(IndexFolder))
            {
                _lastDirModifyTimeUtc = System.IO.Directory.GetLastWriteTimeUtc(IndexFolder);
            }
        }

        private void RescheduleAccessTimes()
        {
            // forces re-opening the reader within 30 seconds from now (used mainly by commit)
            var now = DateTime.UtcNow;
            if (_readerTimeSpan > DefaultRereadTimeSpan && (now - _lastReadTimeUtc).TotalSeconds > DefaultRereadTimeSpan)
            {
                _lastReadTimeUtc = now - TimeSpan.FromSeconds(_readerTimeSpan - DefaultRereadTimeSpan);
            }
        }

        private void CheckValidIndexFolder()
        {
            if (!ValidateIndexFolder())
            {
                throw new SearchIndexEmptyException(Localization.GetExceptionMessage("SearchIndexingDirectoryNoValid", "Search indexing directory is either empty or does not exist"));
            }
        }

        private bool ValidateIndexFolder()
        {
            return System.IO.Directory.Exists(IndexFolder) &&
                   System.IO.Directory.GetFiles(IndexFolder, "*.*").Length > 0;
        }

        #endregion

        private SearchResults Search(string type, Query Filter, Query Query, Sort Sort, int PageSize, int PageIndex)
        {
            var luceneResults = new SearchResults();
            //validate whether index folder is exist and contains index files, otherwise return null.
            if (!ValidateIndexFolder())
            {
                return luceneResults;
            }
            var searcher = GetSearcher();
            TopDocs topDocs;
            if (Filter == null)
                topDocs = searcher.Search(type, Query, (PageIndex + 1) * PageSize, Sort);
            else
                topDocs = searcher.Search(type, Filter, Query, (PageIndex + 1) * PageSize, Sort);
            luceneResults.TotalResults = topDocs.TotalHits;
            luceneResults.ids = topDocs.ScoreDocs.Skip(PageIndex * PageSize).Select(d => searcher.Doc(d.Doc).GetField(JsonMappingUtils.FieldId).StringValue).ToArray();
            return luceneResults;
        }
        public SearchResults Search(string type, string DefaultFieldName, string Filter, string Query, string Sorts, int PageSize, int PageIndex, FieldConfig IndexConfig)
        {
            Query query = ParseQuery(Query, DefaultFieldName);
            Query filter = ParseQuery(Filter, DefaultFieldName);

            QueryDefinition def = new QueryDefinition(IndexConfig)
            {
                Query = query,
                Filter = filter,
                PageIndex = PageIndex,
                PageSize = PageSize
            };
            def.BuildSort(Sorts);

            return Search(type, DefaultFieldName, def);
        }
        public SearchResults Search(string type, string DefaultFieldName, QueryDefinition def)
        {
            return Search(type, def.Filter, def.Query, def.Sort, def.PageSize, def.PageIndex);
        }
        /*
        public SearchResults FacetSearch(string type, string DefaultFieldName, QueryDefinition def)
        {
            var luceneResults = new SearchResults();
            //validate whether index folder is exist and contains index files, otherwise return null.
            if (!ValidateIndexFolder())
            {
                return luceneResults;
            }
            var searcher = GetSearcher();
            SimpleFacetedSearch sfs = new SimpleFacetedSearch(searcher.IndexReader, new string[] { "source", "category" });
            SimpleFacetedSearch.Hits hits = sfs.Search(def.Query, def.PageSize);
	        
	        luceneResults.ToalResults  = (int)hits.TotalHitCount;
            foreach (SimpleFacetedSearch.HitsPerFacet hpf in hits.HitsPerFacet)
            {
		        long hitCountPerFacet = hpf.HitCount;
                SimpleFacetedSearch.FacetName name = hpf.Name;

                foreach (Document doc in hpf.Documents)
                {
                     ........
                }
            }
            
            return Search(type, def.Filter, def.Query, def.Sort, def.PageSize, def.PageIndex);
        }
        */
        public static Query ParseQuery(string searchQuery, string DefaultFieldName)
        {
            var parser = new QueryParser(global::Lucene.Net.Util.Version.LUCENE_30, DefaultFieldName, JsonMappingUtils.GetAnalyser());
            Query query;
            try
            {
                if (string.IsNullOrEmpty(searchQuery))
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
        public void Add(Document doc)
        {
            Requires.NotNull("searchDocument", doc);
            if (doc.GetFields().Count > 0)
            {
                try
                {
                    Writer.AddDocument(doc);
                }
                catch (OutOfMemoryException)
                {
                    lock (_writerLock)
                    {
                        // as suggested by Lucene's doc
                        DisposeWriter();
                        Writer.AddDocument(doc);
                    }
                }
            }
        }

        public void Delete(Query query)
        {
            Requires.NotNull("luceneQuery", query);
            Writer.DeleteDocuments(query);
        }

        public void Commit()
        {
            if (_writer != null)
            {
                lock (_writerLock)
                {
                    if (_writer != null)
                    {
                        CheckDisposed();
                        _writer.Commit();
                        RescheduleAccessTimes();
                    }
                }
            }
        }

        public bool OptimizeSearchIndex(bool doWait)
        {
            var writer = _writer;
            if (writer != null && writer.HasDeletions())
            {
                if (doWait)
                {
                    Log.Logger.Debug("Compacting Search Index - started");
                }

                CheckDisposed();
                //optimize down to "> 1 segments" for better performance than down to 1
                _writer.Optimize(4, doWait);

                if (doWait)
                {
                    Commit();
                    Log.Logger.Debug("Compacting Search Index - finished");
                }

                return true;
            }

            return false;
        }

        public bool HasDeletions()
        {
            CheckDisposed();
            var searcher = GetSearcher();
            return searcher.IndexReader.HasDeletions;
        }

        public int MaxDocsCount()
        {
            CheckDisposed();
            var searcher = GetSearcher();
            return searcher.IndexReader.MaxDoc;
        }

        public int SearchbleDocsCount()
        {
            CheckDisposed();
            var searcher = GetSearcher();
            return searcher.IndexReader.NumDocs();
        }

        public void DeleteAll()
        {
            Writer.DeleteAll();
        }


        public void Dispose()
        {
            var status = Interlocked.CompareExchange(ref _isDisposed, DISPOSED, UNDISPOSED);
            if (status == UNDISPOSED)
            {
                DisposeWriter();
                DisposeReaders();
            }
        }

        private void DisposeWriter()
        {
            if (_writer != null)
            {
                lock (_writerLock)
                {
                    if (_writer != null)
                    {
                        _idxReader.Dispose();
                        _idxReader = null;

                        _writer.Commit();
                        _writer.Dispose();
                        _writer = null;
                    }
                }
            }
        }

        private void DisposeReaders()
        {
            lock (((ICollection)_oldReaders).SyncRoot)
            {
                foreach (var rdr in _oldReaders)
                {
                    rdr.Dispose();
                }
                _oldReaders.Clear();
                _reader = null;
            }
        }

        class CachedReader : IDisposable
        {
            public DateTime LastUsed { get; private set; }
            private readonly IndexSearcher _searcher;

            public CachedReader(IndexSearcher searcher)
            {
                _searcher = searcher;
                UpdateLastUsed();
            }

            public IndexSearcher GetSearcher()
            {
                UpdateLastUsed();
                return _searcher;
            }

            private void UpdateLastUsed()
            {
                LastUsed = DateTime.Now;
            }

            public void Dispose()
            {
                _searcher.Dispose();
                _searcher.IndexReader.Dispose();
            }
        }
    }


}