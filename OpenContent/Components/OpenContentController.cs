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

using System;
using System.Linq;
using System.Collections.Generic;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Lucene.Config;
using Satrabel.OpenContent.Components.Common;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentController
    {
        private const string CachePrefix = "Satrabel.OpenContent.Components.OpenContentController-";
        private const int CacheTime = 60;

        #region Commands

        internal void AddContent(OpenContentInfo content, bool index, FieldConfig indexConfig)
        {
            ClearCache(content);
            var json = content.JsonAsJToken;
            if (string.IsNullOrEmpty(content.Key))
            {
                content.Key = json["_id"]?.ToString() ?? ObjectId.NewObjectId().ToString();
            }
            if (string.IsNullOrEmpty(content.Collection))
            {
                content.Collection = AppConfig.DEFAULT_COLLECTION;
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
            if (index)
            {
                LuceneController.Instance.Add(content, indexConfig);
                LuceneController.Instance.Store.Commit();
            }
        }

        internal void DeleteContent(OpenContentInfo content, bool index)
        {
            ClearCache(content);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                rep.Delete(content);
                ModuleController.SynchronizeModule(content.ModuleId);
            }
            if (index)
            {
                LuceneController.Instance.Delete(content);
                LuceneController.Instance.Store.Commit();
            }
        }

        internal void UpdateContent(OpenContentInfo content, bool index, FieldConfig indexConfig)
        {
            ClearCache(content);
            var json = content.JsonAsJToken;
            //json["_id"] = content.Id;
            //content.Json = json.ToString();
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
                if (versions.Count > OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetMaxVersions())
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
            if (index)
            {
                content.HydrateDefaultFields(indexConfig);
                LuceneController.Instance.Update(content, indexConfig);
                LuceneController.Instance.Store.Commit();
            }
        }

        #endregion

        #region Queries

        internal IEnumerable<OpenContentInfo> GetContents(int moduleId)
        {
            var cacheArgs = new CacheItemArgs(GetModuleIdCacheKey(moduleId, "GetContents"), CacheTime);
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

        internal OpenContentInfo GetContent(int contentId)
        {
            var cacheArgs = new CacheItemArgs(GetContentIdCacheKey(contentId), CacheTime);
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

        internal OpenContentInfo GetFirstContent(int moduleId)
        {
            var cacheArgs = new CacheItemArgs(GetModuleIdCacheKey(moduleId) + "GetFirstContent", CacheTime);
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

        internal OpenContentInfo GetContent(int moduleId, string collection, string id)
        {
            if (collection == AppConfig.DEFAULT_COLLECTION)
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
        internal OpenContentInfo GetContentByKey(int moduleId, string collection, string key)
        {
            IEnumerable<OpenContentInfo> documents;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                documents = rep.Find("WHERE ModuleId = @0 AND Collection = @1 AND DocumentKey = @2", moduleId, collection, key);
            }
            return documents.SingleOrDefault();
        }
        internal IEnumerable<OpenContentInfo> GetContents(int moduleId, string collection)
        {
            IEnumerable<OpenContentInfo> documents;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                documents = rep.Find("WHERE ModuleId = @0 AND Collection = @1", moduleId, collection);
            }
            return documents;
        }

        internal IEnumerable<OpenContentInfo> GetContents(int[] contentIds)
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
            return string.Concat(CachePrefix, "C-", contentId);
        }

        private static string GetModuleIdCacheKey(int moduleId, string suffix = null)
        {
            return string.Concat(CachePrefix, "M-", moduleId, string.IsNullOrEmpty(suffix) ? string.Empty : string.Concat("-", suffix));
        }

        private static void ClearCache(OpenContentInfo content)
        {
            if (content.ContentId > 0) DataCache.ClearCache(GetContentIdCacheKey(content.ContentId));
            if (content.ModuleId > 0) DataCache.ClearCache(GetModuleIdCacheKey(content.ModuleId));
        }

        #endregion

        #region Obsolete

        [Obsolete("This method is obsolete since dec 2015; use AddContent(OpenContentInfo content, bool index) instead")]
        public void AddContent(OpenContentInfo content)
        {
            AddContent(content, false, null);
        }

        [Obsolete("This method is obsolete since dec 2015; use UpdateContent(OpenContentInfo content, bool index) instead")]
        public void UpdateContent(OpenContentInfo content)
        {
            UpdateContent(content, false, null);
        }

        #endregion
    }
}
