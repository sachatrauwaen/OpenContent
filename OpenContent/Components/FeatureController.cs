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
using System.Collections;
using DotNetNuke.Services.Search.Entities;
using Newtonsoft.Json.Linq;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Content.Taxonomy;
using System.IO;
using System.Web;
using System.Web.Hosting;
using DotNetNuke.Common.Internal;
using DotNetNuke.Services.Search.Controllers;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.TemplateHelpers;

namespace Satrabel.OpenContent.Components
{
    public class FeatureController : ModuleSearchBase, IPortable, IUpgradeable, IModuleSearchResultController
    {
        #region Optional Interfaces
        public string ExportModule(int moduleId)
        {
            string xml = "";
            OpenContentController ctrl = new OpenContentController(PortalSettings.Current.PortalId);

            var tabModules = ModuleController.Instance.GetTabModulesByModule(moduleId);

            Hashtable moduleSettings = tabModules.Any() ? tabModules.First().ModuleSettings : new Hashtable();
            
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

            foreach (DictionaryEntry moduleSetting in moduleSettings)
            {
                xml += "<moduleSetting>";
                xml += "<settingName>" + XmlUtils.XMLEncode(moduleSetting.Key.ToString()) + "</settingName>";
                xml += "<settingValue>" + XmlUtils.XMLEncode(moduleSetting.Value.ToString()) + "</settingValue>";
                xml += "</moduleSetting>";
            }
            
            xml += "</opencontent>";
            return xml;
        }
        public void ImportModule(int moduleId, string content, string version, int userId)
        {
            var module = OpenContentModuleConfig.Create(moduleId, Null.NullInteger, PortalSettings.Current);
            var dataSource = new OpenContentDataSource();
            var dsContext = OpenContentUtils.CreateDataContext(module, userId);
            XmlNode xml = Globals.GetContent(content, "opencontent");
            var items = xml.SelectNodes("item");
            if (items != null)
            {
                foreach (XmlNode item in items)
                {
                    XmlNode json = item.SelectSingleNode("json");
                    XmlNode collection = item.SelectSingleNode("collection");
                    XmlNode key = item.SelectSingleNode("key");
                    try
                    {
                        JToken data = JToken.Parse(json.InnerText);
                        dsContext.Collection = collection?.InnerText ?? "";
                        dataSource.Add(dsContext, data);
                        App.Services.CacheAdapter.SyncronizeCache(module);
                    }
                    catch (Exception e)
                    {
                        App.Services.Logger.Error($"Failed to parse imported json. Item key: {key}. Collection: {collection}. Error: {e.Message}. Stacktrace: {e.StackTrace}");
                        throw;
                    }
                }
            }
            var settings = xml.SelectNodes("moduleSetting");
            if (settings != null)
            {
                foreach (XmlNode setting in settings)
                {
                    XmlNode settingName = setting.SelectSingleNode("settingName");
                    XmlNode settingValue = setting.SelectSingleNode("settingValue");

                    if (!string.IsNullOrEmpty(settingName?.InnerText))
                    {
                        ModuleController.Instance.UpdateModuleSetting(moduleId, settingName.InnerText, settingValue?.InnerText ?? "");
                    }
                }
            }
            module = OpenContentModuleConfig.Create(moduleId, Null.NullInteger, PortalSettings.Current);
            
            LuceneUtils.ReIndexModuleData(module);
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
                        // first process the default language module
                        var culture = modInfo.CultureCode;
                        var localizedData = GetLocalizedContent(content.Data, culture);
                        searchDoc = CreateSearchDocument(modInfo, module.Settings, localizedData, content.Id, culture, content.Title, content.LastModifiedOnDate.ToUniversalTime());
                        searchDocuments.Add(searchDoc);
                        App.Services.Logger.Trace($"Indexing content {modInfo.ModuleID}|{culture} -  OK!  {searchDoc.Title} ({modInfo.TabID}) of {content.LastModifiedOnDate.ToUniversalTime()}");

                        // now do the same with any linked localized instances of this module
                        if (modInfo.LocalizedModules != null)
                            foreach (var localizedModule in modInfo.LocalizedModules)
                            {
                                culture = localizedModule.Value.CultureCode;
                                localizedData = GetLocalizedContent(content.Data, culture);
                                searchDoc = CreateSearchDocument(modInfo, module.Settings, localizedData, content.Id, culture, content.Title, content.LastModifiedOnDate.ToUniversalTime());
                                searchDocuments.Add(searchDoc);
                                App.Services.Logger.Trace($"Indexing content {modInfo.ModuleID}|{culture} -  OK!  {searchDoc.Title} ({modInfo.TabID}) of {content.LastModifiedOnDate.ToUniversalTime()}");
                            }
                    }
                    else
                    {
                        searchDoc = CreateSearchDocument(modInfo, module.Settings, content.Data, content.Id, "", content.Title, content.LastModifiedOnDate.ToUniversalTime());
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

        private static JToken GetLocalizedContent(JToken contentData, string culture)
        {
            JToken retval = contentData.DeepClone(); // clone to prevent from changing the original
            // remove all other culture data
            JsonUtils.SimplifyJson(retval, culture);

            return retval;
        }

        private static SearchDocument CreateSearchDocument(ModuleInfo modInfo, OpenContentSettings settings, JToken contentData, string itemId, string culture, string dataItemTitle, DateTime time)
        {
            // existance of settings.Template.Main has already been checked: we wouldn't be here if it doesn't exist
            // but still, we don't want to count on that too much
            var ps = new PortalSettings(modInfo.PortalID);
            ps.PortalAlias = PortalAliasController.Instance.GetPortalAlias(ps.DefaultPortalAlias);

            string url = null;
            // Check if it is a single or list template 
            if (settings.Template.IsListTemplate && settings.Template.Detail != null)
            {
                url = TestableGlobals.Instance.NavigateURL(modInfo.TabID, ps, "", $"id={itemId}");
            }
            else
            {
                // With a signle template we don't want to identify the content by id.
                url = TestableGlobals.Instance.NavigateURL(modInfo.TabID, ps, "");
            }

            // instanciate the search document
            var retval = new SearchDocument
            {
                UniqueKey = modInfo.ModuleID + "-" + itemId + "-" + culture,
                PortalId = modInfo.PortalID,
                ModifiedTimeUtc = time,
                CultureCode = culture,
                TabId = modInfo.TabID,
                ModuleId = modInfo.ModuleID,
                ModuleDefId = modInfo.ModuleDefID,
                Url = url
            };

            // get the title from the template, if it's there
            if (!string.IsNullOrEmpty(settings.Template?.Main?.DnnSearchTitle))
            {
                var dicForHbs = JsonUtils.JsonToDictionary(contentData.ToString());
                var hbEngine = new HandlebarsEngine();
                retval.Title = hbEngine.ExecuteWithoutFaillure(settings.Template.Main.DnnSearchTitle, dicForHbs, modInfo.ModuleTitle);
            }
            // SK: this is the behaviour before introduction of DnnSearchTitle
            else if (dataItemTitle.IsJson())
            {
                if (contentData["Title"] != null)
                    retval.Title = contentData["Title"].ToString();
                else
                    retval.Title = modInfo.ModuleTitle;
            }
            else
            {
                retval.Title = dataItemTitle;
            }

            // for the search text, we're using the template in DnnSearchText if it's used
            // otherwise, we fall back to previous behaviour:
            // - if the item has a field called Description, we use that
            // - otherwise just get the whole item contents
            if (!string.IsNullOrEmpty(settings.Template?.Main?.DnnSearchText))
            {
                var dicForHbs = JsonUtils.JsonToDictionary(contentData.ToString());
                var hbEngine = new HandlebarsEngine();
                retval.Body = hbEngine.ExecuteWithoutFaillure(settings.Template.Main.DnnSearchText, dicForHbs, modInfo.ModuleTitle);
            }
            else if (contentData["Description"] != null)
            {
                retval.Body = contentData["Description"]?.ToString();
            }
            else
            {
                retval.Body = JsonToSearchableString(contentData);
            }

            // for description, we also try and use the available template first
            // if that's not there, we'll use the body text for the search document
            if (!string.IsNullOrEmpty(settings.Template?.Main?.DnnSearchDescription))
            {
                var dicForHbs = JsonUtils.JsonToDictionary(contentData.ToString());
                var hbEngine = new HandlebarsEngine();
                retval.Description = hbEngine.ExecuteWithoutFaillure(settings.Template.Main.DnnSearchDescription, dicForHbs, modInfo.ModuleTitle);
            }
            else
            {
                retval.Description = retval.Body;
            }

            retval.Title = HttpUtility.HtmlDecode(retval.Title).StripHtml();
            retval.Body = HttpUtility.HtmlDecode(retval.Body).StripHtml();
            retval.Description = HttpUtility.HtmlDecode(retval.Description).StripHtml();

            // Add support for module terms
            if (modInfo.Terms != null && modInfo.Terms.Count > 0)
            {
                retval.Tags = CollectHierarchicalTags(modInfo.Terms);
            }

            return retval;
        }

        private static List<string> CollectHierarchicalTags(List<Term> terms)
        {
            Func<List<Term>, List<string>, List<string>> collectTagsFunc = null;
            collectTagsFunc = (ts, tags) =>
            {
                if (ts != null && ts.Count > 0)
                {
                    foreach (var t in ts)
                    {
                        tags.Add(t.Name);
                        tags.AddRange(collectTagsFunc(t.ChildTerms, new List<string>()));
                    }
                }
                return tags;
            };
            return collectTagsFunc(terms, new List<string>());
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
        //    throw new System.NotImplementedException("The method or operation is not implemented.");
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
                LuceneUtils.IndexAll();
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
