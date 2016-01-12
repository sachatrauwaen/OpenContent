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
            OpenContentController ctrl = new OpenContentController();
            OpenContentSettings settings = this.OpenContentSettings();
            int ModId = settings.IsOtherModule ? settings.ModuleId : ModuleId;
            OpenContentInfo data = ctrl.GetFirstContent(ModId);
            if (data != null)
            {
                var d = new DateTime(long.Parse(ddlVersions.SelectedValue));
                var ver = data.Versions.Single(v => v.CreatedOnDate == d);
                txtSource.Text = ver.Json.ToString();
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
                    OpenContentController ctrl = new OpenContentController();
                    TemplateManifest template = null;
                    OpenContentSettings settings = this.OpenContentSettings();
                    int ModId = settings.IsOtherModule ? settings.ModuleId : ModuleId;
                    if (settings.TemplateAvailable)
                    {
                        template = settings.Template;
                    }
                    if (template != null && template.IsListTemplate)
                    {
                        int itemId = Request.QueryString["id"] == null ? -1 : int.Parse(Request.QueryString["id"]);
                        if (itemId > 0)
                        {
                            OpenContentInfo data = ctrl.GetContent(itemId);
                            if (data != null)
                            {
                                json = data.Json;
                                foreach (var ver in data.Versions)
                                {
                                    ddlVersions.Items.Add(new ListItem()
                                    {
                                        Text = ver.CreatedOnDate.ToShortDateString() + " " + ver.CreatedOnDate.ToShortTimeString(),
                                        Value = ver.CreatedOnDate.Ticks.ToString()
                                    });
                                }
                            }
                        }
                        else
                        {
                            var dataList = ctrl.GetContents(ModId);
                            if (dataList != null)
                            {
                                JArray lst = new JArray();
                                foreach (var item in dataList)
                                {
                                    lst.Add(JObject.Parse(item.Json));
                                }
                                json = lst.ToString();
                            }
                        }
                    }
                    else
                    {
                        OpenContentInfo data = ctrl.GetFirstContent(ModId);
                        if (data != null)
                        {
                            json = data.Json;
                            foreach (var ver in data.Versions)
                            {
                                ddlVersions.Items.Add(new ListItem()
                                {
                                    Text = ver.CreatedOnDate.ToShortDateString() + " " + ver.CreatedOnDate.ToShortTimeString(),
                                    Value = ver.CreatedOnDate.Ticks.ToString()
                                });
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
            }

            txtSource.Text = json;
        }

        private void LoadFiles()
        {
            sourceList.Items.Clear();
            sourceList.Items.Add(new ListItem(cData, cData));
            sourceList.Items.Add(new ListItem(cSettings, cSettings));
            sourceList.Items.Add(new ListItem(cFilter, cFilter));
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
            Response.Redirect(Globals.NavigateURL(), true);
        }

        private void SaveFilter()
        {
            ModuleController mc = new ModuleController();
            if (!string.IsNullOrEmpty(txtSource.Text))
                mc.UpdateModuleSetting(ModuleId, "query", txtSource.Text);
        }

        private void SaveData()
        {
            OpenContentController ctrl = new OpenContentController();
            TemplateManifest template = null;
            OpenContentSettings settings = this.OpenContentSettings();
            int ModId = settings.IsOtherModule ? settings.ModuleId : ModuleId;
            bool index = false;
            if (settings.TemplateAvailable)
            {
                template = settings.Template;
                index = settings.Template.Manifest.Index;
            }
            FieldConfig indexConfig = null;
            if (index)
            {
                indexConfig = OpenContentUtils.GetIndexConfig(settings.Template.Key.TemplateDir);
            }

            if (template != null && template.IsListTemplate)
            {
                int itemId = Request.QueryString["id"] == null ? -1 : int.Parse(Request.QueryString["id"]);
                if (itemId > 0)
                {
                    var data = ctrl.GetContent(itemId);
                    if (string.IsNullOrEmpty(txtSource.Text))
                    {
                        if (data != null)
                        {
                            ctrl.DeleteContent(data, index);
                        }
                    }
                    else
                    {
                        JObject json = txtSource.Text.ToJObject("Saving txtSource");
                        if (data == null)
                        {
                            data = new OpenContentInfo()
                            {
                                ModuleId = ModId,
                                Title = json["Title"] == null ? ModuleContext.Configuration.ModuleTitle : json["Title"].ToString(),
                                CreatedByUserId = UserInfo.UserID,
                                CreatedOnDate = DateTime.Now,
                                LastModifiedByUserId = UserInfo.UserID,
                                LastModifiedOnDate = DateTime.Now,
                                Html = "",
                                Json = txtSource.Text
                            };
                            ctrl.AddContent(data, index, indexConfig);
                        }
                        else
                        {
                            data.Title = json["Title"] == null ? ModuleContext.Configuration.ModuleTitle : json["Title"].ToString();
                            data.LastModifiedByUserId = UserInfo.UserID;
                            data.LastModifiedOnDate = DateTime.Now;
                            data.Json = txtSource.Text;
                            ctrl.UpdateContent(data, index, indexConfig);
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

                    var dataList = ctrl.GetContents(ModId);
                    foreach (var item in dataList)
                    {
                        ctrl.DeleteContent(item, index);
                    }
                    if (lst != null)
                    {
                        foreach (JObject json in lst)
                        {
                            var data = new OpenContentInfo()
                            {
                                ModuleId = ModId,
                                Title = json["Title"] == null ? ModuleContext.Configuration.ModuleTitle : json["Title"].ToString(),
                                CreatedByUserId = UserInfo.UserID,
                                CreatedOnDate = DateTime.Now,
                                LastModifiedByUserId = UserInfo.UserID,
                                LastModifiedOnDate = DateTime.Now,
                                Html = "",
                                Json = json.ToString()
                            };
                            ctrl.AddContent(data, index, indexConfig);
                        }
                    }
                }
            }
            else
            {
                var data = ctrl.GetFirstContent(ModId);

                if (string.IsNullOrEmpty(txtSource.Text))
                {
                    if (data != null)
                    {
                        ctrl.DeleteContent(data, index);
                    }
                }
                else
                {
                    JObject json = txtSource.Text.ToJObject("Saving txtSource");
                    if (data == null)
                    {
                        data = new OpenContentInfo()
                        {
                            ModuleId = ModId,
                            Title = json["Title"] == null ? ModuleContext.Configuration.ModuleTitle : json["Title"].ToString(),
                            CreatedByUserId = UserInfo.UserID,
                            CreatedOnDate = DateTime.Now,
                            LastModifiedByUserId = UserInfo.UserID,
                            LastModifiedOnDate = DateTime.Now,
                            Html = "",
                            Json = txtSource.Text
                        };
                        ctrl.AddContent(data, index, indexConfig);
                    }
                    else
                    {
                        data.Title = json["Title"] == null ? ModuleContext.Configuration.ModuleTitle : json["Title"].ToString();
                        data.LastModifiedByUserId = UserInfo.UserID;
                        data.LastModifiedOnDate = DateTime.Now;
                        data.Json = txtSource.Text;
                        ctrl.UpdateContent(data, index, indexConfig);
                    }
                    if (json["form"]["ModuleTitle"] != null && json["form"]["ModuleTitle"].Type == JTokenType.String)
                    {
                        string ModuleTitle = json["form"]["ModuleTitle"].ToString();
                        OpenContentUtils.UpdateModuleTitle(ModuleContext.Configuration, ModuleTitle);
                    }
                    else if (json["form"]["ModuleTitle"] != null && json["form"]["ModuleTitle"].Type == JTokenType.Object)
                    {
                        string ModuleTitle = json["form"]["ModuleTitle"][DnnUtils.GetCurrentCultureCode()].ToString();
                        OpenContentUtils.UpdateModuleTitle(ModuleContext.Configuration, ModuleTitle);
                    }
                }
            }
        }
        private void SaveSettings()
        {
            ModuleController mc = new ModuleController();
            if (!string.IsNullOrEmpty(txtSource.Text))
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

