/*
' Copyright (c) 2015  Satrabel.com
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
using DotNetNuke.Services.Exceptions;
using Satrabel.OpenContent.Components.Json;
using System.Collections.Generic;
using System.Dynamic;
using System.Web.Helpers;
using System.Data;
using System.Web;
using Satrabel.OpenContent.Components;

namespace Satrabel.OpenForm
{
    public partial class Submissions : DotNetNuke.Entities.Modules.PortalModuleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    var dynData = GetDataAsListOfDynamics();
                    gvData.DataSource = ToDataTable(dynData);
                    gvData.DataBind();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void ExcelDownload_Click(object sender, EventArgs e)
        {
            var dynData = GetDataAsListOfDynamics();
            DataTable datatable = ToDataTable(dynData);
            string filename=GetFileNameFromFormName();
            ExcelUtils.OutputFile(datatable, filename, HttpContext.Current);
        }

        #region Private Methods

        private List<dynamic> GetDataAsListOfDynamics()
        {
            var module = new OpenContentModuleInfo(this.ModuleConfiguration);
            OpenContentController ctrl = new OpenContentController();
            var data = ctrl.GetContents(module.DataModule.ModuleID, "Submissions").OrderByDescending(c => c.CreatedOnDate);
            var dynData = new List<dynamic>();
            foreach (var item in data)
            {
                dynamic o = new ExpandoObject();
                var dict = (IDictionary<string, object>)o;
                o.CreatedOnDate = item.CreatedOnDate;
                //o.Json = item.Json;
                dynamic d = JsonUtils.JsonToDynamic(item.Json);
                //o.Data = d;
                Dictionary<String, Object> jdic = Dyn2Dict(d);
                foreach (var p in jdic)
                {
                    dict[p.Key] = p.Value;
                }
                dynData.Add(o);
            }
            return dynData;
        }

        private Dictionary<String, Object> Dyn2Dict(dynamic dynObj)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var name in dynObj.GetDynamicMemberNames())
            {
                dictionary.Add(name, GetProperty(dynObj, name));
            }
            return dictionary;
        }

        private static object GetProperty(object target, string name)
        {
            var site = System.Runtime.CompilerServices.CallSite<Func<System.Runtime.CompilerServices.CallSite, object, object>>.Create(Microsoft.CSharp.RuntimeBinder.Binder.GetMember(0, name, target.GetType(), new[] { Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(0, null) }));
            return site.Target(site, target);
        }

        private string GetFileNameFromFormName()
        {
            //todo determine that current form and create a filename based on the name of the form.
            return "submissions.xlsx";
        }

        private static DataTable ToDataTable(IEnumerable<dynamic> items)
        {
            var data = items.ToArray();
            if (!data.Any()) return null;
            var dt = new DataTable();
            foreach (dynamic d in data)
            {
                foreach (var key in ((IDictionary<string, object>)d).Keys)
                {
                    if (!dt.Columns.Contains(key))
                    {
                        dt.Columns.Add(key);
                    }
                }
            }
            string[] columnNames = dt.Columns.Cast<DataColumn>()
                .Select(x => x.ColumnName)
                .ToArray();
            foreach (var d in data)
            {
                var row = new List<object>();
                var dic = (IDictionary<string, object>)d;
                foreach (string key in columnNames)
                {
                    if (dic.ContainsKey(key))
                    {
                        object value = dic[key];
                        if (value is DynamicJsonObject)
                        {
                            row.Add(value.ToJson());
                        }
                        else if (value is DynamicJsonArray)
                        {
                            row.Add(string.Join(";", (DynamicJsonArray)value));
                        }
                        else
                        {
                            row.Add(value);
                        }
                    }
                    else
                    {
                        row.Add(""); //add empty value to preserve table structure.
                    }
                }
                dt.Rows.Add(row.ToArray());
            }
            return dt;
        }

        #endregion
    }
}