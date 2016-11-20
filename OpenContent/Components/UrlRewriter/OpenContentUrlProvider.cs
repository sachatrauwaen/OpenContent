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
            var modules = DnnUtils.GetDnnOpenContentModules(portalId);

            foreach (var module in modules)
            {
                try
                {
                    if (module.IsListTemplate() && (!module.Settings.IsOtherModule || module.Settings.DetailTabId > 0))
                    {
                        IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                        var dsContext = OpenContentUtils.CreateDataContext(module);
                        dsContext.Agent = "OpenContentUrlProvider.GetRules()";

                        var dataList = ds.GetAll(dsContext, null).Items;
                        if (dataList.Count() > 1000)
                        {
                            continue;
                        }
                        var physicalTemplateFolder = module.Settings.TemplateDir.PhysicalFullDirectory + "\\";
                        HandlebarsEngine hbEngine = new HandlebarsEngine();
                        if (!string.IsNullOrEmpty(module.Settings.Manifest.DetailUrl))
                        {
                            hbEngine.Compile(module.Settings.Manifest.DetailUrl);
                        }
                        foreach (KeyValuePair<string, Locale> key in dicLocales)
                        {
                            string cultureCode = key.Value.Code;
                            string ruleCultureCode = (dicLocales.Count > 1 ? cultureCode : null);
                            ModelFactory mf = new ModelFactory(dataList, module.Settings.Data, physicalTemplateFolder, module.Settings.Template.Manifest, module.Settings.Template, module.Settings.Template.Main, module, portalId, cultureCode);
                            //dynamic model = mf.GetModelAsDynamic(true);
                            //dynamic items = model.Items;
                            IEnumerable<dynamic> items = mf.GetModelAsDynamicList();
                            //Log.Logger.Debug("OCUR/" + PortalId + "/" + module.TabID + "/" + MainTabId + "/" + module.ModuleID + "/" + MainModuleId + "/" + CultureCode + "/" + dataList.Count() + "/" + module.ModuleTitle);
                            //foreach (IDataItem content in dataList)
                            foreach (dynamic content in items)
                            {
                                string id = content.Context.Id;
                                string url = "content-" + id;
                                if (!string.IsNullOrEmpty(module.Settings.Manifest.DetailUrl))
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
                                        TabId = module.GetDetailTabId(),
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
                    Log.Logger.Error("Failed to generate url for opencontent module " + module.ViewModule.ModuleID, ex);
                }
            }
            return rules;
        }
    }
}