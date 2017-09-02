using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;

namespace Satrabel.OpenContent.Components.Localization
{
    public class LocalizationUtils
    {
        public static JObject LoadLocalizationJson(FolderUri templateDir, string culture)
        {
            var localizationJson = LoadLocalizationJsonV2(templateDir, culture);
            if (localizationJson == null)
            {
                // try loading localization files without prefix (for backwards compatibility)
                localizationJson = LoadLocalizationJsonV1(templateDir, culture);
            }
            return localizationJson;
        }

        public static JObject LoadLocalizationJsonV1(FolderUri templateDir, string culture)
        {
            // try loading localization files without prefix (for backwards compatibility)
            var localizationFilename = new FileUri(templateDir, $"{culture}.json");
            var localizationJson = JsonUtils.LoadJsonFromCacheOrDisk(localizationFilename) as JObject;
            return localizationJson;
        }

        public static JObject LoadLocalizationJsonV2(FolderUri templateDir, string culture)
        {
            var localizationFilename = new FileUri(templateDir, $"localization.{culture}.json");
            JObject localizationJson = JsonUtils.LoadJsonFromCacheOrDisk(localizationFilename) as JObject;
            return localizationJson;
        }
    }
}