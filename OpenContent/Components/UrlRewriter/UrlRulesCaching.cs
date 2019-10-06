using DotNetNuke.Collections.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Satrabel.OpenContent.Components.UrlRewriter
{
    public static class UrlRulesCaching
    {
        #region Private Members

        private const string UrlRuleConfigCacheKey = "UrlRuleConfig{0}";
        private const string DataFileExtension = ".data.resources";
        private const string AttribFileExtension = ".attrib.resources";
        private static readonly SharedDictionary<int, string> CacheFolderPath = new SharedDictionary<int, string>(LockingStrategy.ReaderWriter);

        #endregion

        #region Private Methods

        private static string GenerateModuleCacheKeyHash(int tabId, int viewModuleId, int dataModuleId, string cacheKey)
        {
            byte[] hash = Encoding.ASCII.GetBytes(cacheKey);
            var sha256 = new SHA256CryptoServiceProvider();
            hash = sha256.ComputeHash(hash);
            return viewModuleId + "_" + dataModuleId + "_" + tabId + "_" + ByteArrayToString(hash);
        }

        private static string GeneratePortalCacheKeyHash(int portalId, string cacheKey)
        {
            byte[] hash = Encoding.ASCII.GetBytes(cacheKey);
            var sha256 = new SHA256CryptoServiceProvider();
            hash = sha256.ComputeHash(hash);
            return portalId + "_" + ByteArrayToString(hash);
        }

        private static string ByteArrayToString(byte[] arrInput)
        {
            int i;
            var sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i <= arrInput.Length - 1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
        private static string GetAttribFileName(int portalId, string cacheKey)
        {
            return string.Concat(GetCacheFolder(portalId), cacheKey, AttribFileExtension);
        }

        private static int GetCachedItemCount(int portalId)
        {
            return Directory.GetFiles(GetCacheFolder(portalId), $"*{DataFileExtension}").Length;
        }

        private static string GetCachedOutputFileName(int portalId, string cacheKey)
        {
            return string.Concat(GetCacheFolder(portalId), cacheKey, DataFileExtension);
        }

        /// <summary>
        /// [jmarino]  2011-06-16 Check for ContainsKey for a write added
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns></returns>
        private static string GetCacheFolder(int portalId)
        {
            string cacheFolder;

            using (var readerLock = CacheFolderPath.GetReadLock())
            {
                if (CacheFolderPath.TryGetValue(portalId, out cacheFolder))
                {
                    return cacheFolder;
                }
            }
            var portalController = new PortalController();
            PortalInfo portalInfo = portalController.GetPortal(portalId);
            string homeDirectoryMapPath = portalInfo.HomeSystemDirectoryMapPath;
            if (!(string.IsNullOrEmpty(homeDirectoryMapPath)))
            {
                cacheFolder = string.Concat(homeDirectoryMapPath, "Cache\\OpenContentUrlRules\\");
                if (!(Directory.Exists(cacheFolder)))
                {
                    Directory.CreateDirectory(cacheFolder);
                }
            }

            using (var writerLock = CacheFolderPath.GetWriteLock())
            {
                if (!CacheFolderPath.ContainsKey(portalId))
                    CacheFolderPath.Add(portalId, cacheFolder);
            }
            return cacheFolder;
        }
        private static bool IsFileExpired(string file)
        {
            StreamReader oRead = null;
            try
            {
                oRead = File.OpenText(file);
                DateTime expires = DateTime.Parse(oRead.ReadLine(), CultureInfo.InvariantCulture);
                if (expires < DateTime.UtcNow)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                //if check expire time failed, then force to expire the cache.
                return true;
            }
            finally
            {
                oRead?.Close();
            }
        }

        private static void PurgeCache(string folder)
        {
            var filesNotDeleted = new StringBuilder();
            int i = 0;
            foreach (string file in Directory.GetFiles(folder, "*.resources"))
            {
                if (!FileSystemUtils.DeleteFileWithWait(file, 100, 200))
                {
                    filesNotDeleted.Append($"{file};");
                }
                else
                {
                    i += 1;
                }
            }
            if (filesNotDeleted.Length > 0)
            {
                throw new IOException($"Deleted {i} files, however, some files are locked.  Could not delete the following files: {filesNotDeleted}");
            }
        }

        private static bool IsPathInApplication(string cacheFolder)
        {
            return cacheFolder.Contains(App.Config.ApplicationMapPath);
        }

        #endregion

        #region Abstract Method Implementation

        public static string GenerateModuleCacheKey(int tabId, int viewModuleId, int dataModuleId, SortedDictionary<string, string> varyBy)
        {
            var cacheKey = new StringBuilder();
            if (varyBy != null)
            {
                SortedDictionary<string, string>.Enumerator varyByParms = varyBy.GetEnumerator();
                while ((varyByParms.MoveNext()))
                {
                    string key = varyByParms.Current.Key.ToLower();
                    cacheKey.Append(string.Concat(key, "=", varyByParms.Current.Value, "|"));
                }

                varyByParms.Dispose();
            }
            return GenerateModuleCacheKeyHash(tabId, viewModuleId, dataModuleId, cacheKey.ToString());
        }

        public static string GeneratePortalCacheKey(int portalId, SortedDictionary<string, string> varyBy)
        {
            var cacheKey = new StringBuilder();
            if (varyBy != null)
            {
                SortedDictionary<string, string>.Enumerator varyByParms = varyBy.GetEnumerator();
                while ((varyByParms.MoveNext()))
                {
                    string key = varyByParms.Current.Key.ToLower();
                    cacheKey.Append(string.Concat(key, "=", varyByParms.Current.Value, "|"));
                }

                varyByParms.Dispose();
            }
            return GeneratePortalCacheKeyHash(portalId, cacheKey.ToString());
        }

        public static List<OpenContentUrlRule> GetCache(int portalId, string cacheKey)
        {

            string cacheFileName = GetCachedOutputFileName(portalId, cacheKey);
            try
            {
                if (!File.Exists(cacheFileName))
                {
                    return null;
                }
                if (IsFileExpired(GetAttribFileName(portalId, cacheKey)))
                {
                    return null;
                }
                return JsonConvert.DeserializeObject<List<OpenContentUrlRule>>(File.ReadAllText(cacheFileName));
            }
            catch
            {
                return null;
            }
        }

        public static List<OpenContentUrlRule> GetCache(int portalId, string cacheKey, List<string> validCacheItems)
        {
            string dataCacheFileName = GetCachedOutputFileName(portalId, cacheKey);
            try
            {
                if (!validCacheItems.Contains(dataCacheFileName))
                {
                    return null;
                }
                return JsonConvert.DeserializeObject<List<OpenContentUrlRule>>(File.ReadAllText(dataCacheFileName));
            }
            catch
            {
                return null;
            }
        }

        public static void PurgeCache(int portalId)
        {
            PurgeCache(GetCacheFolder(portalId));
        }

        public static PurgeResult PurgeExpiredItems(int portalId)
        {
            var filesNotDeleted = new StringBuilder();
            int purgedCount = 0;
            string cacheFolder = GetCacheFolder(portalId);
            var validCacheItems = new List<string>();

            if (Directory.Exists(cacheFolder) && IsPathInApplication(cacheFolder))
            {
                foreach (string attribCacheFileName in Directory.GetFiles(cacheFolder, $"*{AttribFileExtension}"))
                {
                    string dataCacheFileName = attribCacheFileName.Replace(AttribFileExtension, DataFileExtension);
                    if (IsFileExpired(attribCacheFileName))
                    {
                        if (!FileSystemUtils.DeleteFileWithWait(dataCacheFileName, 100, 200))
                        {
                            filesNotDeleted.Append($"{dataCacheFileName};");
                        }
                        else
                        {
                            purgedCount += 1;
                        }
                    }
                    else
                    {
                        validCacheItems.Add(dataCacheFileName);
                    }
                }
            }
            if (filesNotDeleted.Length > 0)
            {
                throw new IOException($"Deleted {purgedCount} files, however, some files are locked.  Could not delete the following files: {filesNotDeleted}");
            }
            var pr = new PurgeResult(purgedCount, validCacheItems, filesNotDeleted);
            return pr;
        }

        public static void SetCache(int portalId, string cacheKey, TimeSpan duration, List<OpenContentUrlRule> rules)
        {
            try
            {
                string cachedOutputFile = GetCachedOutputFileName(portalId, cacheKey);
                if (File.Exists(cachedOutputFile))
                {
                    FileSystemUtils.DeleteFileWithWait(cachedOutputFile, 100, 200);
                }

                string attribFile = GetAttribFileName(portalId, cacheKey);
                File.WriteAllText(cachedOutputFile, JsonConvert.SerializeObject(rules));
                File.WriteAllLines(attribFile, new[] { DateTime.UtcNow.Add(duration).ToString(CultureInfo.InvariantCulture) });
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

        public static void Remove(int portalId, int dataModuleId)
        {
            string cacheFolder = GetCacheFolder(portalId);
            var filesNotDeleted = new StringBuilder();
            int i = 0;
            foreach (string file in Directory.GetFiles(cacheFolder, "*_" + dataModuleId + "_*.*"))
            {
                if (!FileSystemUtils.DeleteFileWithWait(file, 100, 200))
                {
                    filesNotDeleted.Append(file + ";");
                }
                else
                {
                    i += 1;
                }
            }

            var portalCacheKey = UrlRulesCaching.GeneratePortalCacheKey(portalId, null);
            string dataCacheFileName = GetCachedOutputFileName(portalId, portalCacheKey);
            if (!FileSystemUtils.DeleteFileWithWait(dataCacheFileName, 100, 200))
            {
                filesNotDeleted.Append(dataCacheFileName + ";");
            }
            else
            {
                i += 1;
            }

            if (filesNotDeleted.Length > 0)
            {
                throw new IOException("Deleted " + i + " files, however, some files are locked.  Could not delete the following files: " + filesNotDeleted);
            }

            DataCache.ClearCache(string.Format(UrlRuleConfigCacheKey, portalId));
        }

        #endregion


    }

    public class PurgeResult
    {
        public PurgeResult(int i, List<string> validCacheItems, StringBuilder filesNotDeleted)
        {
            this.PurgedItemCount = i;
            this.ValidCacheItems = validCacheItems;
            this.FilesNotDeleted = filesNotDeleted;
        }

        public int PurgedItemCount { get; }
        public List<string> ValidCacheItems { get; }
        public StringBuilder FilesNotDeleted { get; }
    }
}