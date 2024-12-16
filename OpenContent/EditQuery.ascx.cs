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
using Satrabel.OpenContent.Components.Render;

#endregion

namespace Satrabel.OpenContent
{
    public partial class EditQuery : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var pageContext = new WebFormsPageContext(Page, this);
            bIndexAll.Visible = UserInfo.IsSuperUser;
            hlCancel.NavigateUrl = Globals.NavigateURL();
            cmdSave.NavigateUrl = Globals.NavigateURL();
            //OpenContentSettings settings = this.OpenContentSettings();
            //AlpacaEngine alpaca = new AlpacaEngine(Page, ModuleContext, settings.Template.Uri().FolderPath, "query");
            AlpacaEngine alpaca = new AlpacaEngine(pageContext, ModuleContext.PortalId, "", "");
            alpaca.RegisterAll(false, false, false, false);
            string itemId = null;//Request.QueryString["id"] == null ? -1 : int.Parse(Request.QueryString["id"]);
            AlpacaContext = new AlpacaContext(PortalId, ModuleId, itemId, ScopeWrapper.ClientID, hlCancel.ClientID, cmdSave.ClientID, null, null, null);
        }
        protected void bIndex_Click(object sender, EventArgs e)
        {
            var module = OpenContentModuleConfig.Create(ModuleConfiguration, PortalSettings);
            LuceneUtils.ReIndexModuleData(module);
        }
        protected void bIndexAll_Click(object sender, EventArgs e)
        {
            LuceneUtils.IndexAll();
        }
        protected void bGenerate_Click(object sender, EventArgs e)
        {
            /*
            OpenContentController occ = new OpenContentController();
            var oc = occ.GetFirstContent(ModuleId);
            if (oc != null)
            {
                var data = JObject.Parse(oc.Json);
                for (int i = 0; i < 10000; i++)
                {
                    data["Title"] = "Title " + i;
                    var newoc = new OpenContentInfo()
                    {
                        Title = "check" + i,
                        ModuleId = ModuleId,
                        Html = "tst",
                        Json = data.ToString(),
                        CreatedByUserId = UserId,
                        CreatedOnDate = DateTime.Now,
                        LastModifiedByUserId = UserId,
                        LastModifiedOnDate = DateTime.Now

                    };
                    occ.AddContent(newoc, true, null);
                }
            }
            */
        }

        public AlpacaContext AlpacaContext { get; private set; }
    }
}

