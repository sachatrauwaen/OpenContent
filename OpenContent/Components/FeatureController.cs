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

using System.Collections.Generic;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Common.Utilities;
using System.Xml;
using System.Linq;
using DotNetNuke.Common;
using System;
using DotNetNuke.Services.Search.Entities;
using Newtonsoft.Json.Linq;
using DotNetNuke.Entities.Portals;
using System.IO;
using System.Web.Hosting;
using DotNetNuke.Common.Internal;
using DotNetNuke.Services.Search.Controllers;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Indexing;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.TemplateHelpers;

namespace Satrabel.OpenContent.Components
{
    public class FeatureController : ModuleSearchBase, IPortable, IUpgradeable, IModuleSearchResultController
    {
        #region Optional Interfaces
        public string ExportModule(int moduleId)
        {
            string xml = "";
            OpenContentController ctrl = new OpenContentController();
            var items = ctrl.GetContents(moduleId);
            xml += "<opencontent>";
            foreach (var item in items)
            {
                xml += "<item>";
                xml += "<json>" + XmlUtils.XMLEncode(item.Json) + "</json>";
                xml += "<collection>" + XmlUtils.XMLEncode(item.Collection) + "</collection>";
                xml += "<key>" + XmlUtils.XMLEncode(item.Id) + "</key>";
                xml += "</item>";
            }
            xml += "</opencontent>";
            return xml;
        }
        public void ImportModule(int moduleId, string content, string version, int userId)
        {
            var module = OpenContentModuleConfig.Create(moduleId, Null.NullInteger, PortalSettings.Current);
            //var index = module.Settings.Template.Manifest.Index;
            //var indexConfig = OpenContentUtils.GetIndexConfig(module.Settings.Template);
            var dataSource = new OpenContentDataSource();
            XmlNode xml = Globals.GetContent(content, "opencontent");
            var items = xml.SelectNodes("item");
            if (items != null)
            {
                foreach (XmlNode item in items)
                {
                    XmlNode json = item.SelectSingleNode("json");
                    XmlNode collection = item.SelectSingleNode("collection");
                    XmlNode key = item.SelectSingleNode("key");
                    JToken data;
                    try
                    {
                        data = item.InnerText;
                        data["_id"] = key?.InnerText ?? "";
                    }
                    catch (Exception e)
                    {
                        App.Services.Logger.Error($"Failed to parse imported json. Item key: {key}. Collection: {collection}. Error: {e.Message}. Stacktrace: {e.StackTrace}");
                        throw;
                    }

                    var dsContext = OpenContentUtils.CreateDataContext(module, userId);
                    dsContext.Collection = collection?.InnerText ?? "";
                    //dsContext.Key = key?.InnerText ?? "";

                    dataSource.Add(dsContext, data);
                    //dataSource.Add(contentInfo, index, indexConfig);
                }
            }
        }

