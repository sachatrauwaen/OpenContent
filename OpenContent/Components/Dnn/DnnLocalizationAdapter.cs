using Satrabel.OpenContent.Components.Localization;

namespace Satrabel.OpenContent.Components.Dnn
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
    }

}