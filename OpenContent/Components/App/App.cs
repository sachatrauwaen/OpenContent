using System;

namespace Satrabel.OpenContent.Components
{
    public class App
    {
        public static IAppConfig Config = new DnnConfig();
        public static IAppServices Services = new DnnServices();
    }
}