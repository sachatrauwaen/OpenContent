using System;
using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components.Lucene
{
    [Obsolete("LuceneController is deprecated (since july 2017), please use App.Service.LuceneIndex instead.")]
    public class LuceneController
    {
        [Obsolete("LuceneController.Instance is deprecated (since july 2017), please use App.Service.LuceneIndex instead.")]
        public static ILuceneIndexAdapter Instance => App.Services.LuceneIndex;
    }
}
