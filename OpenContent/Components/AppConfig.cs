using System;

namespace Satrabel.OpenContent.Components
{
    public class AppConfig
    {
        private static readonly Lazy<AppConfig> lazy = new Lazy<AppConfig>(() => new AppConfig());

        public static AppConfig Instance { get { return lazy.Value; } }

        private AppConfig()
        {
        }
        public string LuceneIndexFolder { get { return @"App_Data\OpenContent\lucene_index"; } }


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
    }
}