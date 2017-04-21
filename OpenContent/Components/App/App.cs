using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components
{
    public class App : AppBase
    {
        private static readonly Lazy<AppBase> Lazy = new Lazy<AppBase>(() => new App());
        public new static IAppConfig Config => Lazy.Value.Config;

        private App()
        {
            AppBase.Init(new MyConfig());
        }

    }
}