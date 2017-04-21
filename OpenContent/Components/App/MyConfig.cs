using System;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Indexing;
using Satrabel.OpenContent.Components.Localization;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Lucene;

namespace Satrabel.OpenContent.Components
{
    public class MyConfig : IAppConfig
    {

        #region Constants

        public string FieldNamePublishStartDate
        {
            get
            {
                const string CONSTANT = "publishstartdate";
                return CONSTANT;
            }
        }

        public string FieldNamePublishEndDate
        {
            get
            {
                const string CONSTANT = "publishenddate";
                return CONSTANT;
            }
        }

        public string FieldNamePublishStatus
        {
            get
            {
                const string CONSTANT = "publishstatus";
                return CONSTANT;
            }
        }

        public string Opencontent
        {
            get
            {
                const string CONSTANT = "OpenContent";
                return CONSTANT;
            }
        }

        public string DefaultCollection
        {
            get
            {
                const string CONSTANT = "Items";
                return CONSTANT;
            }
        }


        #endregion

        #region Adapters config

        public string LuceneIndexFolder => @"App_Data\OpenContent\lucene_index";

        public ILocalizationAdapter LocalizationAdapter => new DnnLocalizationAdapter();
        public ILogAdapter LogAdapter => DnnLogAdapter.GetLogAdapter(Opencontent);
        public IIndexAdapter IndexAdapter => new LuceneIndexAdapter();

        #endregion
    }
}