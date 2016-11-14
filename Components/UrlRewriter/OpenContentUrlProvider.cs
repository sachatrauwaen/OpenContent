using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Localization;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Handlebars;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.UrlRewriter
{
    public class OpenContentUrlProvider
    {
        public static List<OpenContentUrlRule> GetRules(int portalId)
        {
            Dictionary<string, Locale> dicLocales = LocaleController.Instance.GetLocales(portalId);
            List<OpenContentUrlRule> rules = new List<OpenContentUrlRule>();
            ModuleController mc = new ModuleController();
            ArrayList modules = mc.GetModulesByDefinition(portalId, AppConfig.OPENCONTENT);
            //foreach (ModuleInfo module in modules.OfType<ModuleInfo>().GroupBy(m => m.ModuleID).Select(g => g.First())){                
            foreach (ModuleInfo module in modules.OfType<ModuleInfo>())
            {
                try
                {
                    OpenContentSettings settings = new OpenContentSettings(module.ModuleSettings);
                    int mainTabId = settings.GetMainTabId(module.TabID);
                    int mainModuleId = settings.GetModuleId(module.ModuleID);
                    if (settings.IsListTemplate() && (!settings.IsOtherModule || settings.DetailTabId > 0))
                    {
                        var ds = DataSourceManager.GetDataSource(settings.Manifest.DataSource);
                        var dsContext = new DataSourceContext()
                        {
                            ModuleId = mainModuleId,
                            TemplateFolder = settings.TemplateDir.FolderPath,
                            PortalId = module.PortalID,
                            //CurrentCultureCode = DnnLanguageUtils.GetCurrentCultureCode(),   ==> todo: gives errors as PortalSettings is not available in context of scheduler
                            Config = settings.Manifest.DataSourceConfig,
                            Agent = "OpenContentUrlProvider.GetRules()"
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
                            string cultureCode = key.Value.Code;
                            string ruleCultureCode = (dicLocales.Count > 1 ? cultureCode : null);
                            ModelFactory mf = new ModelFactory(dataList, settings.Data, physicalTemplateFolder, settings.Template.Manifest, settings.Template, settings.Template.Main, module, portalId, cultureCode, mainTabId, mainModuleId);
                            //dynamic model = mf.GetModelAsDynamic(true);
                            //dynamic items = model.Items;
                            IEnumerable<dynamic> items = mf.GetModelAsDynamicList();
                            //Log.Logger.Debug("OCUR/" + PortalId + "/" + module.TabID + "/" + MainTabId + "/" + module.ModuleID + "/" + MainModuleId + "/" + CultureCode + "/" + dataList.Count() + "/" + module.ModuleTitle);
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
                                        url = HttpUtility.HtmlDecode(url);
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
                                        CultureCode = ruleCultureCode,
                                        TabId = mainTabId,
                                        Parameters = "id=" + id,
                                        Url = url
                                    };
                                    var reducedRules = rules.Where(r => r.CultureCode == rule.CultureCode && r.TabId == rule.TabId);
                                    bool ruleExist = reducedRules.Any(r => r.Parameters == rule.Parameters);
                                    if (!ruleExist)
                                    {
                                        if (reducedRules.Any(r => r.Url == rule.Url))
                                        {
                                            rule.Url = id + "-" + url;
                                        }
                                        rules.Add(rule);
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
            return rules;
        }
    }
}