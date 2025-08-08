using DotNetNuke.Services.Localization;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Render;
using Satrabel.OpenContent.Components.TemplateHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.UrlRewriter
{
    public class OpenContentUrlProvider
    {
        private static readonly object padlock = new object();
        public static List<OpenContentUrlRule> GetRules(int portalId)
        {
            
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
                if (portalRules != null && portalRules.Count > 0)
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

                //#if DEBUG
                //var cachedModules = 0;
                //var nonCached = 0;
                //#endif

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
                            if (moduleRules != null && moduleRules.Count > 0)
                            {
                                //App.Services.Logger.Error($"GetRules {portalId}/{module.TabId}/{module.ModuleId} count: {moduleRules.Count}");
                                rules.AddRange(moduleRules);
                                //#if DEBUG
                                //cachedModules += 1;
                                //#endif
                                continue;
                            }

                            //#if DEBUG
                            //nonCached += 1;
                            //#endif
                            moduleRules = new List<OpenContentUrlRule>();
                            IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                            GenerateCollectionRules(portalId, rules, dicLocales, module, dsContext, moduleRules, ds);
                            //var collections = new string[] { "Items", "Kunstenaars" };
                            //if (module.Settings.Manifest.Collections != null)
                            //{
                            //    foreach (string collectionKey in module.Settings.Manifest.Collections.Keys)
                            //    {
                            //        var collection = module.Settings.Manifest.Collections[collectionKey];
                            //        dsContext.Collection = collectionKey;
                            //        GenerateCollectionRules(portalId, rules, dicLocales, module, dsContext, moduleRules, ds, collection.DetailUrl);
                            //    }
                            //}
                            UrlRulesCaching.SetCache(portalId, UrlRulesCaching.GenerateModuleCacheKey(module.TabId, module.ModuleId, dsContext.ModuleId, null), new TimeSpan(1, 0, 0, 0), moduleRules);
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

        private static void GenerateCollectionRules(int portalId, List<OpenContentUrlRule> rules, Dictionary<string, Locale> dicLocales, OpenContentModuleConfig module, DataSourceContext dsContext, List<OpenContentUrlRule> moduleRules, IDataSource ds)
        {
            var dataList = ds.GetAll(dsContext, null).Items.ToList();
            if (dataList.Count() > 1000)
            {
                App.Services.Logger.Warn($"Module {module.DataModule.ModuleId} (portal/tab {module.DataModule.PortalId}/{module.DataModule.TabId}) has >1000 items. We are not making sluggs for them as this would be too inefficient");
                return;
            }
            HandlebarsEngine hbEngine = new HandlebarsEngine();

            var detailUrl = module.Settings.Manifest.DetailUrl;
            if (!string.IsNullOrEmpty(dsContext.Collection) && 
                module.Settings.Manifest.Collections != null &&
                module.Settings.Manifest.Collections.ContainsKey(dsContext.Collection)
                )
            {
                var collection = module.Settings.Manifest.Collections[dsContext.Collection];
                if (!string.IsNullOrEmpty(collection.DetailUrl))
                {
                    detailUrl = collection.DetailUrl;
                }
            }
            if (!string.IsNullOrEmpty(detailUrl))
            {
                hbEngine.Compile(detailUrl);
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
                    if (!string.IsNullOrEmpty(detailUrl))
                    {
                        try
                        {
                            url = hbEngine.Execute(content);
                            url = HttpUtility.HtmlDecode(url).StripHtml("-").CleanupUrl();
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
                            Url = url,
                            InSitemap = true
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
        }
    }
}