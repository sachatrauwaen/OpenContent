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
using System.Linq;
using System.Collections.Generic;
using DotNetNuke.Data;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;

namespace Satrabel.OpenContent.Components
{
    public class OpenContentController
    {
        public void AddContent(OpenContentInfo Content)
        {
            OpenContentVersion ver = new OpenContentVersion()
            {
                Json = Content.Json.ToJObject("Adding Content"),
                CreatedByUserId = Content.LastModifiedByUserId,
                CreatedOnDate = Content.LastModifiedOnDate
            };
            var versions = new List<OpenContentVersion>();
            versions.Add(ver);
            Content.Versions = versions;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                rep.Insert(Content);
            }
        }

        public void DeleteContent(OpenContentInfo Content)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                rep.Delete(Content);
            }
        }

        public IEnumerable<OpenContentInfo> GetContents(int moduleId)
        {
            IEnumerable<OpenContentInfo> Contents;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                Contents = rep.Get(moduleId);
            }
            return Contents;
        }

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

        public OpenContentInfo GetFirstContent(int moduleId)
        {
            OpenContentInfo Content;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                Content = rep.Get(moduleId).FirstOrDefault();
            }
            return Content;
        }

        public void UpdateContent(OpenContentInfo Content)
        {
            OpenContentVersion ver = new OpenContentVersion()
            {
                Json = Content.Json.ToJObject("UpdateContent"),
                CreatedByUserId = Content.LastModifiedByUserId,
                CreatedOnDate = Content.LastModifiedOnDate
            };
            var versions = Content.Versions;
            if (versions.Count == 0 || versions[0].Json != ver.Json)
            {
                versions.Insert(0, ver);
                if (versions.Count > 5)
                {
                    versions.RemoveAt(versions.Count - 1);
                }
                Content.Versions = versions;
            }
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<OpenContentInfo>();
                rep.Update(Content);
            }
        }

    }
}
