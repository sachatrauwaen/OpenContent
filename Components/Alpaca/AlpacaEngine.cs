using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using DotNetNuke.UI.Modules;
using System.Web.UI.WebControls;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;
using System.Web.UI.HtmlControls;


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

        public Page Page { get; private set; }
        public ModuleInstanceContext ModuleContext { get; private set; }

        public AlpacaEngine(Page Page, ModuleInstanceContext moduleContext, string virtualDir, string filePrefix)
        {
            this.Page = Page;
            this.ModuleContext = moduleContext;
            VirtualDirectory = virtualDir;
            Prefix = filePrefix;
        }

        public void RegisterAll(bool bootstrap = false)
        {
            RegisterAlpaca(bootstrap);
            RegisterTemplates();
            RegisterScripts(bootstrap);
            RegisterFields();
        }

        private void RegisterAlpaca(bool bootstrap)
        {
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/handlebars/handlebars.js", FileOrder.Js.DefaultPriority);
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/typeahead.js/dist/typeahead.bundle.min.js", FileOrder.Js.DefaultPriority);

            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/wysihtml/wysihtml-toolbar.js", FileOrder.Js.DefaultPriority + 1);
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/wysihtml/parser_rules/advanced_opencontent.js", FileOrder.Js.DefaultPriority + 1);
            if (bootstrap)
            {
                ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/OpenContent/js/alpaca-1.5.8/alpaca/bootstrap/alpaca.css", FileOrder.Css.DefaultPriority);
                ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/alpaca-1.5.8/alpaca/bootstrap/alpaca.js", FileOrder.Js.DefaultPriority + 1);
                ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/alpaca/js/views/dnnbootstrap.js", FileOrder.Js.DefaultPriority + 2);
            }
            else
            {
                ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/OpenContent/alpaca/css/alpaca-dnn.css", FileOrder.Css.DefaultPriority);
                ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/alpaca-1.5.8/alpaca/web/alpaca.js", FileOrder.Js.DefaultPriority + 1);
                ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/alpaca/js/views/dnn.js", FileOrder.Js.DefaultPriority + 2);
            }
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/alpaca/js/fields/dnn/dnnfields.js", FileOrder.Js.DefaultPriority + 3);

            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/alpacaengine.js", FileOrder.Js.DefaultPriority + 10);

            ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/OpenContent/css/font-awesome/css/font-awesome.min.css", FileOrder.Css.DefaultPriority);
        }
        public void RegisterTemplates()
        {

            var body = (HtmlGenericControl)Page.FindControl("Body");
            if (body.FindControl("oc-dnntemplates") == null)
            {
                string templates = File.ReadAllText(HostingEnvironment.MapPath("~/DesktopModules/OpenContent/alpaca/templates/dnn-edit/dnntemplates.html"));
                var lit = new LiteralControl(templates);
                lit.ID = "oc-dnntemplates";

                body.Controls.Add(lit);
            }

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
            DotNetNuke.UI.Utilities.ClientAPI.RegisterClientVariable(Page, "oc_websiteRoot", FileUri.NormalizedApplicationPath, true);
        }

        private void RegisterFields()
        {
            bool allFields = string.IsNullOrEmpty(VirtualDirectory);
            List<string> fieldTypes = new List<string>();
            if (!allFields)
            {
                JToken options = GetOptions();
                if (options != null)
                {
                    fieldTypes = FieldTypes(options);
                }
            }
            if (allFields || fieldTypes.Contains("address"))
            {
                ClientResourceManager.RegisterScript(Page, "//maps.googleapis.com/maps/api/js?v=3.exp&libraries=places", FileOrder.Js.DefaultPriority);
            }
            if (allFields || fieldTypes.Contains("imagecropper"))
            {
                ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/cropper/cropper.js", FileOrder.Js.DefaultPriority);
                ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/OpenContent/js/cropper/cropper.css", FileOrder.Css.DefaultPriority);
            }
            if (allFields || 
                    fieldTypes.Contains("select2") || fieldTypes.Contains("image2") || fieldTypes.Contains("file2") || fieldTypes.Contains("url2") ||
                    fieldTypes.Contains("mlimage2") || fieldTypes.Contains("mlfile2") || fieldTypes.Contains("mlurl2") 
                )
            {
                ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/select2/select2.js", FileOrder.Js.DefaultPriority);
                ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/OpenContent/js/select2/select2.css", FileOrder.Css.DefaultPriority);
            }

            //<!-- bootstrap datetimepicker for date, time and datetime controls -->
            //<dnncl:DnnJsInclude ID="DnnJsInclude13" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/moment/min/moment-with-locales.min.js" Priority="106" ForceProvider="DnnPageHeaderProvider" />
            //<dnncl:DnnJsInclude ID="DnnJsInclude14" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/eonasdan-bootstrap-datetimepicker/build/js/bootstrap-datetimepicker.min.js" Priority="106" ForceProvider="DnnPageHeaderProvider" />
            //<dnncl:DnnCssInclude ID="DnnCssInclude2" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.css" AddTag="false" />

            if (allFields || fieldTypes.Contains("date") || fieldTypes.Contains("datetime") || fieldTypes.Contains("time"))
            {
                ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/moment/min/moment-with-locales.min.js", FileOrder.Js.DefaultPriority, "DnnPageHeaderProvider");
                ClientResourceManager.RegisterScript(Page, "~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/eonasdan-bootstrap-datetimepicker/build/js/bootstrap-datetimepicker.min.js", FileOrder.Js.DefaultPriority + 1, "DnnPageHeaderProvider");
                ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.css", FileOrder.Css.DefaultPriority);
            }
            if (allFields || fieldTypes.Contains("ckeditor") || fieldTypes.Contains("mlckeditor"))
            {
                var form = Page.FindControl("Form");
                if (form.FindControl("CKDNNporid") == null)
                {
                    if (File.Exists(HostingEnvironment.MapPath("~/Providers/HtmlEditorProviders/CKEditor/ckeditor.js")))
                    {
                        ClientResourceManager.RegisterScript(Page, "~/Providers/HtmlEditorProviders/CKEditor/ckeditor.js", FileOrder.Js.DefaultPriority);
                        DotNetNuke.UI.Utilities.ClientAPI.RegisterClientVariable(Page, "PortalId", ModuleContext.PortalId.ToString(), true);
                        var CKDNNporid = new HiddenField();
                        CKDNNporid.ID = "CKDNNporid";
                        CKDNNporid.ClientIDMode = ClientIDMode.Static;

                        form.Controls.Add(CKDNNporid);
                        CKDNNporid.Value = ModuleContext.PortalId.ToString();
                    }
                    else
                    {
                        Log.Logger.Warn("Failed to load CKEeditor. Can not find ~/Providers/HtmlEditorProviders/CKEditor/ckeditor.js");
                    }
                }
            }
        }
        private JToken GetOptions()
        {
            string physicalDirectory = HostingEnvironment.MapPath("~/" + VirtualDirectory);
            JToken optionsJson = null;
            // default options
            string optionsFilename = physicalDirectory + "\\" + (string.IsNullOrEmpty(Prefix) ? "" : Prefix + "-") + "options.json";
            if (File.Exists(optionsFilename))
            {
                string fileContent = File.ReadAllText(optionsFilename);
                if (!string.IsNullOrWhiteSpace(fileContent))
                {
                    optionsJson = JObject.Parse(fileContent);
                }
            }
            // language options
            optionsFilename = physicalDirectory + "\\" + (string.IsNullOrEmpty(Prefix) ? "" : Prefix + "-") + "options." + DnnUtils.GetCurrentCultureCode() + ".json";
            if (File.Exists(optionsFilename))
            {
                string fileContent = File.ReadAllText(optionsFilename);
                if (!string.IsNullOrWhiteSpace(fileContent))
                {
                    if (optionsJson == null)
                        optionsJson = JObject.Parse(fileContent);
                    else
                        optionsJson = optionsJson.JsonMerge(JObject.Parse(fileContent));
                }
            }
            return optionsJson;
        }

        private List<string> FieldTypes(JToken options)
        {
            List<string> types = new List<string>();
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
            else if (options["items"] != null && options["items"]["type"] != null)
            {
                var fieldtype = options["items"]["type"] as JValue;
                types.Add(fieldtype.Value.ToString());
            }
            return types;
        }

        public static string AlpacaCulture(string CultureCode)
        {
            string[] alpacaLocales = { "zh_CN", "hr_HR", "fr_FR", "de_AT", "it_IT", "ja_JP", "pl_PL", "pt_BR", "es_ES" };
            string lang = CultureCode.Replace("-", "_");
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
                    return lang;
                }
            }
            return "en_US";
        }

        

    }
}