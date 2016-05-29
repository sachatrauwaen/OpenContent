using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Localization;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Handlebars;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Satrabel.OpenContent.Components.UrlRewriter
{
    public class OpenContentUrlProvider
    {
        public static List<OpenContentUrlRule> GetRules(int PortalId)
        {
            Dictionary<string, Locale> dicLocales = LocaleController.Instance.GetLocales(PortalId);
            List<OpenContentUrlRule> Rules = new List<OpenContentUrlRule>();
            OpenContentController occ = new OpenContentController();
            ModuleController mc = new ModuleController();
            ArrayList modules = mc.GetModulesByDefinition(PortalId, "OpenContent");
            //foreach (ModuleInfo module in modules.OfType<ModuleInfo>().GroupBy(m => m.ModuleID).Select(g => g.First())){                
            foreach (ModuleInfo module in modules.OfType<ModuleInfo>())
            {
                try
                {
                    OpenContentSettings settings = new OpenContentSettings(module.ModuleSettings);
                    int MainTabId = settings.DetailTabId > 0 ? settings.DetailTabId : (settings.TabId > 0 ? settings.TabId : module.TabID);
                    int MainModuleId = settings.IsOtherModule ? settings.ModuleId : module.ModuleID;
                    if (settings.Template != null && settings.Template.IsListTemplate && (!settings.IsOtherModule || settings.DetailTabId > 0))
                    {
                        var ds = DataSourceManager.GetDataSource(settings.Manifest.DataSource);
                        var dsContext = new DataSourceContext()
                        {
                            ModuleId = MainModuleId,
                            TemplateFolder = settings.TemplateDir.FolderPath,
                            Config = settings.Manifest.DataSourceConfig
                        };
                        IEnumerable<IDataItem> dataList = new List<IDataItem>();
                        dataList = ds.GetAll(dsContext, null).Items;
                        if (dataList.Count() > 1000)
                        {
                            continue;
                        }
                        var physicalTemplateFolder = settings.TemplateDir.PhysicalFullDirectory + "\\";
                        HandlebarsEngine hbEngine = new HandlebarsEngine();
                        if (!string.IsNullOrEmpty(settings.Manifest.DetailUrl))
                        {
                            hbEngine.Compile(settings.Manifest.DetailUrl);
                        }
                        foreach (KeyValuePair<string, Locale> key in dicLocales)
                        {
                            string CultureCode = key.Value.Code;
                            string RuleCultureCode = (dicLocales.Count > 1 ? CultureCode : null);
                            ModelFactory mf = new ModelFactory(dataList, settings.Data, physicalTemplateFolder, settings.Template.Manifest, settings.Template, settings.Template.Main, module, PortalId, CultureCode, MainTabId, MainModuleId);
                            //dynamic model = mf.GetModelAsDynamic(true);
                            //dynamic items = model.Items;
                            IEnumerable<dynamic> items = mf.GetModelAsDynamicList();
                            Log.Logger.Error("OCUR/" + module.TabID + "/" + MainTabId + "/" + module.ModuleID + "/" + MainModuleId + "/" + CultureCode + "/" + dataList.Count() + "/" + module.ModuleTitle);
                            //foreach (IDataItem content in dataList)
                            foreach (dynamic content in items)
                            {
                                string id = content.Context.Id;
                                string url = "content-" + id;
                                if (!string.IsNullOrEmpty(settings.Manifest.DetailUrl))
                                {
                                    try
                                    {
                                        //ModelFactory mf = new ModelFactory(content, settings.Data, physicalTemplateFolder, settings.Template.Manifest, settings.Template, settings.Template.Main, module, PortalId, CultureCode, MainTabId, MainModuleId);
                                        //dynamic model = mf.GetModelAsDynamic(true);
                                        url = hbEngine.Execute(content);
                                        //title = OpenContentUtils.CleanupUrl(dyn.Title);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Logger.Error("Failed to generate url for opencontent item " + content.Id, ex);
                                    }
                                }

                                if (!string.IsNullOrEmpty(url))
                                {
                                    var rule = new OpenContentUrlRule
                                    {
                                        CultureCode = RuleCultureCode,
                                        TabId = MainTabId,
                                        Parameters = "id=" + id,
                                        Url = url
                                    };
                                    var reducedRules = Rules.Where(r => r.CultureCode == rule.CultureCode && r.TabId == rule.TabId);
                                    bool RuleExist = reducedRules.Any(r => r.Parameters == rule.Parameters);
                                    if (!RuleExist)
                                    {
                                        if (reducedRules.Any(r => r.Url == rule.Url))
                                        {
                                            rule.Url = id + "-" + url;
                                        }
                                        Rules.Add(rule);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("Failed to generate url for opencontent module " + module.ModuleID, ex);
                }
            }
            return Rules;
        }
    }
}