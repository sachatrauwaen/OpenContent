﻿using System;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web.UI;
using DotNetNuke.UI.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;
using Satrabel.OpenContent.Components.TemplateHelpers;
using Satrabel.OpenContent.Components.Render;


namespace Satrabel.OpenContent.Components.Alpaca
{
    public class AlpacaEngine
    {
        private string _virtualDirectory;

        private string VirtualDirectory
        {
            get
            {
                return _virtualDirectory;
            }
            set
            {
                _virtualDirectory = value.TrimStart('\\');
            }
        }

        private string Prefix { get; set; }

        private IPageContext Page { get; set; }
        private int PortalId { get; set; }

        public AlpacaEngine(Page page, int portalId, string virtualDir, string filePrefix)
        {
            this.Page = new WebFormsPageContext(page, page);
            this.PortalId = portalId;
            VirtualDirectory = virtualDir;
            Prefix = filePrefix;
        }

        public AlpacaEngine(IPageContext page, int portalId, string virtualDir, string filePrefix)
        {
            this.Page = page;
            this.PortalId = portalId;
            VirtualDirectory = virtualDir;
            Prefix = filePrefix;
        }

        [Obsolete("This method is obsolete since v3.2.0 (jan 2017); use AlpacaEngine(Page page, int portalId, string virtualDir, string filePrefix) instead")]
        public AlpacaEngine(IPageContext page, ModuleInstanceContext moduleContext, string virtualDir, string filePrefix)
        {
            this.Page = page;
            this.PortalId = moduleContext.PortalId;
            VirtualDirectory = virtualDir;
            Prefix = filePrefix;
        }

        public void RegisterAll() // for openform
        {
            RegisterAll(false, false, false, false);
        }

        public void RegisterAll(bool bootstrap) // for openform
        {
            RegisterAll(bootstrap, false, false, false);
        }

        public void RegisterAll(bool bootstrapLayoutEnabled, bool loadBootstrap) // for openform
        {
            RegisterAll(bootstrapLayoutEnabled, loadBootstrap, false, false);
        }

        [Obsolete("Use RegisterAll with 4 params")]
        public void RegisterAll(bool bootstrapLayoutEnabled, bool loadBootstrap, bool loadGlyphicons)
        {
            RegisterAll(bootstrapLayoutEnabled, loadBootstrap, loadGlyphicons, false);
        }

        public void RegisterAll(bool bootstrapLayoutEnabled, bool loadBootstrap, bool loadGlyphicons, bool builderV2)
        {
            RegisterAlpaca(bootstrapLayoutEnabled, loadBootstrap, loadGlyphicons, builderV2);
            RegisterTemplates();
            RegisterScripts(bootstrapLayoutEnabled);
            if (!builderV2)
            {
                RegisterFields(bootstrapLayoutEnabled);
            }
            else
            {
                RegisterStartupScript("oc-ckdnnporid", $@"<input type=""hidden"" id=""CKDNNporid"" value=""{PortalId}"">", false);
            }
        }

