using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Entities.Modules;
using Satrabel.OpenContent.Components.Datasource;
using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Manifest
{
    public class RenderInfo
    {
        public RenderInfo()
        {
            SettingsJson = "";
            DataJson = "";
            OutputString = "";
            Template = null;
            Files = null;
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

        public JToken DataJson { get; private set; }
        public string SettingsJson { get; private set; }
        public IDataItem Data { get; private set; }
        public IEnumerable<IDataItem> DataList { get; private set; }
        public bool DataExist { get; set; }
        public bool ShowDemoData { get; set; }
        public bool ShowInitControl
        {
            get
            {
                return Template == null  || (!DataExist && Template.DataNeeded()) || (string.IsNullOrEmpty(SettingsJson) && Template.SettingsNeeded()) ;
            }
        }

        public bool SettingsMissing
        {
            get
            {
                return string.IsNullOrEmpty(SettingsJson) && Template.SettingsNeeded();
            }
        }

        #endregion

        #region DataSource Module information

        public void SetDataSourceModule(int tabId, int moduleId, ModuleInfo viewModule, TemplateManifest template, string data)
        {
            TabId = tabId;
            ModuleId = moduleId;
            Module = viewModule;
            OtherModuleTemplate = template;
            OtherModuleSettingsJson = data;
        }

        public int TabId { get; private set; }
        public int ModuleId { get; private set; }
        public ModuleInfo Module { get; private set; }
        public TemplateManifest OtherModuleTemplate { get; private set; }
        public string OtherModuleSettingsJson { get; private set; }

        #endregion



        public TemplateManifest Template { get; set; }

        #region ReadOnly

        public bool IsOtherModule { get { return TabId > 0 && ModuleId > 0; } }

        #endregion

        public TemplateFiles Files { get; set; }

        //public FileUri Template { get;private set; }
        //public Manifest Manifest { get; private set; }
    }
}