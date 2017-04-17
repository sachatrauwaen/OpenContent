using System.Collections.Generic;
using Satrabel.OpenContent.Components.Indexing;
using Lucene.Net.Search; //todo: remove Lucene dependancy


namespace Satrabel.OpenContent.Components
{
    public interface IIndexAdapter
    {
        void IndexAll();
        void ReIndexModuleData(IEnumerable<IIndexableItem> context, FieldConfig indexConfig, string scope);
        void Add(IIndexableItem indexableItem, FieldConfig indexConfig);
        void Update(IIndexableItem indexableItem, FieldConfig indexConfig);
        void Delete(IIndexableItem content);
        void Commit();

        SearchResults Search(string luceneScope, Query defFilter, Query defQuery, Sort defSort, int defPageSize, int defPageIndex);
        Query ParseQuery(string p0, string fieldName);
    }
}