        private void RegisterAlpaca(bool bootstrap, bool loadBootstrap, bool loadGlyphicons, bool builderV2)
        {
            if (loadBootstrap)
            {
                if (builderV2)
                {
                    Page.RegisterScript("~/DesktopModules/OpenContent/js/bootstrap4/js/bootstrap.min.js", FileOrder.Js.DefaultPriority);
                    Page.RegisterStyleSheet("~/DesktopModules/OpenContent/js/bootstrap4/css/bootstrap.min.css", FileOrder.Css.DefaultPriority);
                }
                else
                {
                    Page.RegisterScript("~/DesktopModules/OpenContent/js/bootstrap/js/bootstrap.min.js", FileOrder.Js.DefaultPriority);
                    Page.RegisterStyleSheet( "~/DesktopModules/OpenContent/js/bootstrap/css/bootstrap.min.css", FileOrder.Css.DefaultPriority);
                }
            }


            if (loadGlyphicons)
            {
                Page.RegisterStyleSheet("~/DesktopModules/OpenContent/js/bootstrap/css/glyphicons.css", FileOrder.Css.DefaultPriority);
            }
            //ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/OpenContent/css/font-awesome/css/font-awesome.min.css", FileOrder.Css.DefaultPriority);
            if (builderV2)
            {
                //ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/lamavue/0.js", FileOrder.Js.DefaultPriority + 10);
                Page.RegisterScript("~/DesktopModules/OpenContent/lama/dist/js/chunk-vendors.js", FileOrder.Js.DefaultPriority + 10);
                Page.RegisterScript("~/DesktopModules/OpenContent/lama/dist/js/app.js", FileOrder.Js.DefaultPriority + 10);
                Page.RegisterScript("~/DesktopModules/OpenContent/js/lamaengine.js", FileOrder.Js.DefaultPriority + 10);
            }
            else
            {
                Page.RegisterScript("~/DesktopModules/OpenContent/js/lib/handlebars/handlebars.js", FileOrder.Js.DefaultPriority);
                Page.RegisterScript("~/DesktopModules/OpenContent/js/lib/typeahead.js/dist/typeahead.bundle.min.js", FileOrder.Js.DefaultPriority);

                Page.RegisterScript("~/DesktopModules/OpenContent/js/wysihtml/wysihtml-toolbar.min.js", FileOrder.Js.DefaultPriority + 1);
                Page.RegisterScript("~/DesktopModules/OpenContent/js/wysihtml/parser_rules/advanced_opencontent.js", FileOrder.Js.DefaultPriority + 1);
                if (bootstrap)
                {
                    Page.RegisterStyleSheet("~/DesktopModules/OpenContent/js/alpaca/bootstrap/alpaca.css", FileOrder.Css.DefaultPriority);
                    Page.RegisterStyleSheet("~/DesktopModules/OpenContent/alpaca/css/alpaca-dnnbootstrap.css", FileOrder.Css.DefaultPriority);
                    Page.RegisterScript("~/DesktopModules/OpenContent/js/alpaca/bootstrap/alpaca.min.js", FileOrder.Js.DefaultPriority + 1);
                    Page.RegisterScript("~/DesktopModules/OpenContent/alpaca/js/views/dnnbootstrap.js", FileOrder.Js.DefaultPriority + 2);
                }
                else
                {
                    Page.RegisterStyleSheet("~/DesktopModules/OpenContent/alpaca/css/alpaca-dnn.css", FileOrder.Css.DefaultPriority);
                    Page.RegisterScript("~/DesktopModules/OpenContent/js/alpaca/web/alpaca.min.js", FileOrder.Js.DefaultPriority + 1);
                    Page.RegisterScript("~/DesktopModules/OpenContent/alpaca/js/views/dnn.js", FileOrder.Js.DefaultPriority + 2);
                }
                Page.RegisterScript("~/DesktopModules/OpenContent/alpaca/js/fields/dnn/dnnfields.js", FileOrder.Js.DefaultPriority + 3);

                Page.RegisterScript("~/DesktopModules/OpenContent/js/alpacaengine.js", FileOrder.Js.DefaultPriority + 10);
            }

            Page.RegisterStyleSheet("~/DesktopModules/OpenContent/css/font-awesome/css/font-awesome.min.css", FileOrder.Css.DefaultPriority + 1);


            // Register Edit.js and css in the template directory..
            string trimmedPath = VirtualDirectory.TrimEnd('/');
            int lastSlash = trimmedPath.LastIndexOf('/');

            if (lastSlash > -1)
            {
                // Get the parent directory (one level up)
                string parentVirtualPath = trimmedPath.Substring(0, lastSlash);

                // Check if the parent directory contains 'templates', if so register files from it
                if (parentVirtualPath.EndsWith("templates", StringComparison.OrdinalIgnoreCase))
                {
                    // Always register from the "templates" folder
                    RegisterEditFiles(parentVirtualPath); // This will register from "templates"
                }
            }

            // Register Edit.js and css for this template (loaded later so you can derive from the central definitions)
            RegisterEditFiles(VirtualDirectory);


        }

        /// <summary>
        /// Registers both `edit.js` and `edit.css` files if they exist in the specified physical directory.
        /// </summary>
        /// <param name="virtualPath">The physical file system path to the directory containing the files.</param>
        public void RegisterEditFiles(string virtualPath)
        {
            RegisterEditFile(virtualPath, "edit.js");
            RegisterEditFile(virtualPath, "edit.css");
        }

