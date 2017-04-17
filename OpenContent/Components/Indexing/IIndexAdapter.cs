using System.Collections.Generic;
using Lucene.Net.Search;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Lucene.Config;
using Satrabel.OpenContent.Components.Lucene.Index;

namespace Satrabel.OpenContent.Components
{
    public interface IIndexAdapter
    {
        void IndexAll();
        void ReIndexModuleData(IEnumerable<IIndexableItem> context, FieldConfig indexConfig, string scope);
        SearchResults Search(string luceneScope, Query defFilter, Query defQuery, Sort defSort, int defPageSize, int defPageIndex);
        void Add(IIndexableItem indexableItem, FieldConfig indexConfig);
        void Update(IIndexableItem indexableItem, FieldConfig indexConfig);
        void Delete(IIndexableItem content);
        void Commit();
        Query ParseQuery(string p0, string fieldName);
    }
}