using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetNuke.Entities.Modules;

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
            //Manifest = null;
            Files = null;
        }

        #region Public Properties

        public int DetailItemId { get; set; }
        public string OutputString { get; set; }

        #endregion

        #region Data & Settings

        public void ResetData()
        {
            DataJson = "";
            SettingsJson = "";
        }

        public void SetData(OpenContentInfo data, string dataJson, string settingsData)
        {
            Data = data;
            DataJson = dataJson;
            SettingsJson = settingsData;
            if (!string.IsNullOrWhiteSpace(dataJson)) DataExist = true;            
        }

        public void SetData(IEnumerable<OpenContentInfo> getContents, string settingsData)
        {
            DataList = getContents;
            SettingsJson = settingsData;
            if (getContents != null && getContents.Any()) DataExist = true;
        }

        public string DataJson { get; private set; }
        public string SettingsJson { get; private set; }
        public OpenContentInfo Data { get; private set; }
        public IEnumerable<OpenContentInfo> DataList { get; private set; }
        public bool DataExist { get; set; }
        public bool ShowDemoData { get; set; }
        public bool ShowInitControl { 
            get { 
                return !DataExist || (string.IsNullOrEmpty(SettingsJson) && Template.SettingsNeeded()); 
            } 
        }

        public bool SettingsMissing {
            get{
                return string.IsNullOrEmpty(SettingsJson) && Template.SettingsNeeded(); 
            }
        }

        #endregion

        #region DataSource Module information

        public void SetDataSourceModule(int tabId, int moduleId, ModuleInfo getModule, TemplateManifest template, string data)
        {
            TabId = tabId;
            ModuleId = moduleId;
            Module = getModule;
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