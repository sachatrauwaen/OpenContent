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
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Lucene.Config;
using Satrabel.OpenContent.Components.Lucene.Index;
using Satrabel.OpenContent.Components.Manifest;

#endregion

namespace Satrabel.OpenContent
{
    public partial class EditAddData : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Key = Request.QueryString["key"];
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            OpenContentSettings settings = this.OpenContentSettings();
            AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext, settings.Template.Uri().FolderPath, Key);
            alpaca.RegisterAll();
            int ItemId = 0;//Request.QueryString["id"] == null ? -1 : int.Parse(Request.QueryString["id"]);
            AlpacaContext = new AlpacaContext(PortalId, ModuleId, ItemId, ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, null, null);
        }
        
        public AlpacaContext AlpacaContext { get; private set ; }

        public string Key { get; private set; }
    }
}

