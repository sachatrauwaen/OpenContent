namespace Satrabel.OpenContent.Components.Localization
{

    internal class DnnLocalizationAdapter : ILocalizationAdapter
    {
        public string GetString(string value)
        {
            var result = DotNetNuke.Services.Localization.Localization.GetString(value);
            if (string.IsNullOrEmpty(result))
                return value;
            return result;
        }

        public string GetString(string value, string localResourceFile)
        {
            var result = DotNetNuke.Services.Localization.Localization.GetString(value, localResourceFile);
            if (string.IsNullOrEmpty(result))
                return value;
            return result;
        }
    }

}