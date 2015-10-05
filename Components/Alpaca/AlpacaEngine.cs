using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using System;
using System.Collections.Generic;
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
        public string VirtualDirectory { get; set; }
        public string Prefix { get; set; }
        public Page Page { get; private set; }
        public ModuleInstanceContext ModuleContext { get; private set; }

        public AlpacaEngine(Page Page, ModuleInstanceContext ModuleContext)
        {
            this.Page = Page;
            this.ModuleContext = ModuleContext;
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
            //<dnncl:DnnCssInclude ID="customJS" runat="server" FilePath="~/DesktopModules/OpenContent/alpaca/css/alpaca-dnn.css" AddTag="false" />
            //<dnncl:DnnJsInclude ID="DnnJsInclude1" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/handlebars/handlebars.js" Priority="106" ForceProvider="DnnPageHeaderProvider" />
            //<dnncl:DnnJsInclude ID="DnnJsInclude2" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/alpaca/web/alpaca.js" Priority="107" ForceProvider="DnnPageHeaderProvider" />
            //<dnncl:DnnJsInclude ID="DnnJsInclude4" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/typeahead.js/dist/typeahead.bundle.min.js" Priority="106" ForceProvider="DnnPageHeaderProvider" />
            //<dnncl:DnnJsInclude ID="DnnJsInclude14" runat="server" FilePath="~/DesktopModules/OpenContent/js/wysihtml/wysihtml-toolbar.js" Priority="107"  />
            //<dnncl:DnnJsInclude ID="DnnJsInclude6" runat="server" FilePath="~/DesktopModules/OpenContent/js/wysihtml/parser_rules/advanced_opencontent.js" Priority="107"  />

            //<dnncl:dnnjsinclude id="DnnJsInclude15" runat="server" filepath="~/DesktopModules/OpenContent/alpaca/js/fields/dnn/dnnfields.js" priority="108" forceprovider="DnnFormBottomProvider" />
            //<dnncl:DnnJsInclude ID="DnnJsInclude7" runat="server" FilePath="~/DesktopModules/OpenContent/alpaca/js/views/dnn.js" Priority="107" ForceProvider="DnnFormBottomProvider" />
            //<dnncl:DnnJsInclude ID="DnnJsInclude3" runat="server" FilePath="~/DesktopModules/OpenContent/js/requirejs/require.js" Priority="110" ForceProvider="DnnFormBottomProvider" />
            //<dnncl:DnnJsInclude ID="DnnJsInclude5" runat="server" FilePath="~/DesktopModules/OpenContent/js/requirejs/config.js" Priority="111" ForceProvider="DnnFormBottomProvider" />
            //<dnncl:DnnCssInclude ID="DnnCssInclude1" runat="server" FilePath="~/DesktopModules/OpenContent/css/font-awesome/css/font-awesome.min.css" AddTag="false" />



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
            if (allFields || fieldTypes.Contains("select2") || fieldTypes.Contains("image2"))
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
                }

            }

        }

        private JToken GetOptions()
        {
            string physicalDirectory = HostingEnvironment.MapPath("~" + VirtualDirectory);

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
            optionsFilename = physicalDirectory + "\\" + (string.IsNullOrEmpty(Prefix) ? "" : Prefix + "-") + "options." + ModuleContext.PortalSettings.CultureCode + ".json";
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


    }
}