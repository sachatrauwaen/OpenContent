using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lucene.Net.Search;
using Satrabel.OpenContent.Components.Datasource.Search;
using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components.Lucene
{
    [Obsolete("LuceneController is deprecated (since july 2017), please use App.Service.LuceneIndex instead.")]
    public class LuceneController
    {
        [Obsolete("LuceneController.Instance is deprecated (since july 2017), please use App.Service.LuceneIndex instead.")]
        public static readonly ILuceneIndexAdapter Instance = App.Services.LuceneIndex;
    }
}
