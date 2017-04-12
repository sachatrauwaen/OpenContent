using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Localization
{
    public class Localization
    {
        private static readonly DnnLocalizationAdapter Adapter = new DnnLocalizationAdapter();

        private class DnnLocalizationAdapter
        {
            public string GetString(string value)
            {
                return DotNetNuke.Services.Localization.Localization.GetString("InsufficientFolderPermission");
            }
        }

        public static string GetString(string value)
        {
            var result = Adapter.GetString(value);
            if (string.IsNullOrEmpty(result))
                return value;
            return result;
        }
    }
}