using System;

namespace Satrabel.OpenContent.Components.Indexing
{
    public class Indexer
    {
        private static readonly Lazy<IIndexAdapter> Lazy = new Lazy<IIndexAdapter>(() => AppConfig.Instance.IndexAdapter);
        public static IIndexAdapter Instance => Lazy.Value;

        private Indexer()
        {
        }
    }
}