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
using System.Web.UI.WebControls;
using System.IO;
using Satrabel.OpenContent.Components;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Manifest;
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
            cmdImport.Click += cmdImport_Click;
            cmdRestApi.NavigateUrl = Globals.NavigateURL("Swagger", "mid=" + ModuleContext.ModuleId) + "?popUp=true";
            //ServicesFramework.Instance.RequestAjaxScriptSupport();
            //ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            sourceList.SelectedIndexChanged += sourceList_SelectedIndexChanged;
            ddlVersions.SelectedIndexChanged += ddlVersions_SelectedIndexChanged;
        }

        private void cmdImport_Click(object sender, EventArgs e)
        {
            OpenContentSettings settings = this.OpenContentSettings();

            switch (sourceList.SelectedValue)
            {
                case cData:
                    {
                        txtSource.Text = File.ReadAllText(settings.TemplateDir.PhysicalFullDirectory + "\\data.json");
                    }
                    break;
            }
        }
        private void ddlVersions_SelectedIndexChanged(object sender, EventArgs e)
        {

            OpenContentSettings settings = this.OpenContentSettings();
            int ModId = settings.IsOtherModule ? settings.ModuleId : ModuleId;
            var ds = DataSourceManager.GetDataSource("OpenContent");
            var dsContext = new DataSourceContext()
            {
                ModuleId = ModId,
                ActiveModuleId = ModuleContext.ModuleId,
                TemplateFolder = settings.TemplateDir.FolderPath,
                PortalId = ModuleContext.PortalId,
                CurrentCultureCode = DnnLanguageUtils.GetCurrentCultureCode(),
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
            cmdImport.Visible = false;
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
                            ActiveModuleId = ModuleContext.ModuleId,
                            TemplateFolder = settings.TemplateDir.FolderPath,
                            PortalId = ModuleContext.PortalId,
                            CurrentCultureCode = DnnLanguageUtils.GetCurrentCultureCode(),
                            Config = settings.Manifest.DataSourceConfig
                        };
                        if (template != null && template.IsListTemplate)
                        {
                            ddlVersions.Visible = false;
                            cmdRestApi.Visible = true;
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
                            ddlVersions.Visible = true;
                            cmdRestApi.Visible = false;
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
                        cmdImport.Visible = string.IsNullOrEmpty(json) && File.Exists(settings.TemplateDir.PhysicalFullDirectory + "\\data.json");
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
                        var dataManifest = manifest.GetAdditionalData(key);
                        var ds = DataSourceManager.GetDataSource(manifest.DataSource);
                        var dsContext = new DataSourceContext()
                        {
                            PortalId = PortalSettings.PortalId,
                            CurrentCultureCode = DnnLanguageUtils.GetCurrentCultureCode(),
                            TabId = TabId,
                            ModuleId = ModId,
                            TabModuleId = this.TabModuleId,
                            UserId = UserInfo.UserID,
                            TemplateFolder = settings.TemplateDir.FolderPath,
                            Config = manifest.DataSourceConfig,
                            //Options = reqOptions
                        };
                        var dsItem = ds.GetData(dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? key);
                        json = dsItem == null ? "" : dsItem.Data.ToString();
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
            if (template != null && template.Manifest != null && template.Manifest.AdditionalDataExists())
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
            var dataManifest = manifest.GetAdditionalData(key);
            var ds = DataSourceManager.GetDataSource(manifest.DataSource);
            var dsContext = new DataSourceContext()
            {
                PortalId = PortalSettings.PortalId,
                CurrentCultureCode = DnnLanguageUtils.GetCurrentCultureCode(),
                TabId = TabId,
                ModuleId = ModId,
                TabModuleId = this.TabModuleId,
                UserId = UserInfo.UserID,
                TemplateFolder = settings.TemplateDir.FolderPath,
                Config = manifest.DataSourceConfig,
                //Options = reqOptions
            };
            var dsItem = ds.GetData(dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? key);
            if (dsItem == null)
            {
                ds.AddData(dsContext, dataManifest.ScopeType, dataManifest.StorageKey ?? key, JToken.Parse(txtSource.Text));
            }
            else
            {
                ds.UpdateData(dsContext, dsItem, JToken.Parse(txtSource.Text));
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
                ActiveModuleId = ModuleContext.ModuleId,
                TemplateFolder = settings.TemplateDir.FolderPath,
                Index = index,
                UserId = UserInfo.UserID,
                PortalId = ModuleContext.PortalId,
                CurrentCultureCode = DnnLanguageUtils.GetCurrentCultureCode(),
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
                            string ModuleTitle = json["ModuleTitle"][DnnLanguageUtils.GetCurrentCultureCode()].ToString();
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
                        string ModuleTitle = json["ModuleTitle"][DnnLanguageUtils.GetCurrentCultureCode()].ToString();
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

