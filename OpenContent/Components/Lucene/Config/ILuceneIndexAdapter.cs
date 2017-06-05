using System;
using System.Collections.Generic;
using Lucene.Net.Search;
using Satrabel.OpenContent.Components.Datasource.Search;

namespace Satrabel.OpenContent.Components.Lucene.Config
{
    public interface ILuceneIndexAdapter
    {
        ILuceneIndexAdapter Instance { get; }

        void IndexAll();
        void ReIndexData(IEnumerable<IIndexableItem> context, FieldConfig indexConfig, string scope);
        void Add(IIndexableItem indexableItem, FieldConfig indexConfig);
        void AddList(IEnumerable<IIndexableItem> context, FieldConfig indexConfig, string scope);
        void Update(IIndexableItem indexableItem, FieldConfig indexConfig);
        void Delete(IIndexableItem content);
        void Commit();
        SearchResults Search(string indexScope, Select selectQuery);
        SearchResults Search(string type, Query filter, Query query, Sort sort, int pageSize, int pageIndex);

        [Obsolete("Don't use this. Only made available for backwards compatibility (since july 2017). Please use App.Service.LuceneIndex instead.")]
        LuceneService Store { get; }

    }
}