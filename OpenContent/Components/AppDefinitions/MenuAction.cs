using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.AppDefinitions
{
    public class MenuAction:IMenuAction
    {
        public string Title { get; }
        public ActionType ActionType { get; }
        public string Image { get; }
        public string Url { get; }
    }
}