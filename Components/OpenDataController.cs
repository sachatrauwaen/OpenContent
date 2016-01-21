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
    public class OpenDataController
    {
        #region Commands

        public void AddData(OpenDataInfo data)
        {
            OpenContentVersion ver = new OpenContentVersion()
            {
                Json = data.Json.ToJObject("Adding Data"),
                CreatedByUserId = data.LastModifiedByUserId,
                CreatedOnDate = data.LastModifiedOnDate
            };
            var versions = new List<OpenContentVersion>();
            versions.Add(ver);
            data.Versions = versions;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenDataInfo>();
                rep.Insert(data);
            }
        }
        public void DeleteData(OpenDataInfo data)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenDataInfo>();
                rep.Delete(data);
            }
        }

        public void UpdateData(OpenDataInfo data)
        {
            OpenContentVersion ver = new OpenContentVersion()
            {
                Json = data.Json.ToJObject("UpdateContent"),
                CreatedByUserId = data.LastModifiedByUserId,
                CreatedOnDate = data.LastModifiedOnDate
            };
            var versions = data.Versions;
            if (versions.Count == 0 || versions[0].Json.ToString() != data.Json)
            {
                versions.Insert(0, ver);
                if (versions.Count > 5)
                {
                    versions.RemoveAt(versions.Count - 1);
                }
                data.Versions = versions;
            }
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenDataInfo>();
                rep.Update(data);
            }
        }

        #endregion

        #region Queries

        public IEnumerable<OpenDataInfo> GetDatas(string scope)
        {
            IEnumerable<OpenDataInfo> content;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenDataInfo>();
                content = rep.Get(scope);
            }
            return content;
        }

        public OpenDataInfo GetData(int dataId)
        {
            OpenDataInfo data;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenDataInfo>();
                data = rep.GetById(dataId);
            }
            return data;
        }

        public OpenDataInfo GetData(string scope, string key)
        {
            OpenDataInfo content = null;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenDataInfo>();
                var lst = rep.Get(scope);
                if (lst != null)
                {
                    content = lst.SingleOrDefault(d => d.DataKey == key);
                }
            }
            return content;
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
