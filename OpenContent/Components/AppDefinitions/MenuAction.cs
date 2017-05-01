using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.AppDefinitions
{
    public class MenuAction : IMenuAction
    {
        public MenuAction(string title, string image, string url, ActionType actionType, SecurityAccessLevel accessLevel = SecurityAccessLevel.EditRights, bool newWindow = false)
        {
            Title = title;
            ActionType = actionType;
            Image = image;
            Url = url;
            AccessLevel = accessLevel;
            NewWindow = newWindow;
        }

        public string Title { get; }
        public ActionType ActionType { get; }
        public string Image { get; }
        public string Url { get; }
        public SecurityAccessLevel AccessLevel { get; }
        public bool NewWindow { get; }
    }
}