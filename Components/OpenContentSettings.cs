using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Satrabel.OpenContent.Components
{
    public class OpenContentSettings
    {
        private JObject _json = null;
        public OpenContentSettings(Hashtable moduleSettings)
        {
            var template = moduleSettings["template"] as string;    //path+file  of //manifestpath+key
            if (!string.IsNullOrEmpty(template))
            {
                Template = new FileUri(template);
            }
            var sTabId = moduleSettings["tabid"] as string;
            var sModuleId = moduleSettings["moduleid"] as string;
            TabId = -1;
            ModuleId = -1;

            if (sTabId != null && sModuleId != null)
            {
                TabId = int.Parse(sTabId);
                ModuleId = int.Parse(sModuleId);
            }

            Data = moduleSettings["data"] as string;
        }

        public int TabId { get; set; }
        public int ModuleId { get; set; }

        public FolderUri TemplateDir { get { return Template; } }
        public string TemplateKey { get { return Template == null ? "" : Template.FileNameWithoutExtension; } }

        internal FileUri Template { get; private set; }
        public string Data { get; private set; }
        public bool IsOtherModule
        {
            get
            {
                return TabId > 0 && ModuleId > 0;
            }
        }

        public bool TemplateAvailable { get { return Template != null; } }

        public JObject DataAsJson
        {
            get
            {
                if (_json == null)
                {
                    _json = JObject.Parse(Data);

                }
                return _json;
            }
        }

        public string LuceneFilter
        {
            get
            {
                var filter = DataAsJson["LuceneFilter"];
                if (filter != null)
                    return filter.ToString();
                else
                    return "";
            }
        }
        public string LuceneSort
        {
            get
            {
                var filter = DataAsJson["LuceneSort"];
                if (filter != null)
                    return filter.ToString();
                else
                    return "";
            }
        }

        public int? LuceneMaxResults
        {
            get
            {
                int maxResults = 0;
                var sMaxResults = DataAsJson["LuceneMaxResults"];

                if (sMaxResults != null && int.TryParse(sMaxResults.ToString(), out maxResults))
                    return maxResults;
                else
                    return null;
            }
        }
    }
}