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
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentController
    {
        private const string CachePrefix = "Satrabel.OpenContent.Components.OpenContentController-";
        private const int CacheTime = 60;

        #region Commands

        public void AddContent(OpenContentInfo content, bool index, FieldConfig indexConfig)
        {
            ClearCache(content);

            OpenContentVersion ver = new OpenContentVersion()
            {
                Json = content.JsonAsJToken,
                CreatedByUserId = content.LastModifiedByUserId,
                CreatedOnDate = content.LastModifiedOnDate
            };
            var versions = new List<OpenContentVersion>();
            versions.Add(ver);
            content.Versions = versions;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                rep.Insert(content);
            }
            if (index)
            {

                LuceneController.Instance.Add(content, indexConfig);
                LuceneController.Instance.Store.Commit();
            }
        }

        [Obsolete("This method is obsolete since dec 2015; use AddContent(OpenContentInfo content, bool index) instead")]
        public void AddContent(OpenContentInfo content)
        {
            AddContent(content, false, null);
        }

        public void DeleteContent(OpenContentInfo content, bool index)
        {
            ClearCache(content);

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                rep.Delete(content);
            }
            if (index)
            {
                LuceneController.Instance.Delete(content);
                LuceneController.Instance.Store.Commit();
            }
        }

        public void UpdateContent(OpenContentInfo content, bool index, FieldConfig indexConfig)
        {
            ClearCache(content);

            OpenContentVersion ver = new OpenContentVersion()
            {
                Json = content.JsonAsJToken,
                CreatedByUserId = content.LastModifiedByUserId,
                CreatedOnDate = content.LastModifiedOnDate
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
            }
            if (index)
            {
                if (indexConfig != null && indexConfig.Fields != null && !indexConfig.Fields.ContainsKey("publishstartdate")
                    && content.JsonAsJToken != null && content.JsonAsJToken["publishstartdate"] == null)
                {
                    content.JsonAsJToken["publishstartdate"] = DateTime.MinValue;
                }
                if (indexConfig != null && indexConfig.Fields != null && !indexConfig.Fields.ContainsKey("publishenddate")
                    && content.JsonAsJToken != null && content.JsonAsJToken["publishenddate"] == null)
                {
                    content.JsonAsJToken["publishenddate"] = DateTime.MaxValue;
                }
                LuceneController.Instance.Update(content, indexConfig);
                LuceneController.Instance.Store.Commit();
            }
        }

        [Obsolete("This method is obsolete since dec 2015; use UpdateContent(OpenContentInfo content, bool index) instead")]
        public void UpdateContent(OpenContentInfo content)
        {
            UpdateContent(content, false, null);
        }


        #endregion

        #region Queries

        public IEnumerable<OpenContentInfo> GetContents(int moduleId)
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

        public OpenContentInfo GetContent(int contentId)
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

        public OpenContentInfo GetFirstContent(int moduleId)
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

        /* slow !!!
        public OpenContentInfo GetContent(int ContentId, int moduleId)
        {
            OpenContentInfo Content;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                Content = rep.GetById(ContentId, moduleId);                
            }
            return Content;
        }
         */
    }
}
