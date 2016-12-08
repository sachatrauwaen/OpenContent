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
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;
using Satrabel.OpenContent.Components.Lucene.Index;
using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Documents
{
    [TableName("OpenContent_Documents")]
    [PrimaryKey("DocumentId", AutoIncrement = true)]
    //[Cacheable("OpenContentDocuments", CacheItemPriority.Default, 20)]
    //[Scope("Scope")]
    public class DocumentInfo : IIndexableItem
    {
        public int DocumentId { get; set; }
        public string Scope { get; set; }
        public string Collection { get; set; }
        [ColumnName("DocumentKey")]
        public string Key { get; set; }
        public string Json { get; set; }
        public string VersionsJson { get; set; }
        public int CreatedByUserId { get; set; }
        public int LastModifiedByUserId { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        
        [IgnoreColumn]
        public List<OpenContentVersion> Versions
        {
            get
            {
                List<OpenContentVersion> lst;
                if (string.IsNullOrWhiteSpace(VersionsJson))
                {
                    lst = new List<OpenContentVersion>();
                }
                else
                {
                    lst = JsonConvert.DeserializeObject<List<OpenContentVersion>>(VersionsJson);
                }
                return lst;
            }
            set
            {
                VersionsJson = JsonConvert.SerializeObject(value);
            }
        }

        private JToken _jsonAsJToken = null;
        [IgnoreColumn]
        public JToken JsonAsJToken
        {
            get
            {
                if (_jsonAsJToken == null && !string.IsNullOrEmpty(this.Json))
                {
                    _jsonAsJToken = JToken.Parse(this.Json);
                }
                // JsonAsJToken is modified (to remove other cultures)
                return _jsonAsJToken != null ? _jsonAsJToken.DeepClone() : null;
            }
            set
            {
                _jsonAsJToken = value;
            }
        }

        public string GetId()
        {
            return DocumentId.ToString();
        }

        public string GetScope()
        {
            return Scope+"/"+Collection;
        }

        public string GetCreatedByUserId()
        {
            return CreatedByUserId.ToString();
        }

        public DateTime GetCreatedOnDate()
        {
            return CreatedOnDate;
        }

        public JToken GetData()
        {
            return JsonAsJToken;
        }

        public string GetSource()
        {
            return Json;
        }
    }
}
