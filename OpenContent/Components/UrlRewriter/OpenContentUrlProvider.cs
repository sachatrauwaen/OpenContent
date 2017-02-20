using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Localization;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Render;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Satrabel.OpenContent.Components.TemplateHelpers;

namespace Satrabel.OpenContent.Components.UrlRewriter
{
    public class OpenContentUrlProvider
    {
        public static List<OpenContentUrlRule> GetRules(int portalId)
        {
#if DEBUG
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
#endif
            var purgeResult = UrlRulesCaching.PurgeExpiredItems(portalId);

            
            List<OpenContentUrlRule> rules = new List<OpenContentUrlRule>();

            var portalCacheKey = UrlRulesCaching.GeneratePortalCacheKey(portalId, null);
            var portalRules = UrlRulesCaching.GetCache(portalId, portalCacheKey, purgeResult.ValidCacheItems);
            if (portalRules != null)
            {
                return portalRules;
            }

            Dictionary<string, Locale> dicLocales = LocaleController.Instance.GetLocales(portalId);
            var modules = DnnUtils.GetDnnOpenContentModules(portalId).ToList();

            var cachedModules = 0;
            var nonCached = 0;

            foreach (var module in modules)
            {
                var cacheKey = UrlRulesCaching.GenerateModuleCacheKey(module.TabId, module.ModuleId, null);
                List<OpenContentUrlRule> moduleRules = UrlRulesCaching.GetCache(portalId, cacheKey, purgeResult.ValidCacheItems);
                if (moduleRules != null)
                {
                    rules.AddRange(moduleRules);
                    cachedModules += 1;
                    continue;
                }
                try
                {
                    if (module.IsListMode() && module.Settings.Template.Detail != null &&
                            ((!module.Settings.IsOtherModule && module.Settings.DetailTabId <= 0) ||
                                (module.Settings.DetailTabId == module.TabId)
                            )
                        )
                    {
                        nonCached += 1;

                        moduleRules = new List<OpenContentUrlRule>();
                        IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                        var dsContext = OpenContentUtils.CreateDataContext(module);
                        dsContext.Agent = "OpenContentUrlProvider.GetRules()";

                        var dataList = ds.GetAll(dsContext, null).Items.ToList();
                        if (dataList.Count() > 1000)
                        {
                            Log.Logger.Warn($"Module {module.DataModule.ModuleID} (portal/tab {module.DataModule.PortalID}/{module.DataModule.TabID}) has >1000 items. We are not making sluggs for them as this would be too inefficient");
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
                            ModelFactoryMultiple mf = new ModelFactoryMultiple(dataList, module.Settings.Data, physicalTemplateFolder, module.Settings.Template.Manifest, module.Settings.Template, module.Settings.Template.Main, module, portalId, cultureCode);
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
                                        url = HttpUtility.HtmlDecode(url).CleanupUrl();
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
                                    var reducedRules = rules.Where(r => r.CultureCode == rule.CultureCode && r.TabId == rule.TabId).ToList();
                                    bool ruleExist = reducedRules.Any(r => r.Parameters == rule.Parameters);
                                    if (!ruleExist)
                                    {
                                        if (reducedRules.Any(r => r.Url == rule.Url))
                                        {
                                            rule.Url = id + "-" + url;
                                        }
                                        rules.Add(rule);
                                        moduleRules.Add(rule);
                                    }
                                }
                            }
                        }
                        UrlRulesCaching.SetCache(portalId,  UrlRulesCaching.GenerateModuleCacheKey(module.TabId, module.ModuleId, null), new TimeSpan(1, 0, 0, 0), moduleRules);
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("Failed to generate url for opencontent module " + module.ViewModule.ModuleID, ex);
                }

            }
            UrlRulesCaching.SetCache(portalId, portalCacheKey, new TimeSpan(1, 0, 0, 0), rules);
#if DEBUG
            stopwatch.Stop();
            decimal speed = (cachedModules + nonCached) == 0 ? -1 : stopwatch.Elapsed.Milliseconds / (cachedModules + nonCached);
            var mess = $"PortalId: {portalId}. Time elapsed: {stopwatch.Elapsed.Milliseconds}ms. Module Count: {modules.Count()}. Relevant Modules: {cachedModules + nonCached}. CachedModules: {cachedModules}. PurgedItems: {purgeResult.PurgedItemCount}. Speed: {speed}";
            Log.Logger.Error(mess);
            Console.WriteLine(mess);
#endif
            return rules;
        }
    }
}