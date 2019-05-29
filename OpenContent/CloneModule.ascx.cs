#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using System.Linq;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework.JavaScriptLibraries;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Manifest;

#endregion

namespace Satrabel.OpenContent
{
    public partial class CloneModule : PortalModuleBase
    {


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            cmdSave.Click += cmdSave_Click;

            var m = ModuleContext.Configuration;
            TabController tc = new TabController();
            var mc = new ModuleController();
            // its the same for each module -> to cache
            var til = TabController.GetTabsBySortOrder(PortalSettings.PortalId, m.CultureCode, true)
                .Where(t => /*!t.IsDeleted &&*/ t.ParentId != PortalSettings.AdminTabId
                        && t.TabID != PortalSettings.AdminTabId
                        && t.CultureCode == m.CultureCode);
            //var tid = tc.GetTabsByModuleID(m.ModuleID);
            var tmiLst = mc.GetAllTabsModulesByModuleID(m.ModuleID).Cast<ModuleInfo>();

            foreach (TabInfo ti in til)
            {
                {
                    ListItem li = new ListItem(ti.TabName, ti.TabID.ToString());
                    li.Text = ti.IndentedTabName;
                    li.Enabled = ti.TabID != m.TabID;
                    cblPages.Items.Add(li);

                    ModuleInfo tmi = tmiLst.SingleOrDefault(t => t.TabID == ti.TabID);

                    //if (tid.Keys.Contains(ti.TabID))
                    if (tmi != null)
                    {
                        if (tmi.IsDeleted)
                        {
                            //li.Enabled = false;
                            li.Text = "<i>" + li.Text + "</i>";
                        }
                        else
                        {
                            li.Selected = true;
                        }
                    }
                }
            }
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            var mi = ModuleContext.Configuration;
            var mc = new ModuleController();
            TabController tc = new TabController();
            var tabModules = mc.GetAllTabsModulesByModuleID(mi.ModuleID).Cast<ModuleInfo>().Where(t => t.IsDeleted == false);
            foreach (ListItem li in cblPages.Items)
            {
                if (li.Enabled)
                {
                    bool Add = li.Selected;
                    int TabId = int.Parse(li.Value);
                    if (Add && !tabModules.Any(m => m.TabID == TabId))
                    {
                        ModuleInfo sourceModule = mc.GetModule(mi.ModuleID, mi.TabID, false);
                        TabInfo destinationTab = tc.GetTab(TabId, PortalSettings.PortalId, false);

                        ModuleInfo tmpModule = mc.GetModule(mi.ModuleID, TabId, false);
                        if (tmpModule != null && tmpModule.IsDeleted)
                        {
                            mc.RestoreModule(tmpModule);
                        }
                        else
                        {
                            mc.CopyModule(sourceModule, destinationTab, Null.NullString, true);
                        }
                        if (sourceModule.DefaultLanguageModule != null && destinationTab.DefaultLanguageTab != null) // not default language
                        {
                            ModuleInfo defaultLanguageModule = mc.GetModule(sourceModule.DefaultLanguageModule.ModuleID, destinationTab.DefaultLanguageTab.TabID, false);
                            if (defaultLanguageModule != null)
                            {
                                ModuleInfo destinationModule = destinationModule = mc.GetModule(sourceModule.ModuleID, destinationTab.TabID, false);
                                destinationModule.DefaultLanguageGuid = defaultLanguageModule.UniqueId;
                                mc.UpdateModule(destinationModule);
                            }
                        }

                    }
                    else if (!Add && tabModules.Any(m => m.TabID == TabId))
                    {
                        mc.DeleteTabModule(TabId, mi.ModuleID, true);
                    }
                }
            }
            DataCache.ClearCache();
            Response.Redirect(Globals.NavigateURL(), true);
        }
    }
}