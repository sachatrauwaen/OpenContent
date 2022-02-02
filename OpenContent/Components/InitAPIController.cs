#region Copyright

// 
// Copyright (c) 2015-2017
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using Newtonsoft.Json.Linq;
using System.IO;
using DotNetNuke.Security;
using Satrabel.OpenContent.Components.Json;
using DotNetNuke.Entities.Modules;
using System.Collections.Generic;
using DotNetNuke.Services.Localization;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Dnn;
using DotNetNuke.Entities.Tabs;
using System.Web.Hosting;
using Satrabel.OpenContent.Components.Logging;
using DotNetNuke.Entities.Portals;

#endregion

namespace Satrabel.OpenContent.Components
{
    [SupportedModules("OpenContent")]
    public class InitAPIController : DnnApiController
    {
        public object TemplateAvailable { get; private set; }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [HttpGet]
        public List<PortalDto> GetPortals()
        {
            IEnumerable<PortalInfo> portals = (new PortalController()).GetPortals().Cast<PortalInfo>();
            var listItems = new List<PortalDto>();
            foreach (var item in portals)
            {
                var li = new PortalDto()
                {
                    Text = item.PortalName,
                    PortalId = item.PortalID
                };
                listItems.Add(li);
            }
            return listItems.OrderBy(x => x.Text).ToList();
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [HttpGet]
        public List<ModuleDto> GetModules()
        {
            IEnumerable<ModuleInfo> modules = (new ModuleController()).GetModules(ActiveModule.PortalID).Cast<ModuleInfo>();
            modules = modules.Where(m => m.ModuleDefinition.DefinitionName == App.Config.Opencontent && m.IsDeleted == false && !m.OpenContentSettings().IsOtherModule);
            var listItems = new List<ModuleDto>();
            foreach (var item in modules)
            {
                if (item.TabModuleID != ActiveModule.TabModuleID)
                {
                    var tc = new TabController();
                    var tab = tc.GetTab(item.TabID, ActiveModule.PortalID, false);
                    //if (!tab.IsNeutralCulture && tab.CultureCode != DnnLanguageUtils.GetCurrentCultureCode())
                    //{
                    //    // skip other cultures
                    //    continue;
                    //}
                    var tabpath = tab.TabPath.Replace("//", "/").Trim('/');
                    if (!tab.IsNeutralCulture && tab.CultureCode != DnnLanguageUtils.GetCurrentCultureCode())
                    {
                        // skip other cultures
                        //continue;
                    }
                    else
                    {
                        var li = new ModuleDto()
                        {
                            Text = string.Format("{1} - {0}", item.ModuleTitle, tabpath),
                            TabModuleId = item.TabModuleID
                        };
                        listItems.Add(li);
                    }
                }
            }
            return listItems.OrderBy(x => x.Text).ToList();
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [HttpGet]
        public List<TemplateDto> GetTemplates(bool advanced)
        {
            var scriptFileSetting = ActiveModule.OpenContentSettings().Template;
            var templates = OpenContentUtils.ListOfTemplatesFiles(PortalSettings, ActiveModule.ModuleID, scriptFileSetting, App.Config.Opencontent, advanced);
            return templates.Select(t => new TemplateDto()
            {
                Value = t.Value,
                Text = t.Text
            }).ToList();
        }
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [HttpGet]
        public List<TemplateDto> GetTemplates(int tabModuleId)
        {
            var dsModule = (new ModuleController()).GetTabModule(tabModuleId);
            var dsSettings = dsModule.OpenContentSettings();
            var templates = OpenContentUtils.ListOfTemplatesFiles(PortalSettings, ActiveModule.ModuleID, dsSettings.Template, App.Config.Opencontent, dsSettings.Template.MainTemplateUri());
            return templates.Select(t => new TemplateDto()
            {
                Value = t.Value,
                Text = t.Text
            }).ToList();
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [HttpGet]
        public List<TemplateDto> GetNewTemplates(bool web)
        {
            if (web)
            {
                var templates = GithubTemplateUtils.GetTemplateList(ActiveModule.PortalID).Where(t => t.Type == Components.Github.TypeEnum.Dir).OrderBy(t => t.Name);
                return templates.Select(t => new TemplateDto()
                {
                    Value = t.Path,
                    Text = t.Name
                }).ToList();
            }
            else
            {
                var scriptFileSetting = ActiveModule.OpenContentSettings().Template;
                var templates = OpenContentUtils.GetTemplates(PortalSettings, ActiveModule.ModuleID, scriptFileSetting, App.Config.Opencontent);
                return templates.Select(t => new TemplateDto()
                {
                    Value = t.Value,
                    Text = t.Text
                }).ToList();
            }

        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [HttpGet]
        public ModuleStateDto GetTemplateState()
        {
            //create tmp TemplateManifest
            //var templateManifest = new FileUri(template).ToTemplateManifest();
            var settings = ActiveModule.OpenContentSettings();
            var templateManifest = settings.Template;
            if (templateManifest == null)
            {
                return new ModuleStateDto()
                {
                    Template = ""
                };
            }
            else
            {
                return new ModuleStateDto()
                {
                    Template = settings.TemplateKey.ToString(),
                    SettingsNeeded = templateManifest.SettingsNeeded(),
                    //templateDefined = templateDefined && (!ddlTemplate.Visible || (settings.Template.Key.ToString() == ddlTemplate.SelectedValue));
                    SettingsDefined = !string.IsNullOrEmpty(settings.Data)
                };
            }
        }
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [HttpPost]
        public ModuleStateDto SaveTemplate(SaveDto input)
        {
            ModuleController mc = new ModuleController();
            if (!input.otherModule) // this module
            {
                mc.DeleteModuleSetting(ActiveModule.ModuleID, "tabid");
                mc.DeleteModuleSetting(ActiveModule.ModuleID, "moduleid");
            }
            else // other module
            {
                var dsModule = (new ModuleController()).GetTabModule(input.tabModuleId);
                mc.UpdateModuleSetting(ActiveModule.ModuleID, "tabid", dsModule.TabID.ToString());
                mc.UpdateModuleSetting(ActiveModule.ModuleID, "moduleid", dsModule.ModuleID.ToString());
            }
            if (!input.newTemplate) // existing
            {
                mc.UpdateModuleSetting(ActiveModule.ModuleID, "template", input.template);
                ActiveModule.ModuleSettings["template"] = input.template;
            }
            else // new
            {
                try
                {
                    if (!input.fromWeb) // site
                    {
                        string oldFolder = HostingEnvironment.MapPath(input.template);
                        var template = OpenContentUtils.CopyTemplate(ActiveModule.PortalID, oldFolder, input.templateName);
                        mc.UpdateModuleSetting(ActiveModule.ModuleID, "template", template);
                        ActiveModule.ModuleSettings["template"] = template;
                    }
                    else  // web
                    {
                        //string fileName = ddlTemplate.SelectedValue;
                        //string template = OpenContentUtils.ImportFromWeb(ModuleContext.PortalId, fileName, tbTemplateName.Text);
                        //mc.UpdateModuleSetting(ModuleContext.ModuleId, "template", template);
                        //ModuleContext.Settings["template"] = template;
                        //string fileName = ddlTemplate.SelectedValue;

                        var template = GithubTemplateUtils.ImportFromGithub(ActiveModule.PortalID, Path.GetFileNameWithoutExtension(input.template), input.template, input.templateName);

                        mc.UpdateModuleSetting(ActiveModule.ModuleID, "template", template);
                        ActiveModule.ModuleSettings["template"] = template;
                    }
                }
                catch (Exception ex)
                {
                    return new ModuleStateDto()
                    {
                        Error = ex.Message
                    };
                }
            }
            //mc.UpdateModuleSetting(ActiveModule.ModuleID, "detailtabid", ddlDetailPage.SelectedValue);


            //don't reset settings. Sure they might be invalid, but maybe not. And you can't ever revert.
            //mc.DeleteModuleSetting(ModuleContext.ModuleId, "data");

            var settings = ActiveModule.OpenContentSettings();
            var templateManifest = settings.Template;
            if (templateManifest.SettingsNeeded())
            {
                var settingsFilename = templateManifest.MainTemplateUri().PhysicalFullDirectory + "\\" + templateManifest.Key.ShortKey + "-data.json";
                if (File.Exists(settingsFilename))
                {
                    var settingContent = File.ReadAllText(settingsFilename);
                    mc.UpdateModuleSetting(ActiveModule.ModuleID, "data", settingContent);
                    ActiveModule.ModuleSettings["data"] = settingContent;
                    settings = ActiveModule.OpenContentSettings();
                }
            }
            // filter settings
            var filterFilename = templateManifest.MainTemplateUri().PhysicalFullDirectory + "\\" + "filter-data.json";
            if (File.Exists(filterFilename))
            {
                var settingContent = File.ReadAllText(filterFilename);
                mc.UpdateModuleSetting(ActiveModule.ModuleID, "query", settingContent);
                ActiveModule.ModuleSettings["query"] = settingContent;
                settings = ActiveModule.OpenContentSettings();
            }
            bool defaultData = false;
            if (templateManifest.DataNeeded())
            {
                var dataFilename = templateManifest.MainTemplateUri().PhysicalFullDirectory + "\\data.json";
                if (File.Exists(dataFilename))
                {
                    var module = OpenContentModuleConfig.Create(ActiveModule, PortalSettings);
                    IDataSource ds = DataSourceManager.GetDataSource(settings.Manifest.DataSource);
                    var dsContext = OpenContentUtils.CreateDataContext(module, UserInfo.UserID);
                    ds.Add(dsContext, JObject.Parse(File.ReadAllText(dataFilename)));
                    defaultData = true;
                }
            }

            return new ModuleStateDto()
            {
                SettingsNeeded = templateManifest.SettingsNeeded(),
                SettingsDefined = !string.IsNullOrEmpty(settings.Data),
                DataNeeded = settings.Template.DataNeeded() && !defaultData,
                Template = settings.TemplateKey.ToString(),
                Error = ""
            };
        }

        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        public List<PageDto> GetDetailPages(string template, int tabModuleId)
        {
            string format;
            int othermoduleTabId = ActiveModule.TabID;
            int moduleId = ActiveModule.ModuleID;

            var listItems = new List<PageDto>();
            var templateUri = new FileUri(template);
            var manifest = templateUri.ToTemplateManifest();

            int othermoduleDetailTabId = -1;
            if (manifest != null && manifest.IsListTemplate && manifest.Manifest.Templates.Any(t => t.Value.Detail != null))
            {
                if (tabModuleId > 0)
                {
                    var dsModule = (new ModuleController()).GetTabModule(tabModuleId);
                    othermoduleTabId = dsModule.TabID;
                    moduleId = dsModule.ModuleID;
                    othermoduleDetailTabId = GetOtherModuleDetailTabId(othermoduleTabId, moduleId);
                    //if (othermoduleDetailTabId > 0)
                    //{
                    //    //add extra li with "Default Detail Page" directly to dropdown
                    //    format = LogContext.IsLogActive ? "Main Module Detail Page - [{0}]" : "Main Module Detail Page";
                    //    listItems.Add(new PageDto()
                    //    {
                    //        Text = string.Format(format, othermoduleDetailTabId),
                    //        TabId = othermoduleDetailTabId
                    //    });
                    //}
                }

                Dictionary<string, int> tabs = TabController.GetTabPathDictionary(ActiveModule.PortalID, DnnLanguageUtils.GetCurrentCultureCode());

                foreach (var tabId in tabs.Where(i => IsTabWithModuleWithSameMainModule(i.Value, moduleId) && IsAccessibleTab(i.Value)))
                {
                    string tabname = tabId.Key.Replace("//", " / ").TrimStart(" / ");

                    List<string> infoText = new List<string>();

                    if (othermoduleDetailTabId > 0 && tabId.Value == othermoduleDetailTabId)
                    {
                        infoText.Add("Main Module Detail");
                    }

                    if ((othermoduleTabId > 0 && tabId.Value == othermoduleTabId) || (othermoduleTabId == -1 && tabId.Value == ActiveModule.TabID))
                    {
                        //add extra li with "Main Module Page" directly to dropdown
                        //format = LogContext.IsLogActive ? "Main Module Page - {0} [{1}]" : "Main Module Page";
                        //listItems.Add(new PageDto()
                        //{
                        //    Text = string.Format(format, tabname, tabId.Value),
                        //    TabId = -1
                        //});
                        infoText.Add("Main Module ");
                    }
                    if (othermoduleTabId > 0 && tabId.Value == ActiveModule.TabID)
                    {
                        //add extra li with "CurrentPage" directly to dropdown
                        //format = LogContext.IsLogActive ? "Current Page - {0} [{1}]" : "Current Page";
                        //listItems.Add(new PageDto()
                        //{
                        //    Text = string.Format(format, tabname, tabId.Value),
                        //    TabId = tabId.Value
                        //});
                        infoText.Add("Current");
                    }

                    format = LogContext.IsLogActive ? "{0} [{1}]" : "{0}";
                    if (othermoduleTabId > 0 && tabId.Value == ActiveModule.TabID)
                    {
                        listItems.Add(new PageDto()
                        {
                            Text = string.Format(format, tabname, tabId.Value) + " (Current)",
                            TabId = tabId.Value
                        });
                    }
                    else
                    {
                        listItems.Add(new PageDto()
                        {
                            Text = string.Format(format, tabname, tabId.Value) + (infoText.Any() ? " (" + string.Join(",", infoText.ToArray()) + ")" : ""),
                            TabId = tabId.Value
                        });
                    }

                }

                return listItems.OrderBy(x => x.Text).ToList();
            }
            return listItems;
        }
        private int GetOtherModuleDetailTabId(int othermoduleTabId, int dataModuleId)
        {
            //If tab<0 then the data does not come from an other module
            if (othermoduleTabId < 0) return 0;

            var moduleInfo = DnnUtils.GetDnnModule(othermoduleTabId, dataModuleId);
            if (moduleInfo == null)
            {
                //This should never happen
                App.Services.Logger.Error($"Module {dataModuleId} not found while in GetOtherModuleDetailTabId()");
                return 0;
            }

            var mainModuleSettings = moduleInfo.OpenContentSettings();

            if (mainModuleSettings == null) return 0;
            if (mainModuleSettings.TabId > -1) return 0; //the other module gets his data also from another module?! Let's not support that.

            return mainModuleSettings.DetailTabId == -1 ? moduleInfo.TabID : mainModuleSettings.DetailTabId;
        }
        private bool IsAccessibleTab(int tabId)
        {
            //ignore redirected tabs
            var tabinfo = TabController.Instance.GetTab(tabId, ActiveModule.PortalID);
            return tabinfo.IsPublishedTab();
        }

        private bool IsTabWithModuleWithSameMainModule(int tabId, int datamoduleId)
        {
            //only tabs with oc-module with main-moduleId= CurrentMainModuleId
            var tabinfo = TabController.Instance.GetTab(tabId, ActiveModule.PortalID);
            foreach (var item in tabinfo.ChildModules)
            {
                ModuleInfo moduleInfo = item.Value;
                if (moduleInfo.ModuleDefinition.FriendlyName == App.Config.Opencontent && !moduleInfo.IsDeleted)
                {
                    if (moduleInfo.OpenContentSettings().GetModuleId(moduleInfo.ModuleID) == datamoduleId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class PageDto
    {
        public int TabId { get; set; }
        public string Text { get; set; }
    }

    public class SaveDto
    {
        public string template { get; set; }
        public bool otherModule { get; set; }
        public int tabModuleId { get; set; }
        public bool newTemplate { get; set; }
        public bool fromWeb { get; set; }
        public string templateName { get; set; }
    }
}

public class ModuleStateDto
{
    public bool SettingsNeeded { get; set; }
    public bool SettingsDefined { get; set; }
    public bool DataNeeded { get; internal set; }
    public string Template { get; set; }
    public string Error { get; set; }
}

public class TemplateDto
{
    public string Text { get; set; }
    public string Value { get; set; }
}

public class PortalDto
{
    public string Text { get; set; }
    public int PortalId { get; set; }
}

public class ModuleDto
{
    public string Text { get; set; }
    public int TabModuleId { get; set; }
}
