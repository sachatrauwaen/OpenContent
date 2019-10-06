namespace Satrabel.OpenContent.Components
{
    public static class OpenContentSettingsExtentions
    {
        public static int GetModuleId(this OpenContentSettings settings, int defaultModuleId)
        {
            return settings.IsOtherModule ? settings.ModuleId : defaultModuleId;
        }
    }
}