#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using System.Linq;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Common;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Framework;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Localization;
using System.IO;
using Satrabel.OpenContent.Components;
using Newtonsoft.Json.Linq;
using System.Globalization;
using DotNetNuke.Common.Utilities;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Lucene.Config;
using Satrabel.OpenContent.Components.Datasource;

#endregion

namespace Satrabel.OpenContent
{

    public partial class EditData : PortalModuleBase
    {
        private const string cData = "Data";
        private const string cSettings = "Settings";
        private const string cFilter = "Filter";

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            cmdSave.Click += cmdSave_Click;
            cmdCancel.Click += cmdCancel_Click;
            //ServicesFramework.Instance.RequestAjaxScriptSupport();
            //ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            sourceList.SelectedIndexChanged += sourceList_SelectedIndexChanged;
            ddlVersions.SelectedIndexChanged += ddlVersions_SelectedIndexChanged;
        }
        private void ddlVersions_SelectedIndexChanged(object sender, EventArgs e)
        {

            OpenContentSettings settings = this.OpenContentSettings();
            int ModId = settings.IsOtherModule ? settings.ModuleId : ModuleId;
            var ds = DataSourceManager.GetDataSource("OpenContent");
            var dsContext = new DataSourceContext()
            {
                ModuleId = ModId,
                TemplateFolder = settings.TemplateDir.FolderPath,
                Single = true
            };
            var dsItem = ds.Get(dsContext, null);
            if (dsItem != null)
            {
                var ticks = long.Parse(ddlVersions.SelectedValue);
                var ver = ds.GetVersion(dsContext, dsItem, new DateTime(ticks));
                //var ver = data.Versions.Single(v => v.CreatedOnDate == d);
                txtSource.Text = ver.ToString();
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                InitEditor();
            }
        }
        private void InitEditor()
        {
            LoadFiles();
            DisplayFile(cData);
        }

        private void DisplayFile(string selectedDataType)
        {
            string json = string.Empty;
            switch (selectedDataType)
            {
                case cData:
                    {
                        TemplateManifest template = null;
                        OpenContentSettings settings = this.OpenContentSettings();
                        int ModId = settings.IsOtherModule ? settings.ModuleId : ModuleId;
                        if (settings.TemplateAvailable)
                        {
                            template = settings.Template;
                        }
                        var ds = DataSourceManager.GetDataSource(settings.Manifest.DataSource);
                        var dsContext = new DataSourceContext()
                        {
                            ModuleId = ModId,
                            TemplateFolder = settings.TemplateDir.FolderPath,
                            Config = settings.Manifest.DataSourceConfig
                        };
                        if (template != null && template.IsListTemplate)
                        {
                            string itemId = Request.QueryString["id"];
                            if (!string.IsNullOrEmpty(itemId))
                            {
                                var dsItem = ds.Get(dsContext, itemId);

                                if (dsItem != null)
                                {
                                    json = dsItem.Data.ToString();
                                    var versions = ds.GetVersions(dsContext, dsItem);
                                    if (versions != null)
                                        foreach (var ver in versions)
                                        {
                                            ddlVersions.Items.Add(new ListItem()
                                            {
                                                //Text = ver.CreatedOnDate.ToShortDateString() + " " + ver.CreatedOnDate.ToShortTimeString(),
                                                //Value = ver.CreatedOnDate.Ticks.ToString()
                                                Text = ver["text"].ToString(),
                                                Value = ver["ticks"].ToString()
                                            });
                                        }
                                }
                            }
                            else
                            {
                                var dataList = ds.GetAll(dsContext, null);
                                if (dataList != null)
                                {
                                    JArray lst = new JArray();
                                    foreach (var item in dataList.Items)
                                    {
                                        lst.Add(item.Data);
                                    }
                                    json = lst.ToString();
                                }
                            }
                        }
                        else
                        {
                            dsContext.Single = true;
                            var dsItem = ds.Get(dsContext, null);
                            if (dsItem != null)
                            {
                                json = dsItem.Data.ToString();
                                var versions = ds.GetVersions(dsContext, dsItem);
                                if (versions != null)
                                    foreach (var ver in versions)
                                    {
                                        ddlVersions.Items.Add(new ListItem()
                                        {
                                            //Text = ver.CreatedOnDate.ToShortDateString() + " " + ver.CreatedOnDate.ToShortTimeString(),
                                            //Value = ver.CreatedOnDate.Ticks.ToString()
                                            Text = ver["text"].ToString(),
                                            Value = ver["ticks"].ToString()
                                        });
                                    }
                            }
                        }
                    }
                    break;
                case cSettings:
                    json = ModuleContext.Settings["data"] as string;
                    break;
                case cFilter:
                    json = ModuleContext.Settings["query"] as string;
                    break;
                default:
                    {
                        OpenContentSettings settings = this.OpenContentSettings();
                        int ModId = settings.IsOtherModule ? settings.ModuleId : ModuleId;
                        var manifest = settings.Manifest;
                        string key = selectedDataType;
                        var dataManifest = manifest.AdditionalData[key];
                        string scope = AdditionalDataUtils.GetScope(dataManifest, PortalSettings.PortalId, PortalSettings.ActiveTab.TabID, ModId, this.TabModuleId);
                        var dc = new AdditionalDataController();
                        var data = dc.GetData(scope, dataManifest.StorageKey ?? key);

                        json = data == null ? "" : data.Json;
                        break;
                    }
            }

            txtSource.Text = json;
        }

