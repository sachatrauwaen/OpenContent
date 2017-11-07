#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Common;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Lucene;

#endregion

namespace Satrabel.OpenContent
{
    public partial class EditNotifications : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            OpenContentSettings settings = this.OpenContentSettings();
            AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext.PortalId, "DeskTopModules/OpenContent", "notifications");
            //AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext, "", "");
            alpaca.RegisterAll(true, true);
            string itemId = null;
            AlpacaContext = new AlpacaContext(PortalId, ModuleId, itemId, ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, null, null, null);
            AlpacaContext.Bootstrap = true;
            AlpacaContext.Horizontal = true;
        }

        public AlpacaContext AlpacaContext { get; private set; }
    }
}

