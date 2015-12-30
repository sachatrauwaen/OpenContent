using DotNetNuke.Services.Localization;
using Newtonsoft.Json;
namespace Satrabel.OpenContent.Components.Alpaca
{
    public class AlpacaContext
    {
        private int PortalId;

        public AlpacaContext(int PortalId, int ModuleId, int ItemId,
                            string ScopeWrapperID,
                            string CancelButtonID, string SaveButtonID, string DeleteButtonID, string VersionsID)
        {
            this.PortalId = PortalId;
            this.ModuleId = ModuleId;
            this.ItemId = ItemId;
            this.ScopeWrapperID = ScopeWrapperID;
            this.CancelButtonID = CancelButtonID;
            this.SaveButtonID = SaveButtonID;
            this.DeleteButtonID = DeleteButtonID;
            this.VersionsID = VersionsID;
        }
        [JsonProperty(PropertyName = "scopeWrapperID")]
        public string ScopeWrapperID { get; set; }
        [JsonProperty(PropertyName = "cancelButtonID")]
        public string CancelButtonID { get; set; }
        [JsonProperty(PropertyName = "saveButtonID")]
        public string SaveButtonID { get; set; }
        [JsonProperty(PropertyName = "deleteButtonID")]
        public string DeleteButtonID { get; set; }
        [JsonProperty(PropertyName = "versionsID")]
        public string VersionsID { get; set; }

        [JsonProperty(PropertyName = "moduleId")]
        public int ModuleId { get; private set; }
        [JsonProperty(PropertyName = "itemId")]
        public int ItemId { get; set; }
        [JsonProperty(PropertyName = "currentCulture")]
        public string CurrentCulture
        {
            get
            {
                return LocaleController.Instance.GetCurrentLocale(PortalId).Code;
            }
        }
        [JsonProperty(PropertyName = "defaultCulture")]
        public string DefaultCulture
        {
            get
            {
                return LocaleController.Instance.GetDefaultLocale(PortalId).Code;
            }
        }
        [JsonProperty(PropertyName = "numberDecimalSeparator")]
        public string NumberDecimalSeparator
        {
            get
            {
                return LocaleController.Instance.GetCurrentLocale(PortalId).Culture.NumberFormat.NumberDecimalSeparator;
            }
        }
        [JsonProperty(PropertyName = "alpacaCulture")]
        public string AlpacaCulture
        {
            get
            {
                string cultureCode = LocaleController.Instance.GetCurrentLocale(PortalId).Code;
                return AlpacaEngine.AlpacaCulture(cultureCode);
            }
        }
    }
}
