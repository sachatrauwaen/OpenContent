using System;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Indexing;
using Satrabel.OpenContent.Components.Localization;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Lucene;

namespace Satrabel.OpenContent.Components
{
    public class AppConfig
    {
        private static readonly Lazy<AppConfig> Lazy = new Lazy<AppConfig>(() => new AppConfig());

        public static AppConfig Instance => Lazy.Value;

        private AppConfig()
        {
        }

        public string LuceneIndexFolder => @"App_Data\OpenContent\lucene_index";


        #region Constants

        internal static string FieldNamePublishStartDate
        {
            get
            {
                const string CONSTANT = "publishstartdate";
                return CONSTANT;
            }
        }

        internal static string FieldNamePublishEndDate
        {
            get
            {
                const string CONSTANT = "publishenddate";
                return CONSTANT;
            }
        }

        internal static string FieldNamePublishStatus
        {
            get
            {
                const string CONSTANT = "publishstatus";
                return CONSTANT;
            }
        }

        public const string OPENCONTENT = "OpenContent";

        public const string DEFAULT_COLLECTION = "Items";

        #endregion


        #region Adapters config

        public ILocalizationAdapter LocalizationAdapter => new DnnLocalizationAdapter();
        public ILogAdapter LogAdapter => DnnLogAdapter.GetLogAdapter(OPENCONTENT);
        public IIndexAdapter IndexAdapter => new LuceneIndexAdapter();

        #endregion
    }

}