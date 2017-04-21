using System;

namespace Satrabel.OpenContent.Components.Localization
{
    public class Localizer
    {
        private static readonly Lazy<ILocalizationAdapter> Lazy = new Lazy<ILocalizationAdapter>(() => App.Config.LocalizationAdapter);
        public static ILocalizationAdapter Instance => Lazy.Value;

        private Localizer()
        {
        }
    }
}