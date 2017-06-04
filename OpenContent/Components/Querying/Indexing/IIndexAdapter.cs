using System.Collections.Generic;
using Satrabel.OpenContent.Components.Querying.Search;

namespace Satrabel.OpenContent.Components.Indexing
{
    public interface IIndexAdapter
    {
        IIndexAdapter Instance { get; }

        void IndexAll();
        void ReIndexData(IEnumerable<IIndexableItem> context, FieldConfig indexConfig, string scope);
        void Add(IIndexableItem indexableItem, FieldConfig indexConfig);
        void AddList(IEnumerable<IIndexableItem> context, FieldConfig indexConfig, string scope);
        void Update(IIndexableItem indexableItem, FieldConfig indexConfig);
        void Delete(IIndexableItem content);
        void Commit();
        SearchResults Search(string indexScope, Select selectQuery);
    }
}