        #region ModuleSearchBase
        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo modInfo, DateTime beginDateUtc)
        {
            App.Services.Logger.Trace($"Indexing content Module {modInfo.ModuleID} - Tab {modInfo.TabID} - Culture {modInfo.CultureCode}- indexing from {beginDateUtc}");
            var searchDocuments = new List<SearchDocument>();

            //If module is marked as "don't index" then return no results
            if (modInfo.ModuleSettings.GetValue("AllowIndex", "True") == "False")
            {
                App.Services.Logger.Trace($"Indexing content {modInfo.ModuleID}|{modInfo.CultureCode} - NOT - MODULE Indexing disabled");
                return searchDocuments;
            }

            //If tab of the module is marked as "don't index" then return no results
            if (modInfo.ParentTab.TabSettings.GetValue("AllowIndex", "True") == "False")
            {
                App.Services.Logger.Trace($"Indexing content {modInfo.ModuleID}|{modInfo.CultureCode} - NOT - TAB Indexing disabled");
                return searchDocuments;
            }

            //If tab is marked as "inactive" then return no results
            if (modInfo.ParentTab.DisableLink)
            {
                App.Services.Logger.Trace($"Indexing content {modInfo.ModuleID}|{modInfo.CultureCode} - NOT - TAB is inactive");
                return searchDocuments;
            }

            var module = OpenContentModuleConfig.Create(modInfo, PortalSettings.Current);
            if (module.Settings.Template?.Main == null || !module.Settings.Template.Main.DnnSearch)
            {
                return searchDocuments;
            }
            if (module.Settings.IsOtherModule)
            {
                return searchDocuments;
            }

            IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
            var dsContext = OpenContentUtils.CreateDataContext(module);

            IDataItems contentList = ds.GetAll(dsContext, null);
            if (!contentList.Items.Any())
            {
                App.Services.Logger.Trace($"Indexing content {modInfo.ModuleID}|{modInfo.CultureCode} - NOT - No content found");
            }
            foreach (IDataItem content in contentList.Items)
            {
                if (content == null)
                {
                    App.Services.Logger.Trace($"Indexing content {modInfo.ModuleID}|{modInfo.CultureCode} - NOT - Content is Null");
                }
                else if (content.LastModifiedOnDate.ToUniversalTime() > beginDateUtc
                      && content.LastModifiedOnDate.ToUniversalTime() < DateTime.UtcNow)
                {
                    SearchDocument searchDoc;
                    if (DnnLanguageUtils.IsMultiLingualPortal(modInfo.PortalID))
                    {
                        searchDoc = GetLocalizedItem(modInfo, module.Settings, content);
                        searchDocuments.Add(searchDoc);
                        if (modInfo.LocalizedModules != null)
                            foreach (var localizedModule in modInfo.LocalizedModules)
                            {
                                SearchDocument localizedSearchDoc = GetLocalizedItem(localizedModule.Value, module.Settings, content);
                                searchDocuments.Add(localizedSearchDoc);
                            }
                    }
                    else
                    {
                        searchDoc = CreateSearchDocument(modInfo, module.Settings, content.Data, content.Id, "", content.Title, JsonToSearchableString(content.Data), content.LastModifiedOnDate.ToUniversalTime());
                        searchDocuments.Add(searchDoc);
                        App.Services.Logger.Trace($"Indexing content {modInfo.ModuleID}|{modInfo.CultureCode} -  OK!  {searchDoc.Title} ({modInfo.TabID}) of {content.LastModifiedOnDate.ToUniversalTime()}");
                    }
                }
                else
                {
                    App.Services.Logger.Trace($"Indexing content {modInfo.ModuleID}|{modInfo.CultureCode} - NOT - No need to index: lastmod {content.LastModifiedOnDate.ToUniversalTime()} ");
                }
            }
            return searchDocuments;
        }

        private static SearchDocument GetLocalizedItem(ModuleInfo moduleInfo, OpenContentSettings settings, IDataItem content)
        {
            string culture = moduleInfo.CultureCode;
            JToken title;
            JToken description;
            JToken singleLanguage = content.Data.DeepClone(); //Clone to keep Simplification into this Method
            JsonUtils.SimplifyJson(singleLanguage, culture);

            if (content.Title.IsJson())
            {
                title = singleLanguage["Title"] ?? moduleInfo.ModuleTitle;
                description = singleLanguage["Description"] ?? JsonToSearchableString(content.Data);
            }
            else
            {
                title = content.Title;
                description = JsonToSearchableString(singleLanguage);
            }
            var searchDoc = CreateSearchDocument(moduleInfo, settings, singleLanguage, content.Id, culture, title.ToString(), description.ToString(), content.LastModifiedOnDate.ToUniversalTime());
            App.Services.Logger.Debug($"Indexing content {moduleInfo.ModuleID}|{culture} -  OK!  {searchDoc.Title} ({ moduleInfo.TabID})  {content.LastModifiedOnDate.ToUniversalTime()}");
            return searchDoc;
        }

