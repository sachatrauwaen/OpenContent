using System.Collections.Generic;
using Satrabel.OpenContent.Components.Datasource.Search;

namespace Satrabel.OpenContent.Components.Indexing
{
    public interface IIndexAdapter
    {
        void IndexAll();
        void ReIndexModuleData(IEnumerable<IIndexableItem> context, FieldConfig indexConfig, string scope);
        void Add(IIndexableItem indexableItem, FieldConfig indexConfig);
        void Update(IIndexableItem indexableItem, FieldConfig indexConfig);
        void Delete(IIndexableItem content);
        void Commit();
        SearchResults Search(string indexScope, Select selectQuery);
    }
}