/*
' Copyright (c) 2015-2016 Satrabel.be
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System.Linq;
using System.Collections.Generic;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using Satrabel.OpenContent.Components.Common;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentController
    {
        private const string CACHE_PREFIX = "Satrabel.OpenContent.Components.OpenContentController-";
        private const int CACHE_TIME = 60;

        #region Commands

        public void AddContent(OpenContentInfo content)
        {
            ClearDataCache(content);
            var json = content.JsonAsJToken;
            if (string.IsNullOrEmpty(content.Key))
            {
                content.Key = json["_id"]?.ToString() ?? ObjectId.NewObjectId().ToString();
            }
            if (string.IsNullOrEmpty(content.Collection))
            {
                content.Collection = App.Config.DefaultCollection;
            }
            OpenContentVersion ver = new OpenContentVersion()
            {
                Json = json,
                CreatedByUserId = content.CreatedByUserId,
                CreatedOnDate = content.CreatedOnDate,
                LastModifiedByUserId = content.LastModifiedByUserId,
                LastModifiedOnDate = content.LastModifiedOnDate
            };
            var versions = new List<OpenContentVersion>();
            versions.Add(ver);
            content.Versions = versions;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                rep.Insert(content);
                ModuleController.SynchronizeModule(content.ModuleId);
            }
        }

        public void DeleteContent(OpenContentInfo content)
        {
            ClearDataCache(content);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                rep.Delete(content);
                ModuleController.SynchronizeModule(content.ModuleId);
            }
        }

        public void UpdateContent(OpenContentInfo content)
        {
            ClearDataCache(content);
            var json = content.JsonAsJToken;
            OpenContentVersion ver = new OpenContentVersion()
            {
                Json = json,
                CreatedByUserId = content.CreatedByUserId,
                CreatedOnDate = content.CreatedOnDate,
                LastModifiedByUserId = content.LastModifiedByUserId,
                LastModifiedOnDate = content.LastModifiedOnDate
            };
            var versions = content.Versions;
            if (versions.Count == 0 || versions[0].Json.ToString() != content.Json)
            {
                versions.Insert(0, ver);
                if (versions.Count > App.Services.GlobalSettings().GetMaxVersions())
                {
                    versions.RemoveAt(versions.Count - 1);
                }
                content.Versions = versions;
            }
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                rep.Update(content);
                ModuleController.SynchronizeModule(content.ModuleId);
            }
        }

        #endregion

        #region Queries

        internal IEnumerable<OpenContentInfo> GetContents(int moduleId)
        {
            var cacheArgs = new CacheItemArgs(GetModuleIdCacheKey(moduleId, "GetContents"), CACHE_TIME);
            return DataCache.GetCachedData<IEnumerable<OpenContentInfo>>(cacheArgs, args =>
                {
                    IEnumerable<OpenContentInfo> content;

                    using (IDataContext ctx = DataContext.Instance())
                    {
                        var rep = ctx.GetRepository<OpenContentInfo>();
                        content = rep.Get(moduleId);
                    }
                    return content;
                });
        }

        public OpenContentInfo GetContent(int contentId)
        {
            var cacheArgs = new CacheItemArgs(GetContentIdCacheKey(contentId), CACHE_TIME);
            return DataCache.GetCachedData<OpenContentInfo>(cacheArgs, args =>
                {
                    OpenContentInfo content;

                    using (IDataContext ctx = DataContext.Instance())
                    {
                        var rep = ctx.GetRepository<OpenContentInfo>();
                        content = rep.GetById(contentId);
                    }
                    return content;
                });
        }

        public OpenContentInfo GetFirstContent(int moduleId)
        {
            var cacheArgs = new CacheItemArgs(GetModuleIdCacheKey(moduleId) + "GetFirstContent", CACHE_TIME);
            return DataCache.GetCachedData<OpenContentInfo>(cacheArgs, args =>
                {
                    OpenContentInfo content;

                    using (IDataContext ctx = DataContext.Instance())
                    {
                        var rep = ctx.GetRepository<OpenContentInfo>();
                        content = rep.Get(moduleId).FirstOrDefault();
                    }
                    return content;
                });
        }

        public OpenContentInfo GetContent(int moduleId, string collection, string id)
        {
            if (collection == App.Config.DefaultCollection)
            {
                int intid = 0;
                if (int.TryParse(id, out intid))
                    return GetContent(intid);
                else
                    return null;
            }
            else
            {
                return GetContentByKey(moduleId, collection, id);
            }
        }
        public OpenContentInfo GetContentByKey(int moduleId, string collection, string key)
        {
            IEnumerable<OpenContentInfo> documents;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                documents = rep.Find("WHERE ModuleId = @0 AND Collection = @1 AND DocumentKey = @2", moduleId, collection, key);
            }
            return documents.SingleOrDefault();
        }
        public IEnumerable<OpenContentInfo> GetContents(int moduleId, string collection)
        {
            IEnumerable<OpenContentInfo> documents;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                documents = rep.Find("WHERE ModuleId = @0 AND Collection = @1", moduleId, collection);
            }
            return documents;
        }

        public IEnumerable<OpenContentInfo> GetContents(int[] contentIds)
        {
            IEnumerable<OpenContentInfo> documents;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                documents = rep.Find("WHERE ContentId IN (" + string.Join(",", contentIds) + ")");
            }
            return documents;
        }

        #endregion

        #region Private helper

        private static string GetContentIdCacheKey(int contentId)
        {
            return string.Concat(CACHE_PREFIX, "C-", contentId);
        }

        private static string GetModuleIdCacheKey(int moduleId, string suffix = null)
        {
            return string.Concat(CACHE_PREFIX, "M-", moduleId, string.IsNullOrEmpty(suffix) ? string.Empty : string.Concat("-", suffix));
        }

        private static void ClearDataCache(OpenContentInfo content)
        {
            if (content.ContentId > 0) DataCache.ClearCache(GetContentIdCacheKey(content.ContentId));
            if (content.ModuleId > 0) DataCache.ClearCache(GetModuleIdCacheKey(content.ModuleId));
        }

        #endregion

    }
}