        private static SearchDocument CreateSearchDocument(ModuleInfo modInfo, OpenContentSettings settings, JToken content, string itemId, string culture, string title, string body, DateTime time)
        {
            // existance of settings.Template.Main has already been checked: we wouldn't be here if it doesn't exist
            // but still, we don't want to count on that too much
            var ps = new PortalSettings(modInfo.PortalID);
            ps.PortalAlias = PortalAliasController.Instance.GetPortalAlias(ps.DefaultPortalAlias);

            var url = TestableGlobals.Instance.NavigateURL(modInfo.TabID, ps, "", $"id={itemId}");

            string docTitle = modInfo.ModuleTitle; // SK: this is the behaviour before introduction of DnnSearchTitle
            if (!string.IsNullOrEmpty(settings.Template?.Main?.DnnSearchTitle))
            {
                var dynForHBS = JsonUtils.JsonToDictionary(content.ToString());
                var hbEngine = new HandlebarsEngine();
                docTitle = hbEngine.ExecuteWithoutFaillure(settings.Template.Main.DnnSearchTitle, dynForHBS, modInfo.ModuleTitle);
            }

            var retval = new SearchDocument
            {
                UniqueKey = modInfo.ModuleID + "-" + itemId + "-" + culture,
                PortalId = modInfo.PortalID,
                Title = docTitle.StripHtml(),
                Description = title.StripHtml(),
                Body = body.StripHtml(),
                ModifiedTimeUtc = time,
                CultureCode = culture,
                TabId = modInfo.TabID,
                ModuleId = modInfo.ModuleID,
                ModuleDefId = modInfo.ModuleDefID,
                Url = url
            };

            return retval;
        }

        private static string JsonToSearchableString(JToken data)
        {
            //string tagPattern = @"<(.|\n)*?>";
            string result = "";

            if (data.Type == JTokenType.Object)
            {
                foreach (JProperty item in data.Children<JProperty>())
                {
                    result += JsonToSearchableString(item);
                }
            }
            else if (data.Type == JTokenType.Array)
            {
                foreach (JToken item in data.Children())
                {
                    result += JsonToSearchableString(item);
                }
            }
            else if (data.Type == JTokenType.Property)
            {
                var item = (JProperty)data;

                if (item.Value.Type == JTokenType.String)
                {
                    //result += Regex.Replace(item.Value.ToString(), tagPattern, string.Empty) + " ... ";
                    result += data.ToString().StripHtml() + " ... ";
                }
                else
                {
                    result += JsonToSearchableString(item.Value);
                }
            }
            else
            {
                //result += Regex.Replace(data.ToString(), tagPattern, string.Empty) + " ... ";
                result += data.ToString().StripHtml() + " ... ";
            }

            return result;
        }

        #endregion
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpgradeModule implements the IUpgradeable Interface
        /// </summary>
        /// <param name="version">The current version of the module</param>
        /// -----------------------------------------------------------------------------
        //public string UpgradeModule(string Version)
        //{
        //	throw new System.NotImplementedException("The method or operation is not implemented.");
        //}
        #endregion

        public string UpgradeModule(string version)
        {
            string res = "";
            if (version == "02.01.00")
            {

                var pc = new PortalController();
                foreach (var p in pc.GetPortals().Cast<PortalInfo>())
                {
                    string webConfig = HostingEnvironment.MapPath("~/" + p.HomeDirectory + "/OpenContent/Templates/web.config");
                    res += webConfig;
                    if (File.Exists(webConfig))
                    {
                        res += " : found \n";
                        File.Delete(webConfig);
                        string filename = HostingEnvironment.MapPath("~/DesktopModules/OpenContent/Templates/web.config");
                        File.Copy(filename, webConfig);
                    }
                }
            }
            else if (version == "03.02.00")
            {
                Indexer.Instance.IndexAll();
            }
            return version + res;
        }

        #region IModuleSearchResultController

        /// <summary>
        /// Does the user in the Context have View Permission on the Document
        /// </summary>
        /// <param name="searchResult">Search Result</param>
        /// <returns>True or False</returns>
        public bool HasViewPermission(SearchResult searchResult)
        {
            return true; //todo: should do some checking here.
        }

        public string GetDocUrl(SearchResult searchResult)
        {
            return DnnUrlUtils.NavigateUrl(searchResult.TabId);
        }
        #endregion
    }
}
