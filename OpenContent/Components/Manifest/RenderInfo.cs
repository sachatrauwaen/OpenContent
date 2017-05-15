using System.Collections.Generic;
using System.Linq;
using Satrabel.OpenContent.Components.Datasource;
using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Manifest
{
    public class RenderInfo
    {
        public RenderInfo(TemplateManifest template, bool isOtherModule)
        {
            SettingsJson = "";
            DataJson = "";
            OutputString = "";
            Template = null;
            Files = null;

            Template = template;
            IsOtherModule = isOtherModule;
        }

        #region Public Properties

        public string DetailItemId { get; set; }
        public string OutputString { get; set; }

        #endregion

        #region Data & Settings

        public void ResetData()
        {
            DataJson = "";
            SettingsJson = "";
        }

        public void SetData(IDataItem data, JToken dataJson, string settingsData)
        {
            Data = data;
            DataJson = dataJson;
            SettingsJson = settingsData;
            DataExist = data != null;
        }

        public void SetData(IEnumerable<IDataItem> getContents, string settingsData)
        {
            DataList = getContents;
            SettingsJson = settingsData;
            if (getContents != null && getContents.Any()) DataExist = true;
        }

        public bool DataExist { get; set; }
        public bool ShowDemoData { get; set; }
        public TemplateManifest Template { get; set; }
        public TemplateFiles Files { get; set; }
        public bool IsOtherModule { get; set; }

        #endregion

        #region ReadOnly

        public JToken DataJson { get; private set; }
        public string SettingsJson { get; private set; }
        public IDataItem Data { get; private set; }
        public IEnumerable<IDataItem> DataList { get; private set; }

        #endregion

        #region ReadOnly Semantic Extentions

        public bool ShowInitControl => Template == null || (!DataExist && Template.DataNeeded()) || (string.IsNullOrEmpty(SettingsJson) && Template.SettingsNeeded());
        public bool IsListMode => Template != null && Template.IsListTemplate;
        public bool IsListPageRequest => IsListMode && string.IsNullOrEmpty(DetailItemId);
        public bool IsDetailPageRequest => IsListMode && !string.IsNullOrEmpty(DetailItemId);

        #endregion

    }
}