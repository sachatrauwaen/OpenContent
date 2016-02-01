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
using System.Web.Caching;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Content;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Satrabel.OpenContent.Components
{
    [TableName("OpenContent_AdditionalData")]
    [PrimaryKey("AdditionalDataId", AutoIncrement = true)]
    [Cacheable("OpenContentAdditionalData", CacheItemPriority.Default, 20)]
    [Scope("Scope")]
    public class AdditionalDataInfo
    {
        public int AdditionalDataId { get; set; }
        public string Json { get; set; }
        public string Scope { get; set; }
        public string DataKey { get; set; }
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
