using DotNetNuke.Services.Localization;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Render;
using System;
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
            object padlock = new object();

            lock (padlock)
            {
                List<OpenContentUrlRule> rules = new List<OpenContentUrlRule>();
                //#if DEBUG
                //                decimal speed;
                //                string mess;
                //                var stopwatch = new System.Diagnostics.Stopwatch();
                //                stopwatch.Start();
                //#endif
                var purgeResult = UrlRulesCaching.PurgeExpiredItems(portalId);

                var portalCacheKey = UrlRulesCaching.GeneratePortalCacheKey(portalId, null);
                var portalRules = UrlRulesCaching.GetCache(portalId, portalCacheKey, purgeResult.ValidCacheItems);
                if (portalRules != null)
                {
                    //#if DEBUG
                    //   App.Services.Logger.Debug($"GetRules {portalId} CachedRuleCount: {portalRules.Count}");
                    //   stopwatch.Stop();
                    //   speed = stopwatch.Elapsed.Milliseconds;
                    //   mess = $"PortalId: {portalId}. Time elapsed: {stopwatch.Elapsed.Milliseconds}ms. All Cached. PurgedItems: {purgeResult.PurgedItemCount}. Speed: {speed}";
                    //   App.Services.Logger.Error(mess);
                    //#endif
                    return portalRules;
                }

                Dictionary<string, Locale> dicLocales = LocaleController.Instance.GetLocales(portalId);
                var modules = DnnUtils.GetDnnOpenContentModules(portalId).ToList();

                var cachedModules = 0;
                var nonCached = 0;

                foreach (var module in modules)
                {
                    try
                    {
                        //Urls are generated for every detailmodule of a list-template
                        var isDetailTemplate = (module.Settings.DetailTabId == module.TabId) || (module.Settings.DetailTabId <= 0 && !module.Settings.IsOtherModule);
                        var partofListTemplateWithDetailTemplate = module.IsListMode() && module.Settings.Template.Detail != null;

                        if (isDetailTemplate && partofListTemplateWithDetailTemplate)
                        {
                            var dsContext = OpenContentUtils.CreateDataContext(module);
                            dsContext.Agent = "OpenContentUrlProvider.GetRules()";

                            var cacheKey = UrlRulesCaching.GenerateModuleCacheKey(module.TabId, module.ModuleId, dsContext.ModuleId, null);
                            List<OpenContentUrlRule> moduleRules = UrlRulesCaching.GetCache(portalId, cacheKey, purgeResult.ValidCacheItems);
                            if (moduleRules != null)
                            {
                                //App.Services.Logger.Error($"GetRules {portalId}/{module.TabId}/{module.ModuleId} count: {moduleRules.Count}");
                                rules.AddRange(moduleRules);
                                cachedModules += 1;
                                continue;
                            }

                            //App.Services.Logger.Error($"GetRules {portalId}/{module.TabId}/{module.ModuleId} start processing");

                            nonCached += 1;

                            moduleRules = new List<OpenContentUrlRule>();
                            IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);

                            var dataList = ds.GetAll(dsContext, null).Items.ToList();
                            if (dataList.Count() > 1000)
                            {
                                App.Services.Logger.Warn($"Module {module.DataModule.ModuleId} (portal/tab {module.DataModule.PortalId}/{module.DataModule.TabId}) has >1000 items. We are not making sluggs for them as this would be too inefficient");
                                continue;
                            }
                            HandlebarsEngine hbEngine = new HandlebarsEngine();
                            if (!string.IsNullOrEmpty(module.Settings.Manifest.DetailUrl))
                            {
                                hbEngine.Compile(module.Settings.Manifest.DetailUrl);
                            }
                            foreach (KeyValuePair<string, Locale> key in dicLocales)
                            {
                                string cultureCode = key.Value.Code;
                                string ruleCultureCode = (dicLocales.Count > 1 ? cultureCode : null);
                                ModelFactoryMultiple mf = new ModelFactoryMultiple(dataList, module.Settings.Data, module.Settings.Template.Manifest, module.Settings.Template, module.Settings.Template.Main, module, portalId, cultureCode);
                                IEnumerable<Dictionary<string, object>> items = mf.GetModelAsDictionaryList();
                                //App.Services.Logger.Debug("OCUR/" + PortalId + "/" + module.TabID + "/" + MainTabId + "/" + module.ModuleID + "/" + MainModuleId + "/" + CultureCode + "/" + dataList.Count() + "/" + module.ModuleTitle);
                                foreach (Dictionary<string, object> content in items)
                                {
                                    string id = (content["Context"] as Dictionary<string, object>)["Id"].ToString();
                                    string url = "content-" + id;
                                    if (!string.IsNullOrEmpty(module.Settings.Manifest.DetailUrl))
                                    {
                                        try
                                        {
                                            url = hbEngine.Execute(content);
                                            url = HttpUtility.HtmlDecode(url).CleanupUrl();
                                        }
                                        catch (Exception ex)
                                        {
                                            App.Services.Logger.Error("Failed to generate url for opencontent item " + id, ex);
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
                            UrlRulesCaching.SetCache(portalId, UrlRulesCaching.GenerateModuleCacheKey(module.TabId, module.ModuleId, dsContext.ModuleId,  null), new TimeSpan(1, 0, 0, 0), moduleRules);
                            //App.Services.Logger.Error($"GetRules {portalId}/{module.TabId}/{module.ModuleId} NewCount: {moduleRules.Count}");
                        }
                    }
                    catch (Exception ex)
                    {
                        App.Services.Logger.Error("Failed to generate url for opencontent module " + module.ViewModule.ModuleId, ex);
                    }

                }
                UrlRulesCaching.SetCache(portalId, portalCacheKey, new TimeSpan(1, 0, 0, 0), rules);
                //#if DEBUG
                //                stopwatch.Stop();
                //                speed = (cachedModules + nonCached) == 0 ? -1 : stopwatch.Elapsed.Milliseconds / (cachedModules + nonCached);
                //                mess = $"PortalId: {portalId}. Time elapsed: {stopwatch.Elapsed.Milliseconds}ms. Module Count: {modules.Count()}. Relevant Modules: {cachedModules + nonCached}. CachedModules: {cachedModules}. PurgedItems: {purgeResult.PurgedItemCount}. Speed: {speed}";
                //                App.Services.Logger.Error(mess);
                //                Console.WriteLine(mess);
                //#endif
                return rules;
            }
        }
    }
}