        /// <summary>
        /// Registers a single JS or CSS file if it exists at the specified virtual path.
        /// </summary>
        /// <param name="virtualPath">The virtual (URL) path relative to the application root, e.g., "Templates/MyTemplate".</param>
        /// <param name="fileName">The name of the file to register (e.g., "edit.js" or "edit.css").</param>
        public void RegisterEditFile(string virtualPath, string fileName)
        {
            // Map the virtual path to a physical path
            string physicalPath = HostingEnvironment.MapPath("~/" + virtualPath);
            if (physicalPath == null) return; // Safety check in case the path doesn't resolve

            string filePath = Path.Combine(physicalPath, fileName);

            if (File.Exists(filePath))
            {
                string relativePath = $"~/{virtualPath.TrimEnd('/')}/{fileName}";

                if (fileName.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                {
                    Page.RegisterScript(relativePath, FileOrder.Js.DefaultPriority + 11);
                }
                else if (fileName.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                {
                    Page.RegisterStyleSheet(relativePath, FileOrder.Css.DefaultPriority + 11);
                }
            }
        }

        public void RegisterTemplates()
        {
            /*
            var body = (HtmlGenericControl)Page.FindControl("Body");
            if (body.FindControl("oc-dnntemplates") == null)
            {
                string templates = File.ReadAllText(HostingEnvironment.MapPath("~/DesktopModules/OpenContent/alpaca/templates/dnn-edit/dnntemplates.html"));
                var lit = new LiteralControl(templates);
                lit.ID = "oc-dnntemplates";

                body.Controls.Add(lit);
            }
            */

            string templates = File.ReadAllText(HostingEnvironment.MapPath("~/DesktopModules/OpenContent/alpaca/templates/dnn-edit/dnntemplates.html"));
            RegisterStartupScript("oc-dnntemplates", templates, false);
        }

        public void RegisterScripts(bool bootstrap)
        {
            ServicesFramework.Instance.RequestAjaxScriptSupport();
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            if (!bootstrap)
            {
                JavaScript.RequestRegistration(CommonJs.DnnPlugins); // dnnPanels
            }
            JavaScript.RequestRegistration(CommonJs.jQueryFileUpload); // image file upload
            Page.RegisterClientVariable( "oc_websiteRoot", FileUri.NormalizedApplicationPath, true);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins); // avoid js error TypeError: $(...).jScrollPane is not a function
        }

        private void RegisterFields(bool bootstrap)
        {
            //bool allFields = string.IsNullOrEmpty(VirtualDirectory);
            bool allFields = Prefix == "builder";
            List<string> fieldTypes = new List<string>();
            if (!allFields)
            {
                JToken options = GetOptions();
                if (options != null)
                {
                    fieldTypes = FieldTypes(options);
                }
            }
            if (allFields || fieldTypes.Contains("address") || fieldTypes.Contains("mladdress"))
            {
                string apikey = App.Services.CreateGlobalSettingsRepository(PortalId).GetGoogleApiKey();
                Page.RegisterScript( "//maps.googleapis.com/maps/api/js?loading=async&libraries=places" + (string.IsNullOrEmpty(apikey) ? "" : "&key=" + apikey), FileOrder.Js.DefaultPriority);
            }
            if (allFields || fieldTypes.ContainsAny("imagecropper", "imagecrop", "imagecrop2", "imagex", "mlimagex"))
            {
                Page.RegisterScript("~/DesktopModules/OpenContent/js/cropper/cropper.js", FileOrder.Js.DefaultPriority);
                Page.RegisterStyleSheet("~/DesktopModules/OpenContent/js/cropper/cropper.css", FileOrder.Css.DefaultPriority);
            }
            if (allFields || fieldTypes.ContainsAny("select2", "image2", "file2", "url2", "mlimage2", "mlfile2", "mlurl2", "mlfolder2", "imagecrop2", "role2", "user2", "imagex", "mlimagex"))
            {
                Page.RegisterScript("~/DesktopModules/OpenContent/js/select2/select2.js", FileOrder.Js.DefaultPriority);
                Page.RegisterStyleSheet("~/DesktopModules/OpenContent/js/select2/select2.css", FileOrder.Css.DefaultPriority);
                if (bootstrap)
                {
                    Page.RegisterStyleSheet("~/DesktopModules/OpenContent/js/select2/select2-bootstrap.min.css", FileOrder.Css.DefaultPriority + 1);
                }
            }

            //<!-- bootstrap datetimepicker for date, time and datetime controls -->
            //<dnncl:DnnJsInclude ID="DnnJsInclude13" runat="server" FilePath="~/DesktopModules/OpenContent/js/lib/moment/min/moment-with-locales.min.js" Priority="106" ForceProvider="DnnPageHeaderProvider" />
            //<dnncl:DnnJsInclude ID="DnnJsInclude14" runat="server" FilePath="~/DesktopModules/OpenContent/js/lib/eonasdan-bootstrap-datetimepicker/build/js/bootstrap-datetimepicker.min.js" Priority="106" ForceProvider="DnnPageHeaderProvider" />
            //<dnncl:DnnCssInclude ID="DnnCssInclude2" runat="server" FilePath="~/DesktopModules/OpenContent/js/lib/eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.css" AddTag="false" />

            if (allFields || fieldTypes.Contains("date") || fieldTypes.Contains("datetime") || fieldTypes.Contains("time"))
            {
                Page.RegisterScript( "~/DesktopModules/OpenContent/js/lib/moment/min/moment-with-locales.min.js", FileOrder.Js.DefaultPriority, "DnnPageHeaderProvider");
                Page.RegisterScript( "~/DesktopModules/OpenContent/js/lib/eonasdan-bootstrap-datetimepicker/build/js/bootstrap-datetimepicker.min.js", FileOrder.Js.DefaultPriority + 1, "DnnPageHeaderProvider");
                Page.RegisterStyleSheet("~/DesktopModules/OpenContent/js/lib/eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.min.css", FileOrder.Css.DefaultPriority);
            }
            if (allFields || fieldTypes.Contains("ckeditor") || fieldTypes.Contains("mlckeditor"))
            {
                Page.RegisterScript("~/DesktopModules/OpenContent/js/CKEditor/ckeditor.js", FileOrder.Js.DefaultPriority);
                /*
                ClientScriptManager cs = Page.ClientScript;
                Type csType = GetType();
                const string CsName = "CKEdScript";
                if (!cs.IsClientScriptIncludeRegistered(csType, CsName))
                {
                    cs.RegisterClientScriptInclude(
                        csType, CsName, Page.ResolveUrl("~/DesktopModules/OpenContent/js/CKEditor/ckeditor.js"));
                }
                */

                Page.RegisterClientVariable("PortalId", PortalId.ToString(), true);
                /*
                var CKDNNporid = new HiddenField();
                CKDNNporid.ID = "CKDNNporid";
                CKDNNporid.ClientIDMode = ClientIDMode.Static;
                form.Controls.Add(CKDNNporid);
                CKDNNporid.Value = PortalId.ToString();
                */
                RegisterStartupScript("oc-ckdnnporid", $@"<input type=""hidden"" id=""CKDNNporid"" value=""{PortalId}"">", false);
                GenerateEditorLoadScript(PortalId);


            }
            if (allFields || fieldTypes.Contains("icon"))
            {
                Page.RegisterScript( "~/DesktopModules/OpenContent/js/fontIconPicker/js/jquery.fonticonpicker.js", FileOrder.Js.DefaultPriority, "DnnPageHeaderProvider");
                Page.RegisterStyleSheet("~/DesktopModules/OpenContent/js/fontIconPicker/css/base/jquery.fonticonpicker.min.css", FileOrder.Css.DefaultPriority);
                //if (bootstrap)
                //    ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/OpenContent/js/fontIconPicker/css/themes/bootstrap-theme/jquery.fonticonpicker.bootstrap.min.css", FileOrder.Css.DefaultPriority);
                //else
                Page.RegisterStyleSheet("~/DesktopModules/OpenContent/js/fontIconPicker/css/themes/grey-theme/jquery.fonticonpicker.grey.min.css", FileOrder.Css.DefaultPriority);
                Page.RegisterStyleSheet("~/DesktopModules/OpenContent/css/glyphicons/glyphicons.css", FileOrder.Css.DefaultPriority + 1);
            }
            if (allFields || fieldTypes.Contains("summernote") || fieldTypes.Contains("mlsummernote"))
            {
                Page.RegisterScript( "~/DesktopModules/OpenContent/js/summernote/summernote.min.js", FileOrder.Js.DefaultPriority, "DnnPageHeaderProvider");
                Page.RegisterStyleSheet("~/DesktopModules/OpenContent/js/summernote/summernote.css", FileOrder.Css.DefaultPriority);
            }
        }

        private static bool CKEditorDnnConnectIsInstalled => File.Exists(HostingEnvironment.MapPath("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.5.3/ckeditor.js"));

        private static bool CKEditorIngoThaWatchaIsInstalled => File.Exists(HostingEnvironment.MapPath("~/Providers/HtmlEditorProviders/CKEditor/ckeditor.js"));

        private JToken GetOptions()
        {
            string prefix = (string.IsNullOrEmpty(Prefix) ? "" : $"{Prefix}-");
            string physicalDirectory = HostingEnvironment.MapPath("~/" + VirtualDirectory);
            string optionsFilename = physicalDirectory + "\\" + $"{prefix}options.json";
            // default options
            JToken optionsJson = JsonUtils.LoadJsonFileFromDisk(optionsFilename);

            // language options
            optionsFilename = physicalDirectory + "\\" + $"{prefix}options.{DnnLanguageUtils.GetCurrentCultureCode()}.json";
            JToken languageOptionsJson = JsonUtils.LoadJsonFileFromDisk(optionsFilename);
            optionsJson = optionsJson.JsonMerge(languageOptionsJson);

            return optionsJson;
        }

        private static List<string> FieldTypes(JToken options)
        {
            var types = new List<string>();
            var fields = options["fields"];
            if (fields != null)
            {
                foreach (JProperty fieldProp in fields.Children())
                {
                    var field = fieldProp.First();
                    var fieldtype = field["type"];
                    if (fieldtype != null)
                    {
                        types.Add(fieldtype.ToString());
                    }
                    var subtypes = FieldTypes(field);
                    types.AddRange(subtypes);
                }
            }
            else if (options["items"] != null)
            {
                if (options["items"]["type"] != null)
                {
                    var fieldtype = options["items"]["type"] as JValue;
                    types.Add(fieldtype.Value.ToString());
                }
                var subtypes = FieldTypes(options["items"]);
                types.AddRange(subtypes);
            }
            return types;
        }

        public static string AlpacaCulture(string cultureCode)
        {
            string[] alpacaLocales = { "zh_CN", "hr_HR", "fr_FR", "de_AT", "it_IT", "ja_JP", "pl_PL", "pt_BR", "es_ES", "nl_BE" };
            string lang = cultureCode.Replace("-", "_");
            foreach (var item in alpacaLocales)
            {
                if (item == lang)
                {
                    return lang;
                }
            }
            string lang2 = lang.Substring(0, 2);
            foreach (var item in alpacaLocales)
            {
                if (item.Substring(0, 2) == lang2)
                {
                    return item;
                }
            }
            return "en_US";
        }

        /// <summary>
        /// Generates the editor load script.
        /// </summary>
        private void GenerateEditorLoadScript(int portalId)
        {
            //var editorVar = string.Format(
            //    "editor{0}",
            //    ClientID.Substring(ClientID.LastIndexOf("_", StringComparison.Ordinal) + 1).Replace(
            //        "-", string.Empty));

            //var editorFixedId = ClientID.Replace("-", string.Empty).Replace(".", string.Empty);
            var editorFixedId = "opencontentCK";

            ////if (HasMsAjax)
            ////{
            //var postBackScript =
            //    string.Format(
            //        @"if (CKEDITOR && CKEDITOR.instances && CKEDITOR.instances.{0}) {{ CKEDITOR.instances.{0}.updateElement(); CKEDITOR.instances.{0}.destroy(); }}",
            //        editorFixedId);

            //RegisterOnSubmitStatement(
            //    GetType(), string.Format("CKEditor_OnAjaxSubmit_{0}", editorFixedId), postBackScript);
            ////}

            var editorScript = new StringBuilder();

            //editorScript.AppendFormat(
            //    "Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(LoadCKEditorInstance_{0});", editorFixedId);

            //editorScript.AppendFormat("function LoadCKEditorInstance_{0}(sender,args) {{", editorFixedId);

            //editorScript.AppendFormat(
            //    @"if (jQuery(""[id*='UpdatePanel']"").length == 0 && CKEDITOR && CKEDITOR.instances && CKEDITOR.instances.{0}) {{ CKEDITOR.instances.{0}.updateElement();}}",
            //    editorFixedId);

            //editorScript.AppendFormat(
            //    "if (document.getElementById('{0}') == null){{return;}}",
            //    editorFixedId);

            //// Render EditorConfig
            //var editorConfigScript = new StringBuilder();
            //editorConfigScript.AppendFormat("var editorConfig{0} = {{", editorVar);

            //var keysCount = Settings.Keys.Count;
            //var currentCount = 0;

            //// Write options
            //foreach (string key in Settings.Keys)
            //{
            //    var value = Settings[key];

            //    currentCount++;

            //    // Is boolean state or string
            //    if (value.Equals("true", StringComparison.InvariantCultureIgnoreCase)
            //        || value.Equals("false", StringComparison.InvariantCultureIgnoreCase) || value.StartsWith("[")
            //        || value.StartsWith("{") || Utility.IsNumeric(value))
            //    {
            //        if (value.Equals("True"))
            //        {
            //            value = "true";
            //        }
            //        else if (value.Equals("False"))
            //        {
            //            value = "false";
            //        }

            //        editorConfigScript.AppendFormat("{0}:{1}", key, value);

            //        editorConfigScript.Append(currentCount == keysCount ? "};" : ",");
            //    }
            //    else
            //    {
            //        if (key == "browser")
            //        {
            //            continue;
            //        }

            //        editorConfigScript.AppendFormat("{0}:\'{1}\'", key, value);

            //        editorConfigScript.Append(currentCount == keysCount ? "};" : ",");
            //    }
            //}

            //editorScript.AppendFormat(
            //    "if (CKEDITOR.instances.{0}){{return;}}",
            //    editorFixedId);

            //// Check if we can use jQuery or $, and if both fail use ckeditor without the adapter
            //editorScript.Append("if (jQuery().ckeditor) {");

            //editorScript.AppendFormat("var {0} = jQuery('#{1}').ckeditor(editorConfig{0});", editorVar, editorFixedId);

            //editorScript.Append("} else if ($.ckeditor) {");

            //editorScript.AppendFormat("var {0} = $('#{1}').ckeditor(editorConfig{0});", editorVar, editorFixedId);

            //editorScript.Append("} else {");

            //editorScript.AppendFormat("var {0} = CKEDITOR.replace( '{1}', editorConfig{0});", editorVar, editorFixedId);

            //editorScript.Append("}");

            //// firefox maximize fix
            //editorScript.Append("CKEDITOR.on('instanceReady', function (ev) {");
            //editorScript.Append("ev.editor.on('maximize', function () {");
            //editorScript.Append("if (ev.editor.commands.maximize.state == 1) {");
            //editorScript.Append("var mainDocument = CKEDITOR.document;");
            //editorScript.Append("CKEDITOR.env.gecko && mainDocument.getDocumentElement().setStyle( 'position', 'fixed' );");
            //editorScript.Append("}");
            //editorScript.Append("});");
            //editorScript.Append("});");

            editorScript.Append("if(CKEDITOR && CKEDITOR.config){");
            editorScript.Append("  CKEDITOR.config.portalId = " + portalId + ";");
            editorScript.Append("  CKEDITOR.config.enableConfigHelper = " + EnableConfigHelper() + ";");

            editorScript.Append("  CKEDITOR.config.cloudServices_uploadUrl= 'https://33333.cke-cs.com/easyimage/upload/';");

            editorScript.Append("  CKEDITOR.config.cloudServices_tokenUrl= 'https://33333.cke-cs.com/token/dev/ijrDsqFix838Gh3wGO3F77FSW94BwcLXprJ4APSp3XQ26xsUHTi0jcb1hoBt';");



            editorScript.Append("};");

            //if(CKEDITOR && CKEDITOR.config && CKEDITOR.config.plugins ))

            // End of LoadScript
            //editorScript.Append("}");

            //ClientResourceManager.RegisterScript(string.Format(@"{0}_CKE_Config", editorFixedId), editorConfigScript.ToString(), true);
            RegisterStartupScript($@"{editorFixedId}_CKE_Startup", editorScript.ToString(), true);
        }

        private string EnableConfigHelper()
        {
            if (File.Exists(HostingEnvironment.MapPath("~/Providers/HtmlEditorProviders/CKEditor/plugins/confighelper/plugin.js")))
            {
                return "true";
            }
            else if (File.Exists(HostingEnvironment.MapPath("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.5.3/plugins/confighelper/plugin.js")))
            {
                return "true";
            }
            return "false";
        }

        private void RegisterStartupScript(string key, string script, bool addScriptTags)
        {
            Page.RegisterStartupScript(GetType(), key, script, addScriptTags);
        }

    }
}