        private void LoadFiles()
        {
            TemplateManifest template = ModuleContext.OpenContentSettings().Template;
            sourceList.Items.Clear();
            sourceList.Items.Add(new ListItem(cData, cData));
            sourceList.Items.Add(new ListItem(cSettings, cSettings));
            sourceList.Items.Add(new ListItem(cFilter, cFilter));
            if (template.Manifest.AdditionalData != null)
            {
                foreach (var addData in template.Manifest.AdditionalData)
                {
                    string title = string.IsNullOrEmpty(addData.Value.Title) ? addData.Key : addData.Value.Title;
                    sourceList.Items.Add(new ListItem(title, addData.Key));
                }
            }
        }

        protected void cmdSave_Click(object sender, EventArgs e)
        {
            if (sourceList.SelectedValue == cData)
            {
                SaveData();
            }
            else if (sourceList.SelectedValue == cSettings)
            {
                SaveSettings();
            }
            else if (sourceList.SelectedValue == cFilter)
            {
                SaveFilter();
            }
            else
            {
                SaveAdditionalData(sourceList.SelectedValue);
            }
            Response.Redirect(Globals.NavigateURL(), true);
        }

        private void SaveAdditionalData(string key)
        {
            OpenContentSettings settings = this.OpenContentSettings();
            int ModId = settings.IsOtherModule ? settings.ModuleId : ModuleId;
            var manifest = settings.Manifest;
            var dataManifest = manifest.AdditionalData[key];
            string scope = AdditionalDataUtils.GetScope(dataManifest, PortalSettings.PortalId, PortalSettings.ActiveTab.TabID, ModId, this.TabModuleId);
            var dc = new AdditionalDataController();
            var data = dc.GetData(scope, dataManifest.StorageKey ?? key);
            if (data == null)
            {

                data = new AdditionalDataInfo()
                {
                    CreatedByUserId = UserId,
                    CreatedOnDate = DateTime.Now,
                    DataKey = key,
                    Json = txtSource.Text,
                    LastModifiedByUserId = UserId,
                    LastModifiedOnDate = DateTime.Now,
                    Scope = scope
                };
                dc.AddData(data);
            }
            else
            {
                data.Json = txtSource.Text;
                data.LastModifiedByUserId = UserId;
                data.LastModifiedOnDate = DateTime.Now;
                dc.UpdateData(data);
            }
        }

        private void SaveFilter()
        {
            ModuleController mc = new ModuleController();
            if (!string.IsNullOrEmpty(txtSource.Text))
                mc.UpdateModuleSetting(ModuleId, "query", txtSource.Text);
        }

