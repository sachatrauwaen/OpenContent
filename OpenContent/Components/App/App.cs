using System;

namespace Satrabel.OpenContent.Components
{
    public static class App
    {
        public static readonly IAppConfig Config = new DnnConfig();
        public static readonly IAppServices Services = new DnnServices();
    }
}