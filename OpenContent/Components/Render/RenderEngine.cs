using DotNetNuke.Entities.Modules;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Json;
using DotNetNuke.Web.Client.ClientResourceManagement;
using System.IO;
using System.Web.UI;
using DotNetNuke.Web.Client;
using Satrabel.OpenContent.Components.Datasource;
using Satrabel.OpenContent.Components.Lucene.Config;
using Satrabel.OpenContent.Components.Alpaca;
using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Razor;
using System.Web.Hosting;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;
using Newtonsoft.Json.Linq;
using IDataSource = Satrabel.OpenContent.Components.Datasource.IDataSource;
using System.Collections;

namespace Satrabel.OpenContent.Components.Render
{
    public class RenderEngine
    {
        private readonly RenderInfo _renderinfo = new RenderInfo();
        private readonly OpenContentModuleInfo _module; // active module (not datasource module)

        public RenderEngine(ModuleInfo viewmodule, IDictionary moduleSettings = null)
        {
            _module = new OpenContentModuleInfo(viewmodule, moduleSettings);
        }

        public RenderInfo Info
        {
            get
            {
                return _renderinfo;
            }
        }

        public OpenContentSettings Settings
        {
            get
            {
                return _module.Settings;
            }
        }
        public string ItemId // For detail view
        {
            get
            {
                return Info.DetailItemId;
            }
            set
            {
                Info.DetailItemId = value;
            }
        }
        public NameValueCollection QueryString { get; set; } // Only for filtering
        public ModuleInstanceContext ModuleContext { get; set; } // Only for Dnn Razor helpers
        public string LocalResourceFile { get; set; } // Only for Dnn Razor helpers
        public void Render(Page page)
        {
            _renderinfo.Template = Settings.Template;
            if (Settings.TabId > 0 && Settings.ModuleId > 0) // other module
            {
                ModuleController mc = new ModuleController();
                _renderinfo.SetDataSourceModule(Settings.TabId, Settings.ModuleId, mc.GetModule(_renderinfo.ModuleId, _renderinfo.TabId, false), null, "");
            }
            else // this module
            {
                _renderinfo.SetDataSourceModule(Settings.TabId, Settings.ModuleId, _module.ViewModule, null, "");
            }
            //start rendering           
            if (Settings.Template != null)
            {
                if (!Settings.Template.DataNeeded())
                {
                    // template without schema & options
                    // render the template with no data
                    _renderinfo.SetData(null, new JObject(), Settings.Data);
                    _renderinfo.OutputString = GenerateOutput(page, _renderinfo.Template.MainTemplateUri(), _renderinfo.DataJson, _renderinfo.SettingsJson, _renderinfo.Template.Main);
                }
                else if (_renderinfo.Template.IsListTemplate)
                {

                    // Multi items template
                    if (string.IsNullOrEmpty(ItemId))
                    {
                        // List template
                        if (_renderinfo.Template.Main != null)
                        {
                            // for list templates a main template need to be defined
                            _renderinfo.Files = _renderinfo.Template.Main;
                            string templateKey = GetDataList(_renderinfo, Settings, _renderinfo.Template.ClientSideData);
                            if (!string.IsNullOrEmpty(templateKey) && _renderinfo.Template.Views != null && _renderinfo.Template.Views.ContainsKey(templateKey))
                            {
                                _renderinfo.Files = _renderinfo.Template.Views[templateKey];
                            }
                            if (!_renderinfo.ShowInitControl)
                            {
                                _renderinfo.OutputString = GenerateListOutput(page, Settings.Template, _renderinfo.Files, _renderinfo.DataList, _renderinfo.SettingsJson);
                            }
                        }
                    }
                    else
                    {
                        LogContext.Log(_module.ViewModule.ModuleID, "RequestContext", "QueryParam Id", ItemId);
                        // detail template
                        if (_renderinfo.Template.Detail != null)
                        {
                            GetDetailData(_renderinfo, _module);
                        }
                        if (_renderinfo.Template.Detail != null && !_renderinfo.ShowInitControl)
                        {
                            _renderinfo.Files = _renderinfo.Template.Detail;
                            _renderinfo.OutputString = GenerateOutput(page, Settings.Template, _renderinfo.Template.Detail, _renderinfo.DataJson, _renderinfo.SettingsJson);
                        }
                        else // if itemid not corresponding to this module or no DetailTemplate present, show list template
                        {
                            // List template
                            if (_renderinfo.Template.Main != null)
                            {
                                // for list templates a main template need to be defined
                                _renderinfo.Files = _renderinfo.Template.Main;
                                string templateKey = GetDataList(_renderinfo, Settings, _renderinfo.Template.ClientSideData);
                                if (!string.IsNullOrEmpty(templateKey) && _renderinfo.Template.Views != null && _renderinfo.Template.Views.ContainsKey(templateKey))
                                {
                                    _renderinfo.Files = _renderinfo.Template.Views[templateKey];
                                }
                                if (!_renderinfo.ShowInitControl)
                                {
                                    _renderinfo.OutputString = GenerateListOutput(page, Settings.Template, _renderinfo.Files, _renderinfo.DataList, _renderinfo.SettingsJson);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // single item template
                    GetSingleData(_renderinfo, Settings);
                    bool settingsNeeded = _renderinfo.Template.SettingsNeeded();
                    //if (!_renderinfo.ShowInitControl && (!settingsNeeded || !string.IsNullOrEmpty(_renderinfo.SettingsJson)))
                    if (!_renderinfo.ShowInitControl)
                    {
                        _renderinfo.OutputString = GenerateOutput(page, _renderinfo.Template.MainTemplateUri(), _renderinfo.DataJson, _renderinfo.SettingsJson, _renderinfo.Template.Main);
                    }
                }
            }
        }

        public void RenderDemoData(Page page)
        {
            TemplateManifest template = _renderinfo.Template;
            if (template != null && template.IsListTemplate)
            {
                // Multi items template
                if (string.IsNullOrEmpty(_renderinfo.DetailItemId))
                {
                    // List template
                    if (template.Main != null)
                    {
                        // for list templates a main template need to be defined
                        _renderinfo.Files = _renderinfo.Template.Main;
                        /*
                        GetDataList(_renderinfo, _viewmodule.Settings, template.ClientSideData);
                        if (!_renderinfo.SettingsMissing)
                        {
                            _renderinfo.OutputString = GenerateListOutput(_renderinfo.Template.Uri().UrlFolder, template.Main, _renderinfo.DataList, _renderinfo.SettingsJson);
                        }
                         */
                    }
                }
            }
            else
            {
                bool demoExist = GetDemoData(_renderinfo, Settings);
                bool settingsNeeded = _renderinfo.Template.SettingsNeeded();

                if (demoExist && _renderinfo.DataExist && (!settingsNeeded || !string.IsNullOrEmpty(_renderinfo.SettingsJson)))
                {
                    _renderinfo.OutputString = GenerateOutput(page, _renderinfo.Template.MainTemplateUri(), _renderinfo.DataJson, _renderinfo.SettingsJson, _renderinfo.Template.Main);
                }
                //too many rendering issues 
                //bool dsDataExist = _datasource.GetOtherModuleDemoData(_info, _info, _viewmodule.Settings);
                //if (dsDataExist)
                //    _info.OutputString = GenerateOutput(_info.Template.Uri(), _info.DataJson, _info.SettingsJson, null);
            }
        }

        public void IncludeResourses(Page page, Control control)
        {
            IncludeResourses(page, _renderinfo.Template);
            if (_renderinfo.Template != null && _renderinfo.Template.ClientSideData)
            {
                DotNetNuke.Framework.ServicesFramework.Instance.RequestAjaxScriptSupport();
                DotNetNuke.Framework.ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            }
            if (_renderinfo.Files != null && _renderinfo.Files.PartialTemplates != null)
            {
                foreach (var item in _renderinfo.Files.PartialTemplates.Where(p => p.Value.ClientSide))
                {
                    var f = new FileUri(_renderinfo.Template.ManifestFolderUri.FolderPath, item.Value.Template);
                    string s = File.ReadAllText(f.PhysicalFilePath);
                    var litPartial = new LiteralControl(s);
                    control.Controls.Add(litPartial);
                }
            }
        }
        private void IncludeResourses(Page page, TemplateManifest template)
        {
            if (template != null)
            {
                //JavaScript.RequestRegistration() 
                //string templateBase = template.FilePath.Replace("$.hbs", ".hbs");
                var cssfilename = new FileUri(Path.ChangeExtension(template.MainTemplateUri().FilePath, "css"));
                if (cssfilename.FileExists)
                {
                    ClientResourceManager.RegisterStyleSheet(page, page.ResolveUrl(cssfilename.UrlFilePath), FileOrder.Css.PortalCss);
                }
                var jsfilename = new FileUri(Path.ChangeExtension(template.MainTemplateUri().FilePath, "js"));
                if (jsfilename.FileExists)
                {
                    ClientResourceManager.RegisterScript(page, page.ResolveUrl(jsfilename.UrlFilePath), FileOrder.Js.DefaultPriority + 100);
                }
                ClientResourceManager.RegisterScript(page, page.ResolveUrl("~/DesktopModules/OpenContent/js/opencontent.js"), FileOrder.Js.DefaultPriority);
            }
        }

        #region Data

        public string GetDataList(RenderInfo info, OpenContentSettings settings, bool clientSide)
        {
            string templateKey = "";
            info.ResetData();

            IDataSource ds = DataSourceManager.GetDataSource(Settings.Manifest.DataSource);
            var dsContext = OpenContentUtils.CreateDataContext(_module);

            IEnumerable<IDataItem> resultList = new List<IDataItem>();
            if (clientSide || !info.Files.DataInTemplate)
            {
                if (ds.Any(dsContext))
                {
                    info.SetData(resultList, settings.Data);
                    info.DataExist = true;
                }

                if (info.Template.Views != null)
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(info.Template);
                    templateKey = GetTemplateKey(indexConfig);
                }
            }
            else
            {
                //server side
                bool useLucene = info.Template.Manifest.Index;
                if (useLucene)
                {
                    PortalSettings portalSettings = PortalSettings.Current;
                    var indexConfig = OpenContentUtils.GetIndexConfig(info.Template);
                    if (info.Template.Views != null)
                    {
                        templateKey = GetTemplateKey(indexConfig);
                    }
                    bool isEditable = _module.ViewModule.CheckIfEditable(portalSettings);//portalSettings.UserMode != PortalSettings.Mode.Edit;
                    QueryBuilder queryBuilder = new QueryBuilder(indexConfig);
                    queryBuilder.Build(settings.Query, !isEditable, portalSettings.UserId, DnnLanguageUtils.GetCurrentCultureCode(), portalSettings.UserInfo.Social.Roles, QueryString);

                    resultList = ds.GetAll(dsContext, queryBuilder.Select).Items;
                    if (LogContext.IsLogActive)
                    {
                        //LogContext.Log(_module.ModuleID, "RequestContext", "EditMode", !addWorkFlow);
                        LogContext.Log(_module.ViewModule.ModuleID, "RequestContext", "IsEditable", isEditable);
                        LogContext.Log(_module.ViewModule.ModuleID, "RequestContext", "UserRoles", portalSettings.UserInfo.Social.Roles.Select(r => r.RoleName));
                        LogContext.Log(_module.ViewModule.ModuleID, "RequestContext", "CurrentUserId", portalSettings.UserId);
                        var logKey = "Query";
                        LogContext.Log(_module.ViewModule.ModuleID, logKey, "select", queryBuilder.Select);
                        //LogContext.Log(_module.ModuleID, logKey, "result", resultList);
                    }
                    //Log.Logger.DebugFormat("Query returned [{0}] results.", total);
                    if (!resultList.Any())
                    {
                        /*
                         if (ds.Any(dsContext) && settings.Query.IsEmpty())
                         {
                             //there seems to be data in de database, but we did not find it in Lucene, so probably the data isn't indexed anymore/yet
                             //Components.Lucene.LuceneController.Instance.ReIndexModuleData(_module.ViewModule.ModuleID, settings);
                         }
                          */
                        //Log.Logger.DebugFormat("Query did not return any results. API request: [{0}], Lucene Filter: [{1}], Lucene Query:[{2}]", settings.Query, queryDef.Filter == null ? "" : queryDef.Filter.ToString(), queryDef.Query == null ? "" : queryDef.Query.ToString());
                        if (ds.Any(dsContext))
                        {
                            info.SetData(resultList, settings.Data);
                            info.DataExist = true;
                        }
                    }
                }
                else
                {
                    resultList = ds.GetAll(dsContext, null).Items;
                    if (LogContext.IsLogActive)
                    {
                        var logKey = "Get all data of module";
                        //LogContext.Log(_module.ModuleID, logKey, "result", resultList);
                    }
                }
                if (resultList.Any())
                {
                    info.SetData(resultList, settings.Data);
                }
            }
            return templateKey;
        }

        public void GetDetailData(RenderInfo info, OpenContentModuleInfo module)
        {
            info.ResetData();
            var ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
            var dsContext = OpenContentUtils.CreateDataContext(module);

            var dsItem = ds.Get(dsContext, info.DetailItemId);
            if (LogContext.IsLogActive)
            {
                var logKey = "Get detail data";
                //LogContext.Log(_module.ModuleID, logKey, "debuginfo", dsItems.DebugInfo);
            }

            if (dsItem != null)
            {
                //check permissions
                var portalSettings = PortalSettings.Current;
                bool isEditable = _module.ViewModule.CheckIfEditable(portalSettings);
                if (!isEditable)
                {
                    var indexConfig = OpenContentUtils.GetIndexConfig(info.Template);
                    string raison;
                    if (!OpenContentUtils.HaveViewPermissions(dsItem, portalSettings.UserInfo, indexConfig, out raison))
                    {
                        if (module.ViewModule.HasEditRightsOnModule())
                            Exceptions.ProcessHttpException(new NotAuthorizedException(404, $"No detail view permissions for id={info.DetailItemId}  (due to {raison}) \nGo into Edit Mode to view/change the item"));
                        else
                            Exceptions.ProcessHttpException(new NotAuthorizedException(404, "Access denied. You might want to contact your administrator for more information."));
                        //throw new UnauthorizedAccessException("No detail view permissions for id " + info.DetailItemId);
                    }
                }
                info.SetData(dsItem, dsItem.Data, module.Settings.Data);
            }
        }

        public void GetSingleData(RenderInfo info, OpenContentSettings settings)
        {
            info.ResetData();

            IDataSource ds = DataSourceManager.GetDataSource(Settings.Manifest.DataSource);
            var dsContext = OpenContentUtils.CreateDataContext(_module, -1, true);

            var dsItem = ds.Get(dsContext, null);
            if (dsItem != null)
            {
                info.SetData(dsItem, dsItem.Data, settings.Data);
            }
        }

        public bool GetDemoData(RenderInfo info, OpenContentSettings settings)
        {
            info.ResetData();
            //bool settingsNeeded = false;
            FileUri dataFilename = null;
            if (info.Template != null)
            {
                dataFilename = new FileUri(info.Template.ManifestFolderUri.UrlFolder, "data.json"); ;
            }
            if (dataFilename != null && dataFilename.FileExists)
            {
                string fileContent = File.ReadAllText(dataFilename.PhysicalFilePath);
                string settingContent = "";
                if (!string.IsNullOrWhiteSpace(fileContent))
                {
                    if (settings.Template != null && info.Template.MainTemplateUri().FilePath == settings.Template.MainTemplateUri().FilePath)
                    {
                        settingContent = settings.Data;
                    }
                    if (string.IsNullOrEmpty(settingContent))
                    {
                        var settingsFilename = info.Template.MainTemplateUri().PhysicalFullDirectory + "\\" + info.Template.Key.ShortKey + "-data.json";
                        if (File.Exists(settingsFilename))
                        {
                            settingContent = File.ReadAllText(settingsFilename);
                        }
                        else
                        {
                            //string schemaFilename = info.Template.Uri().PhysicalFullDirectory + "\\" + info.Template.Key.ShortKey + "-schema.json";
                            //settingsNeeded = File.Exists(schemaFilename);
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(fileContent))
                    info.SetData(null, fileContent, settingContent);
            }
            return !info.ShowInitControl; //!string.IsNullOrWhiteSpace(info.DataJson) && (!string.IsNullOrWhiteSpace(info.SettingsJson) || !settingsNeeded);
        }
        private string GetTemplateKey(FieldConfig IndexConfig)
        {
            string templateKey = "";
            if (QueryString != null)
            {
                foreach (string key in QueryString)
                {
                    if (IndexConfig != null && IndexConfig.Fields != null && IndexConfig.Fields.Any(f => f.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var indexConfig = IndexConfig.Fields.Single(f => f.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                        string val = QueryString[key];
                        if (string.IsNullOrEmpty(templateKey))
                            templateKey = key;
                        else
                            templateKey += "-" + key;
                    }
                }
            }
            return templateKey;
        }

        #endregion

        #region ExecuteTemplates
        private string ExecuteRazor(FileUri template, dynamic model)
        {
            string webConfig = template.PhysicalFullDirectory;
            webConfig = webConfig.Remove(webConfig.LastIndexOf("\\")) + "\\web.config";
            if (!File.Exists(webConfig))
            {
                string filename = HostingEnvironment.MapPath("~/DesktopModules/OpenContent/Templates/web.config");
                File.Copy(filename, webConfig);
            }
            var writer = new StringWriter();
            try
            {
                var razorEngine = new RazorEngine("~/" + template.FilePath, ModuleContext, LocalResourceFile);
                razorEngine.Render(writer, model);
            }
            catch (Exception ex)
            {
                LoggingUtils.RenderEngineException(this, ex);
                string stack = string.Join("\n", ex.StackTrace.Split('\n').Where(s => s.Contains("\\Portals\\") && s.Contains("in")).Select(s => s.Substring(s.IndexOf("in"))).ToArray());
                throw new TemplateException("Failed to render Razor template " + template.FilePath + "\n" + stack, ex, model, template.FilePath);
            }
            return writer.ToString();
        }
        private string ExecuteTemplate(Page page, TemplateManifest templateManifest, TemplateFiles files, FileUri templateUri, dynamic model)
        {
            var templateVirtualFolder = templateManifest.ManifestFolderUri.UrlFolder;
            if (LogContext.IsLogActive)
            {
                var logKey = "Render template";
                LogContext.Log(_module.ViewModule.ModuleID, logKey, "template", templateUri.FilePath);
                LogContext.Log(_module.ViewModule.ModuleID, logKey, "model", model);
            }
            if (templateUri.Extension != ".hbs")
            {
                return ExecuteRazor(templateUri, model);
            }
            else
            {
                HandlebarsEngine hbEngine = new HandlebarsEngine();
                return hbEngine.Execute(page, files, templateVirtualFolder, model);
            }
        }

        #endregion

        #region Generate output

        private string GenerateOutput(Page page, TemplateManifest templateManifest, TemplateFiles files, JToken dataJson, string settingsJson)
        {
            var templateVirtualFolder = templateManifest.ManifestFolderUri.UrlFolder;
            if (!string.IsNullOrEmpty(files.Template))
            {
                string physicalTemplateFolder = HostingEnvironment.MapPath(templateVirtualFolder);
                FileUri templateUri = CheckFiles(templateManifest, files);

                if (dataJson != null)
                {
                    var mf = new ModelFactorySingle(_renderinfo.Data, settingsJson, physicalTemplateFolder, _renderinfo.Template.Manifest, _renderinfo.Template, files, _module, PortalSettings.Current);
                    dynamic model = mf.GetModelAsDynamic();
                    if (!string.IsNullOrEmpty(_renderinfo.Template.Manifest.DetailMetaTitle))
                    {
                        HandlebarsEngine hbEngine = new HandlebarsEngine();
                        page.Title = hbEngine.Execute(_renderinfo.Template.Manifest.DetailMetaTitle, model);
                    }
                    if (!string.IsNullOrEmpty(_renderinfo.Template.Manifest.DetailMetaDescription))
                    {
                        HandlebarsEngine hbEngine = new HandlebarsEngine();
                        PageUtils.SetPageDescription(page, hbEngine.Execute(_renderinfo.Template.Manifest.DetailMetaDescription, model));
                    }
                    if (!string.IsNullOrEmpty(_renderinfo.Template.Manifest.DetailMeta))
                    {
                        HandlebarsEngine hbEngine = new HandlebarsEngine();
                        PageUtils.SetPageMeta(page, hbEngine.Execute(_renderinfo.Template.Manifest.DetailMeta, model));
                    }
                    return ExecuteTemplate(page, templateManifest, files, templateUri, model);
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        private string GenerateOutput(Page page, FileUri template, JToken dataJson, string settingsJson, TemplateFiles files)
        {
            var ps = PortalSettings.Current;
            if (template != null)
            {
                string templateVirtualFolder = template.UrlFolder;
                string physicalTemplateFolder = HostingEnvironment.MapPath(templateVirtualFolder);
                if (dataJson != null)
                {
                    ModelFactorySingle mf;

                    if (_renderinfo.Data == null)
                    {
                        // demo data
                        mf = new ModelFactorySingle(_renderinfo.DataJson, settingsJson, physicalTemplateFolder, _renderinfo.Template.Manifest, _renderinfo.Template, files, _module, ps);
                    }
                    else
                    {
                        mf = new ModelFactorySingle(_renderinfo.Data, settingsJson, physicalTemplateFolder, _renderinfo.Template.Manifest, _renderinfo.Template, files, _module, ps);
                    }
                    dynamic model = mf.GetModelAsDynamic();
                    if (LogContext.IsLogActive)
                    {
                        var logKey = "Render single item template";
                        LogContext.Log(_module.ViewModule.ModuleID, logKey, "template", template.FilePath);
                        LogContext.Log(_module.ViewModule.ModuleID, logKey, "model", model);
                    }

                    if (template.Extension != ".hbs")
                    {
                        return ExecuteRazor(template, model);
                    }
                    else
                    {
                        HandlebarsEngine hbEngine = new HandlebarsEngine();
                        return hbEngine.Execute(page, template, model);
                    }
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        private string GenerateListOutput(Page page, TemplateManifest templateManifest, TemplateFiles files, IEnumerable<IDataItem> dataList, string settingsJson)
        {
            var templateVirtualFolder = templateManifest.ManifestFolderUri.UrlFolder;
            if (!string.IsNullOrEmpty(files.Template))
            {
                string physicalTemplateFolder = HostingEnvironment.MapPath(templateVirtualFolder);
                FileUri templateUri = CheckFiles(templateManifest, files);
                if (dataList != null)
                {
                    ModelFactoryMultiple mf = new ModelFactoryMultiple(dataList, settingsJson, physicalTemplateFolder, _renderinfo.Template.Manifest, _renderinfo.Template, files, _module, PortalSettings.Current);
                    dynamic model = mf.GetModelAsDynamic();
                    return ExecuteTemplate(page, templateManifest, files, templateUri, model);
                }
            }
            return "";
        }



        private FileUri CheckFiles(TemplateManifest templateManifest, TemplateFiles files)
        {
            if (files == null)
            {
                throw new Exception("Manifest.json missing or incomplete");
            }
            var templateUri = new FileUri(templateManifest.ManifestFolderUri, files.Template);
            if (!templateUri.FileExists)
            {
                throw new Exception("Template " + templateUri.UrlFilePath + " don't exist");
            }
            if (files.PartialTemplates != null)
            {
                foreach (var partial in files.PartialTemplates)
                {
                    var partialTemplateUri = new FileUri(templateManifest.ManifestFolderUri, partial.Value.Template);
                    if (!partialTemplateUri.FileExists)
                        throw new Exception("PartialTemplate " + partialTemplateUri.UrlFilePath + " don't exist");
                }
            }
            return templateUri;
        }
        #endregion

    }
}