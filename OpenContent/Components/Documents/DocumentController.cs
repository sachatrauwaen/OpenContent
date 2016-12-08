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
using DotNetNuke.Data;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components.Documents
{
    public class DocumentController
    {
        
        #region Commands

        public void AddDocument(DocumentInfo doc, FieldConfig indexConfig)
        {
            var json = doc.Json.ToJObject("Adding Document");
            if (json["Id"] != null)
            {
                doc.Key = json["Id"].ToString();
            }
            if (string.IsNullOrEmpty(doc.Key))
            {
                doc.Key = ObjectId.NewObjectId().ToString();
            }
            OpenContentVersion ver = new OpenContentVersion()
            {
                Json = json,
                CreatedByUserId = doc.CreatedByUserId,
                CreatedOnDate = doc.CreatedOnDate,
                LastModifiedByUserId = doc.LastModifiedByUserId,
                LastModifiedOnDate = doc.LastModifiedOnDate
            };
            var versions = new List<OpenContentVersion>();
            versions.Add(ver);
            doc.Versions = versions;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DocumentInfo>();
                rep.Insert(doc);
            }
            LuceneController.Instance.Add(doc, indexConfig);
            LuceneController.Instance.Store.Commit();
        }
        public void DeleteDocument(DocumentInfo doc)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DocumentInfo>();
                rep.Delete(doc);
            }
            LuceneController.Instance.Delete(doc);
            LuceneController.Instance.Store.Commit();
        }

        public void UpdateDocument(DocumentInfo doc, FieldConfig indexConfig)
        {
            OpenContentVersion ver = new OpenContentVersion()
            {
                Json = doc.Json.ToJObject("UpdateContent"),
                CreatedByUserId = doc.CreatedByUserId,
                CreatedOnDate = doc.CreatedOnDate,
                LastModifiedByUserId = doc.LastModifiedByUserId,
                LastModifiedOnDate = doc.LastModifiedOnDate
            };
            var versions = doc.Versions;
            if (versions.Count == 0 || versions[0].Json.ToString() != doc.Json)
            {
                versions.Insert(0, ver);
                if (versions.Count > OpenContentControllerFactory.Instance.OpenContentGlobalSettingsController.GetMaxVersions())
                {
                    versions.RemoveAt(versions.Count - 1);
                }
                doc.Versions = versions;
            }
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DocumentInfo>();
                rep.Update(doc);
            }
            //content.HydrateDefaultFields(indexConfig);

            LuceneController.Instance.Update(doc, indexConfig);
            LuceneController.Instance.Store.Commit();
        }

        #endregion

        #region Queries

        public DocumentInfo GetDocument(string scope, string collection, string key)
        {
            IEnumerable<DocumentInfo> documents;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DocumentInfo>();
                documents = rep.Find("WHERE scope = @0 AND Collection = @1 AND Id = @3", scope, collection, key);
            }
            return documents.SingleOrDefault();
        }

        public DocumentInfo GetDocumentById(int documentId)
        {
            DocumentInfo doc;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DocumentInfo>();
                doc = rep.GetById(documentId);
            }
            return doc;
        }

        public IEnumerable<DocumentInfo> GetDocuments(string scope, string collection)
        {
            IEnumerable<DocumentInfo> documents;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DocumentInfo>();
                documents = rep.Find("WHERE scope = @0 AND Collection = @1", scope, collection);
            }
            return documents;
        }

        #endregion
    }
}
