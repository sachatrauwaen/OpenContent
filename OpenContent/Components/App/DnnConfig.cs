﻿
using System;
using DotNetNuke.Common;
using Satrabel.OpenContent.Components.Lucene;

namespace Satrabel.OpenContent.Components
{
    public class DnnConfig : IAppConfig
    {

        #region Constants - explicitly defined to make it easier to see where they are used.

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

        public string ApplicationMapPath { get; } = Globals.ApplicationMapPath;


        public string LuceneIndexFolder
        {
            get
            {
                const string CONSTANT = @"App_Data\OpenContent\lucene_index";
                return CONSTANT;
            }
        }

        public Action LuceneIndexAllDelegate { get; } = LuceneUtils.IndexAll;

        #endregion
}
}