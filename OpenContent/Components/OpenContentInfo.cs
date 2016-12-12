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
using DotNetNuke.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Satrabel.OpenContent.Components.Lucene.Index;
using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components
{
    [TableName("OpenContent_Items")]
    [PrimaryKey("ContentId", AutoIncrement = true)]
    //[Cacheable("OpenContentItems", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class OpenContentInfo : IIndexableItem
    {
        private JToken _jsonAsJToken = null;
        public OpenContentInfo()
        {

        }
        public OpenContentInfo(string json)
        {
            Json = json;
        }
        public int ContentId { get; set; }
        [ColumnName("DocumentKey")] 
        public string Key { get; internal set; }
        public string Collection { get; internal set; }
        public string Title { get; set; }        
        public string Json { get; set; }

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
        public int ModuleId { get; set; }
        public int CreatedByUserId { get; set; }
        public int LastModifiedByUserId { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public string VersionsJson { get; set; }
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

        #region IIndexableItem
        public string GetId()
        {
            return ContentId.ToString();
        }

        public string GetScope()
        {
            return OpenContentInfo.GetScope(ModuleId, Collection);
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

        public static string GetScope(int moduleId, string collection)
        {
            if (collection == "Items")
                return moduleId.ToString();
            else
                return moduleId.ToString() + "/" + collection;
        }
        #endregion
    }
}
