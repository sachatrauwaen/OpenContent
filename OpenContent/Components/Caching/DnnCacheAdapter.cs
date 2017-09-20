using System;
using System.Collections;
using System.Collections.Generic;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.ModuleCache;
using DotNetNuke.Services.OutputCache;

namespace Satrabel.OpenContent.Components
{
    public class DnnCacheAdapter : ICacheAdapter
    {
        public T GetCache<T>(string cacheKey)
        {
            return (T)DataCache.GetCache(cacheKey);
        }

        public void SetCache(string cacheKey, object objectToCache, string dependentFile)
        {
            DataCache.SetCache(cacheKey, objectToCache, new DNNCacheDependency(dependentFile));
        }

        public void SetCache(string cacheKey, object objectToCache, string[] dependentFiles)
        {
            DataCache.SetCache(cacheKey, objectToCache, new DNNCacheDependency(dependentFiles));
        }

        public T GetCachedData<T>(string key, int cacheTimeInMinutes, Func<object, object> func)
        {
            var cacheArgs = new CacheItemArgs(key, cacheTimeInMinutes);
            var retval = DataCache.GetCachedData<T>(cacheArgs, func.Invoke);
            return retval;
        }

        public void ClearCache(string cacheKey)
        {
            DataCache.ClearCache(cacheKey);
        }

        public void SyncronizeCache(OpenContentModuleConfig ocModuleConfig)
        {
            int dataModuleId = ocModuleConfig.DataModule.ModuleId;
            var dataModuleHasCrossPortalData = Json.JsonExtensions.GetValue(ocModuleConfig.DataModule.ModuleInfo.OpenContentSettings().Manifest.Permissions, "AllowCrossPortalData", false);
            if (dataModuleHasCrossPortalData)
                foreach (PortalInfo portal in PortalController.Instance.GetPortals())
                {
                    SyncronizeLinkedModules(portal.PortalID, dataModuleId);
                }
            else
            {
                SyncronizeLinkedModules(PortalSettings.Current.PortalId, dataModuleId);
            }
        }

        private static void SyncronizeLinkedModules(int portalId, int dataModuleId)
        {
            var ocModules = DnnUtils.GetDnnOpenContentModules(portalId);
            foreach (var ocModule in ocModules)
            {
                if (ocModule.DataModule.ModuleId == dataModuleId)
                    SynchronizeModule(ocModule.ViewModule.ModuleId);
            }
        }

        /// <summary>
        /// Synchronizes the cache.
        /// </summary>
        /// <param name="moduleId">The module identifier.</param>
        /// <param name="currentPortal"></param>
        /// <remarks>
        /// The original code comes from DNN, SynchronizeModule(int moduleID)
        /// But we modified it to be more efficient
        /// </remarks>
        private static void SynchronizeModule(int moduleId)
        {
            DataProvider dataProvider = DataProvider.Instance();

            IList<ModuleInfo> modules = ModuleController.Instance.GetTabModulesByModule(moduleId);
            foreach (ModuleInfo module in modules)
            {
                Hashtable tabSettings = TabController.Instance.GetTabSettings(module.TabID);
                if (tabSettings["CacheProvider"] != null && tabSettings["CacheProvider"].ToString().Length > 0)
                {
                    var outputProvider = OutputCachingProvider.Instance(tabSettings["CacheProvider"].ToString());
                    outputProvider?.Remove(module.TabID);
                }

                if (module.CacheTime > 0)
                {
                    var cachingProvider = ModuleCachingProvider.Instance(module.GetEffectiveCacheMethod());
                    cachingProvider?.Remove(module.TabModuleID);
                }

                //Synchronize module is called when a module needs to indicate that the content
                //has changed and the cache's should be refreshed.  So we can update the Version
                //and also the LastContentModificationDate
                dataProvider.UpdateTabModuleVersion(module.TabModuleID, Guid.NewGuid());
                dataProvider.UpdateModuleLastContentModifiedOnDate(module.ModuleID);

                ////We should also indicate that the Transalation Status has changed
                //if (PortalController.GetPortalSettingAsBoolean("ContentLocalizationEnabled", module.PortalID, false))
                //{
                //    ModuleController.Instance.UpdateTranslationStatus(module, false);
                //}


                // and clear the cache
                ModuleController.Instance.ClearCache(module.TabID);
            }
        }
    }
}