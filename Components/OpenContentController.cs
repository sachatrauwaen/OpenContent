/*
' Copyright (c) 2015 Satrabel.be
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
using DotNetNuke.Data;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Lucene.Index;
using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentController
    {
        public void AddContent(OpenContentInfo content, bool index, FieldConfig indexConfig)
        {
            OpenContentVersion ver = new OpenContentVersion()
            {
                Json = content.Json.ToJObject("Adding Content"),
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
                LuceneController.Instance.Commit();
            }
        }

        public void DeleteContent(OpenContentInfo content, bool index)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                rep.Delete(content);
            }
            if (index)
            {
                LuceneController.Instance.Delete(content);
                LuceneController.Instance.Commit();
            }
        }

        public IEnumerable<OpenContentInfo> GetContents(int moduleId)
        {
            IEnumerable<OpenContentInfo> content;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                content = rep.Get(moduleId);
            }
            return content;
        }

        public OpenContentInfo GetContent(int contentId)
        {
            OpenContentInfo content;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                content = rep.GetById(contentId);
            }
            return content;
        }

        public OpenContentInfo GetFirstContent(int moduleId)
        {
            OpenContentInfo content;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                content = rep.Get(moduleId).FirstOrDefault();
            }
            return content;
        }

        public void UpdateContent(OpenContentInfo content, bool index, FieldConfig indexConfig)
        {
            OpenContentVersion ver = new OpenContentVersion()
            {
                Json = content.Json.ToJObject("UpdateContent"),
                CreatedByUserId = content.LastModifiedByUserId,
                CreatedOnDate = content.LastModifiedOnDate
            };
            var versions = content.Versions;
            if (versions.Count == 0 || versions[0].Json.ToString() != content.Json)
            {
                versions.Insert(0, ver);
                if (versions.Count > 5)
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
                LuceneController.Instance.Update(content, indexConfig);
                LuceneController.Instance.Commit();
            }
        }

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
        [Obsolete("This method is obsolete since dec 2015; use UpdateContent(OpenContentInfo content, bool index) instead")]
        public void UpdateContent(OpenContentInfo content)
        {
            UpdateContent(content, false, null);
        }
        [Obsolete("This method is obsolete since dec 2015; use AddContent(OpenContentInfo content, bool index) instead")]
        public void AddContent(OpenContentInfo content)
        {
            AddContent(content, false, null);
        }

    }
}
