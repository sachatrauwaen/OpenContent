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
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Content;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components
{
    [TableName("OpenContent_Items")]
    [PrimaryKey("ContentId", AutoIncrement = true)]
    //[Cacheable("OpenContentItems", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class OpenContentInfo
    {
        private JToken _JsonAsJToken = null;
        public OpenContentInfo()
        {

        }
        public OpenContentInfo(string json)
        {
            Json = json;
        }
        public int ContentId { get; set; }
        public string Title { get; set; }
        public string Html { get; set; }
        public string Json { get; set; }
        [IgnoreColumn]
        public JToken JsonAsJToken
        {
            get
            {
                if (_JsonAsJToken == null && !string.IsNullOrEmpty(this.Json))
                {
                    _JsonAsJToken = JToken.Parse(this.Json);
                }
                // JsonAsJToken is modified (to remove other cultures)
                return _JsonAsJToken != null ? _JsonAsJToken.DeepClone() : null; 
            }
            set
            {
                _JsonAsJToken = value;
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

    }
}
