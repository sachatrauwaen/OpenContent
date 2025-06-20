﻿/*
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
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Exceptions;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Json;

namespace Satrabel.OpenContent
{
    public partial class Submissions : DotNetNuke.Entities.Modules.PortalModuleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    DataBindGrid();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void DataBindGrid()
        {
            var dynData = GetDataAsListOfDynamics();
            gvData.DataSource = ToDataTable(dynData);
            gvData.DataBind();
            for (int i = 0; i < gvData.Rows.Count; i++)
            {
                for (int j = 1; j < gvData.Rows[i].Cells.Count; j++)
                {
                    string encoded = gvData.Rows[i].Cells[j].Text;
                    gvData.Rows[i].Cells[j].Text = Context.Server.HtmlDecode(encoded);
                }
            }
        }

        protected void ExcelDownload_Click(object sender, EventArgs e)
        {
            var dynData = GetDataAsListOfDynamics();
            DataTable datatable = ToDataTable(dynData);
            string filename = GetFileNameFromFormName();
            var excelBytes = ExcelUtils.CreateExcel(datatable);
            ExcelUtils.PushDataAsExcelOntoHttpResponse(excelBytes, filename, HttpContext.Current);
        }

        #region Private Methods

        private List<dynamic> GetDataAsListOfDynamics()
        {
            var module = OpenContentModuleConfig.Create(this.ModuleConfiguration, PortalSettings);
            OpenContentController ctrl = new OpenContentController(ModuleContext.PortalId);
            var data = ctrl.GetContents(module.DataModule.ModuleId, "Submissions").OrderByDescending(c => c.CreatedOnDate);

            var dynData = new List<dynamic>();
            foreach (var item in data)
            {
                dynamic o = new ExpandoObject();
                var dict = (IDictionary<string, object>)o;
                o.CreatedOnDate = item.CreatedOnDate;
                o.Id = item.ContentId;
                o.Title = item.Title;
                try
                {
                    dynamic d = JsonUtils.JsonToDynamic(item.Json);
                    Dictionary<string, object> jdic = Dyn2Dict(d);
                    foreach (var p in jdic)
                    {
                        dict[p.Key] = p.Value;
                    }
                }
                catch (Exception)
                {
                    o.Error = $"Failed to Convert item [{item.ContentId}] to dynamic. Item.CreatedOnDate: {item.CreatedOnDate}";
                }

                dynData.Add(o);
            }
            return dynData;
        }

        private static Dictionary<string, object> Dyn2Dict(dynamic dynObj)
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

        private static string GetFileNameFromFormName()
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
                            if (key == "Files")
                            {
                                string files = "";
                                foreach (dynamic file in (DynamicJsonArray)value)
                                {
                                    //files = files + "<a href=\""+ file.url+"\">"+file.name+"</a> ";
                                    files = files + "<a href=\"" + file.url + "\" target=\"_blank\">" + file.name + "</a> ";
                                }
                                row.Add(files);
                            }
                            else
                            {
                                row.Add(string.Join(";", (DynamicJsonArray)value));
                            }
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

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            int id = int.Parse(btn.CommandArgument);
            OpenContentController ctrl = new OpenContentController(ModuleContext.PortalId);
            var data = ctrl.GetContent(id);
            ctrl.DeleteContent(data);
            DataBindGrid();
        }
    }
}