        private void SaveData()
        {
            TemplateManifest template = null;
            OpenContentSettings settings = this.OpenContentSettings();
            int ModId = settings.IsOtherModule ? settings.ModuleId : ModuleId;
            bool index = false;
            if (settings.TemplateAvailable)
            {
                template = settings.Template;
                index = settings.Template.Manifest.Index;
            }
            /*
            FieldConfig indexConfig = null;
            if (index)
            {
                indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
            }
             */
            var ds = DataSourceManager.GetDataSource(settings.Manifest.DataSource);
            var dsContext = new DataSourceContext()
            {
                ModuleId = ModId,
                TemplateFolder = settings.TemplateDir.FolderPath,
                Index = index,
                UserId = UserInfo.UserID,
                Config = settings.Manifest.DataSourceConfig
            };
            if (template != null && template.IsListTemplate)
            {
                string itemId = Request.QueryString["id"];
                if (!string.IsNullOrEmpty(itemId))
                {
                    var dsItem = ds.Get(dsContext, itemId);
                    if (string.IsNullOrEmpty(txtSource.Text))
                    {
                        if (dsItem != null)
                        {
                            ds.Delete(dsContext, dsItem);
                        }
                    }
                    else
                    {
                        var json = txtSource.Text.ToJObject("Saving txtSource");
                        if (dsItem == null)
                        {
                            ds.Add(dsContext, json);
                        }
                        else
                        {
                            ds.Update(dsContext, dsItem, json);
                        }
                        if (json["ModuleTitle"] != null && json["ModuleTitle"].Type == JTokenType.String)
                        {
                            string ModuleTitle = json["ModuleTitle"].ToString();
                            OpenContentUtils.UpdateModuleTitle(ModuleContext.Configuration, ModuleTitle);
                        }
                        else if (json["ModuleTitle"] != null && json["ModuleTitle"].Type == JTokenType.Object)
                        {
                            string ModuleTitle = json["ModuleTitle"][DnnUtils.GetCurrentCultureCode()].ToString();
                            OpenContentUtils.UpdateModuleTitle(ModuleContext.Configuration, ModuleTitle);
                        }
                    }
                }
                else
                {
                    JArray lst = null;
                    if (!string.IsNullOrEmpty(txtSource.Text))
                    {
                        lst = JArray.Parse(txtSource.Text);
                    }
                    var dataList = ds.GetAll(dsContext, null).Items;
                    foreach (var item in dataList)
                    {
                        ds.Delete(dsContext, item);
                    }
                    if (lst != null)
                    {
                        foreach (JObject json in lst)
                        {
                            ds.Add(dsContext, json);
                        }
                    }
                }
            }
            else
            {
                dsContext.Single = true;
                var dsItem = ds.Get(dsContext, null);
                if (string.IsNullOrEmpty(txtSource.Text))
                {
                    if (dsItem != null)
                    {
                        ds.Delete(dsContext, dsItem);
                    }
                }
                else
                {
                    var json = txtSource.Text.ToJObject("Saving txtSource");
                    if (dsItem == null)
                    {
                        ds.Add(dsContext, json);
                    }
                    else
                    {
                        ds.Update(dsContext, dsItem, json);
                    }
                    if (json["ModuleTitle"] != null && json["ModuleTitle"].Type == JTokenType.String)
                    {
                        string ModuleTitle = json["ModuleTitle"].ToString();
                        OpenContentUtils.UpdateModuleTitle(ModuleContext.Configuration, ModuleTitle);
                    }
                    else if (json["ModuleTitle"] != null && json["ModuleTitle"].Type == JTokenType.Object)
                    {
                        string ModuleTitle = json["ModuleTitle"][DnnUtils.GetCurrentCultureCode()].ToString();
                        OpenContentUtils.UpdateModuleTitle(ModuleContext.Configuration, ModuleTitle);
                    }
                }
            }
        }
        private void SaveSettings()
        {
            ModuleController mc = new ModuleController();
            if (string.IsNullOrEmpty(txtSource.Text))
                mc.DeleteModuleSetting(ModuleId, "data");
            else
                mc.UpdateModuleSetting(ModuleId, "data", txtSource.Text);
        }
        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(Globals.NavigateURL(), true);
        }
        private void sourceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            phVersions.Visible = sourceList.SelectedValue == cData;
            DisplayFile(sourceList.SelectedValue);
        }
        #endregion
    }
}

