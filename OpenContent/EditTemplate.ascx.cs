#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Common;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Localization;
using System.IO;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Json;
using Newtonsoft.Json.Linq;
using DotNetNuke.Framework.JavaScriptLibraries;

#endregion

namespace Satrabel.OpenContent
{

    public partial class EditTemplate : PortalModuleBase
    {
        public string ModuleTemplateDirectory
        {
            get
            {
                return PortalSettings.HomeDirectory + "OpenContent/Templates/" + ModuleId.ToString() + "/";
            }
        }
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            cmdSave.Click += cmdSave_Click;
            cmdSaveClose.Click += cmdSaveAndClose_Click;
            cmdCancel.Click += cmdCancel_Click;
            cmdCustom.Click += cmdCustom_Click;
            cmdBuilder.Click += cmdBuilder_Click;
            //var js = string.Format("javascript:return confirm('{0}');", Localization.GetSafeJSString(LocalizeString("OverwriteTemplate")));
            //cmdCustom.Attributes.Add("onClick", js);
            scriptList.SelectedIndexChanged += scriptList_SelectedIndexChanged;
        }

        private void cmdBuilder_Click(object sender, EventArgs e)
        {
            if (scriptList.SelectedValue.EndsWith("schema.json"))
            {
                var settings = ModuleContext.OpenContentSettings();
                FileUri template = settings.Template.MainTemplateUri();
                string templateFolder = Path.GetDirectoryName(template.FilePath);
                string scriptFile = templateFolder + "/" + scriptList.SelectedValue.Replace("schema.json", "builder.json");
                string srcFile = Server.MapPath(scriptFile);

                var schema = JsonUtils.LoadJsonFromFile(templateFolder + "/" + scriptList.SelectedValue) as JObject;
                var options = JsonUtils.LoadJsonFromFile(templateFolder + "/" + scriptList.SelectedValue.Replace("schema.json", "options.json")) as JObject;

                JObject builder = new JObject();

                if (schema != null && schema["items"] != null)
                {
                    builder["formtype"] = "array";
                    builder["formfields"] = GetBuilder(schema["items"] as JObject, options != null && options["items"] != null ? options["items"] as JObject : null);
                }
                else
                {
                    builder["formtype"] = "object";
                    builder["formfields"] = GetBuilder(schema, options);
                }
                if (!File.Exists(srcFile))
                {
                    File.WriteAllText(srcFile, builder.ToString());
                }
                Response.Redirect(Globals.NavigateURL(), true);
            }
        }

        private JArray GetBuilder(JObject schema, JObject options)
        {
            var formfields = new JArray();

            if (schema != null && schema["properties"] != null)
            {
                var schemaProperties = schema["properties"] as JObject;
                foreach (var schProp in schemaProperties.Properties())
                {
                    var sch = schProp.Value as JObject;
                    var opt = options != null && options["fields"] != null ? options["fields"][schProp.Name] : null;
                    var field = new JObject();
                    field["fieldname"] = schProp.Name;
                    string schematype = sch["type"] != null ? sch["type"].ToString() : "string";
                    string fieldtype = opt != null && opt["type"] != null ? opt["type"].ToString() : "text";
                    if (fieldtype.Substring(0, 2) == "ml")
                    {
                        fieldtype = fieldtype.Substring(2, fieldtype.Length - 2);
                        field["multilanguage"] = true;
                    }
                    if (sch["enum"] != null)
                    {
                        if (fieldtype == "text")
                        {
                            fieldtype = "select";
                        }
                        JArray optionLabels = null;
                        if (opt != null && opt["optionLabels"] != null)
                        {
                            optionLabels = opt["optionLabels"] as JArray;
                        }
                        JArray fieldoptions = new JArray();
                        int i = 0;
                        foreach (var item in sch["enum"] as JArray)
                        {
                            var fieldOpt = new JObject();
                            fieldOpt["value"] = item.ToString();
                            fieldOpt["text"] = optionLabels != null ? optionLabels[i].ToString() : item.ToString();
                            fieldoptions.Add(fieldOpt);
                            i++;
                        }
                        field["fieldoptions"] = fieldoptions;
                    };
                    if (schematype == "boolean")
                    {
                        fieldtype = "checkbox";
                    }
                    else if (schematype == "array")
                    {
                        if (fieldtype == "checkbox")
                        {
                            fieldtype = "multicheckbox";
                        }
                        else if (fieldtype == "text")
                        {
                            fieldtype = "array";
                        }
                        if (sch["items"] != null)
                        {
                            var b = GetBuilder(sch["items"] as JObject, opt != null && opt["items"] != null ? opt["items"] as JObject : null);
                            field["subfields"] = b;
                        }
                    }
                    else if (schematype == "object")
                    {
                        fieldtype = "object";
                        var b = GetBuilder(sch, opt as JObject);
                        field["subfields"] = b;
                    }
                    if (fieldtype == "select2" && opt["dataService"] != null && opt["dataService"]["data"] != null)
                    {
                        fieldtype = "relation";
                        field["relationoptions"] = new JObject();
                        field["relationoptions"]["datakey"] = opt["dataService"]["data"]["dataKey"];
                        field["relationoptions"]["valuefield"] = opt["dataService"]["data"]["valueField"];
                        field["relationoptions"]["textfield"] = opt["dataService"]["data"]["textField"];
                        if (schematype == "array")
                        {
                            field["relationoptions"]["many"] = true;
                        }
                    }
                    else if (fieldtype == "date" && opt["picker"] != null)
                    {
                        field["dateoptions"] = new JObject();
                        field["dateoptions"] = opt["picker"];
                    }
                    else if (fieldtype == "file" && opt["folder"] != null)
                    {
                        field["fileoptions"] = new JObject();
                        field["fileoptions"]["folder"] = opt["folder"];
                    }
                    else if (fieldtype == "file2")
                    {
                        field["file2options"] = new JObject();
                        if (opt["folder"] != null)
                        {
                            field["file2options"]["folder"] = opt["folder"];
                        }
                        if (opt["filter"] != null)
                        {
                            field["file2options"]["filter"] = opt["filter"];
                        }
                    }
                    else if (fieldtype == "icon")
                    {
                        field["iconoptions"] = new JObject();
                        field["iconoptions"]["glyphicons"] = opt["glyphicons"] == null ? false : opt["glyphicons"];
                        field["iconoptions"]["bootstrap"] = opt["bootstrap"] == null ? false : opt["bootstrap"];
                        field["iconoptions"]["fontawesome"] = opt["fontawesome"] == null ? true : opt["fontawesome"];
                    }
                    field["fieldtype"] = fieldtype;
                    if (sch["title"] != null)
                    {
                        field["title"] = sch["title"];
                    }
                    if (sch["default"] != null)
                    {
                        field["default"] = sch["default"];
                        field["advanced"] = true;
                    }
                    if (opt != null && opt["label"] != null)
                    {
                        field["title"] = opt["label"];
                    }
                    if (opt != null && opt["helper"] != null)
                    {
                        field["helper"] = opt["helper"];
                        field["advanced"] = true;
                    }
                    if (opt != null && opt["placeholder"] != null)
                    {
                        field["placeholder"] = opt["placeholder"];
                        field["advanced"] = true;
                    }
                    if (sch["required"] != null)
                    {
                        field["required"] = sch["required"];
                        field["advanced"] = true;
                    }
                    if (opt != null && opt["vertical"] != null)
                    {
                        field["vertical"] = opt["vertical"];
                    }
                    formfields.Add(field);
                }
            }
            return formfields;
        }

        private void cmdCustom_Click(object sender, EventArgs e)
        {
            var settings = ModuleContext.OpenContentSettings();
            TemplateManifest template = settings.Template;
            string templateFolder = template.ManifestFolderUri.UrlFolder;
            string templateDir = Server.MapPath(templateFolder);
            string moduleDir = Server.MapPath(ModuleTemplateDirectory);
            if (!Directory.Exists(moduleDir))
            {
                Directory.CreateDirectory(moduleDir);
            }
            foreach (var item in Directory.GetFiles(moduleDir))
            {
                File.Delete(item);
            }
            foreach (var item in Directory.GetFiles(templateDir))
            {
                File.Copy(item, moduleDir + Path.GetFileName(item));
            }
            ModuleController mc = new ModuleController();

            var newtemplate = new FileUri(ModuleTemplateDirectory, "schema.json");
            mc.UpdateModuleSetting(ModuleId, "template", newtemplate.FilePath);
            ModuleContext.Settings["template"] = newtemplate.FilePath;
            settings = ModuleContext.OpenContentSettings();
            InitEditor(settings.Template);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                TemplateManifest template = ModuleContext.OpenContentSettings().Template;
                InitEditor(template);
            }
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
        }

        private void InitEditor(TemplateManifest template)
        {
            LoadFiles(template);
            var scriptFile = new FileUri(template.ManifestFolderUri.UrlFolder, scriptList.SelectedValue);
            DisplayFile(scriptFile);
            /*
            if (template.MainTemplateUri().FilePath.StartsWith(ModuleTemplateDirectory))
            {
                cmdCustom.Visible = false;
            }
             */
        }

        private void DisplayFile(FileUri file)
        {
            //string TemplateFolder = template.Directory;
            //TemplateFolder = OpenContentUtils.ReverseMapPath(TemplateFolder);
            //string scriptFile = TemplateFolder + "/" + scriptList.SelectedValue;
            //plSource.Text = scriptFile;
            //string srcFile = Server.MapPath(scriptFile);

            lError.Visible = false;
            lError.Text = "";
            plSource.Text = file.FilePath;
            string srcFile = file.PhysicalFilePath;

            if (File.Exists(srcFile))
            {
                txtSource.Text = File.ReadAllText(srcFile);
            }
            else
            {
                txtSource.Text = "";
            }
            SetFileType(srcFile);
            cmdBuilder.Visible = scriptList.SelectedValue.EndsWith("schema.json") && scriptList.SelectedValue != "form-schaema.json";
            var schemaFile = new FileUri(file.FolderPath, "schema.json");
            string schema = "";
            if (schemaFile.FileExists)
            {
                schema = File.ReadAllText(schemaFile.PhysicalFilePath);
            }
            DotNetNuke.UI.Utilities.ClientAPI.RegisterClientVariable(Page, "schema", schema, true);
        }

        public JObject Model
        {
            get
            {
                var model = new JObject();
                TemplateManifest template = ModuleContext.OpenContentSettings().Template;
                var schemaFile = new FileUri(template.ManifestFolderUri, "schema.json");
                model["schema"] = JsonUtils.LoadJsonFromFile(template.ManifestFolderUri.UrlFolder + "schema.json");
                model["options"] = JsonUtils.LoadJsonFromFile(template.ManifestFolderUri.UrlFolder + "options.json");
                string key = template.MainTemplateUri().FileNameWithoutExtension;
                model["settingsSchema"] = JsonUtils.LoadJsonFromFile(template.ManifestFolderUri.UrlFolder + key + "-schema.json");
                model["settingsOptions"] = JsonUtils.LoadJsonFromFile(template.ManifestFolderUri.UrlFolder + key + "-options.json");
                model["listTemplate"] = template.IsListTemplate;
                model["localization"] = JsonUtils.LoadJsonFromFile(template.ManifestFolderUri.UrlFolder + DnnLanguageUtils.GetCurrentCultureCode() + ".json");
                var additionalData = new JObject();
                model["additionalData"] = additionalData;
                if (template.Manifest.AdditionalDataDefined())
                {
                    foreach (var addData in template.Manifest.AdditionalDataDefinition)
                    {
                        var addDataDef = new JObject();
                        additionalData[addData.Key] = addDataDef;
                        addDataDef["schema"] = JsonUtils.LoadJsonFromFile(template.ManifestFolderUri.UrlFolder + addData.Key + "-schema.json");
                        addDataDef["options"] = JsonUtils.LoadJsonFromFile(template.ManifestFolderUri.UrlFolder + addData.Key + "-options.json");
                    }
                }
                var file = new FileUri(ModuleContext.PortalSettings.HomeDirectory + "OpenContent","htmlsnippets.json");
                if (file.FileExists)
                {
                    model["snippets"] = JsonUtils.LoadJsonFromFile(file.FilePath);
                }
                else
                {
                    file = new FileUri("/Portals/_default/OpenContent","htmlsnippets.json");
                    if (file.FileExists)
                    {
                        model["snippets"] = JsonUtils.LoadJsonFromFile(file.FilePath);
                    }
                }
                return model;
            }
        }

        private void SetFileType(string filePath)
        {
            string mimeType;
            switch (Path.GetExtension(filePath).ToLowerInvariant())
            {
                case ".vb":
                    mimeType = "text/x-vb";
                    break;
                case ".cs":
                    mimeType = "text/x-csharp";
                    break;
                case ".css":
                    mimeType = "text/css";
                    break;
                case ".js":
                    mimeType = "text/javascript";
                    break;
                case ".json":
                    mimeType = "application/json";
                    break;
                case ".xml":
                case ".xslt":
                    mimeType = "application/xml";
                    break;
                case ".sql":
                case ".sqldataprovider":
                    mimeType = "text/x-sql";
                    break;
                case ".hbs":
                    mimeType = "htmlhandlebars";

                    break;
                default:
                    mimeType = "text/html";
                    break;
            }
            DotNetNuke.UI.Utilities.ClientAPI.RegisterClientVariable(Page, "mimeType", mimeType, true);
            phHandlebars.Visible = mimeType == "htmlhandlebars" || mimeType == "text/html";
        }
        private void LoadFiles(TemplateManifest template)
        {
            var settings = ModuleContext.OpenContentSettings();
            scriptList.Items.Clear();
            if (template != null)
            {
                //string templateFolder = template.DirectoryName;
                if (template.Main != null)
                {
                    scriptList.Items.Add(newListItem("Template", template.Main.Template, "Template", template));
                    if (template.Main.PartialTemplates != null)
                    {
                        foreach (var part in template.Main.PartialTemplates)
                        {
                            scriptList.Items.Add(newListItem(Path.GetFileNameWithoutExtension(part.Value.Template), part.Value.Template, "Template", template));
                        }
                    }
                }
                if (template.Detail != null)
                {
                    scriptList.Items.Add(newListItem(Path.GetFileNameWithoutExtension(template.Detail.Template), template.Detail.Template, "Template", template));
                    if (template.Detail.PartialTemplates != null)
                    {
                        foreach (var part in template.Detail.PartialTemplates)
                        {
                            scriptList.Items.Add(newListItem(Path.GetFileNameWithoutExtension(part.Value.Template), part.Value.Template, "Template", template));
                        }
                    }
                }
                scriptList.Items.Add(newListItem("Stylesheet", template.Key.ShortKey + ".css", "Template", template));
                scriptList.Items.Add(newListItem("Javascript", template.Key.ShortKey + ".js", "Template", template));
                scriptList.Items.Add(newListItem("Manifest", "manifest.json", "Template", template));
                if (!OpenContentUtils.BuilderExist(settings.Template.ManifestFolderUri))
                {
                    string title = string.IsNullOrEmpty(template.Manifest.Title) ? "Data" : template.Manifest.Title;
                    scriptList.Items.Add(newListItem("Schema", "schema.json", title, template));
                    scriptList.Items.Add(newListItem("Options", "options.json", title, template));
                    //scriptList.Items.Add(new ListItem("Edit Layout Options - Template File Overides", "options." + template.FileNameWithoutExtension + ".json"));
                    foreach (Locale item in LocaleController.Instance.GetLocales(PortalId).Values)
                    {
                        scriptList.Items.Add(newListItem("Options (" + item.Code+")", "options." + item.Code + ".json", title, template));
                    }
                }

                if (!OpenContentUtils.BuilderExist(settings.Template.ManifestFolderUri, template.Key.ShortKey))
                {
                    var title = "Settings";
                    scriptList.Items.Add(newListItem("Schema", template.Key.ShortKey + "-schema.json", title, template));
                    scriptList.Items.Add(newListItem("Options", template.Key.ShortKey + "-options.json", title, template));
                    foreach (Locale item in LocaleController.Instance.GetLocales(PortalId).Values)
                    {
                        scriptList.Items.Add(newListItem("Options (" + item.Code+")", template.Key.ShortKey + "-options." + item.Code + ".json", title, template));
                    }
                }
                if (template.Manifest.AdditionalDataDefined())
                {
                    foreach (var addData in template.Manifest.AdditionalDataDefinition)
                    {
                        if (!OpenContentUtils.BuilderExist(settings.Template.ManifestFolderUri, addData.Key))
                        {
                            string title = string.IsNullOrEmpty(addData.Value.Title) ? addData.Key : addData.Value.Title;
                            scriptList.Items.Add(newListItem(" Schema", addData.Key + "-schema.json", title, template));
                            scriptList.Items.Add(newListItem(" Options", addData.Key + "-options.json", title, template));
                            foreach (Locale item in LocaleController.Instance.GetLocales(PortalId).Values)
                            {
                                scriptList.Items.Add(newListItem(" Options (" + item.Code+")", addData.Key + "-options." + item.Code + ".json", title, template));
                            }
                        }
                    }
                }
                foreach (Locale item in LocaleController.Instance.GetLocales(PortalId).Values)
                {
                    scriptList.Items.Add(newListItem(item.Code, item.Code + ".json", "Localization", template));
                }
                //if (OpenContentUtils.FormExist(settings.Template.ManifestFolderUri))
                {
                    string title = "Form ";
                    scriptList.Items.Add(newListItem("Schema", "form-schema.json", title, template));
                    scriptList.Items.Add(newListItem("Options", "form-options.json", title, template));
                    //scriptList.Items.Add(new ListItem("Edit Layout Options - Template File Overides", "options." + template.FileNameWithoutExtension + ".json"));
                    foreach (Locale item in LocaleController.Instance.GetLocales(PortalId).Values)
                    {
                        scriptList.Items.Add(newListItem("Options (" + item.Code+")", "options." + item.Code + ".json", title, template));
                    }
                }
            }
        }

        private ListItem newListItem(string text, string value, string group, TemplateManifest template)
        {
            var li = new ListItem(text, value);
            li.Attributes["DataGroupField"] = group;
            var scriptFile = new FileUri(template.ManifestFolderUri.UrlFolder, value);
            
            if (!scriptFile.FileExists)
            {
                li.Attributes["CssClass"] = "fileNotExist";
            }            
            return li;
        }

        protected void cmdSave_Click(object sender, EventArgs e)
        {
            Save();
        }
        protected void cmdSaveAndClose_Click(object sender, EventArgs e)
        {
            if (Save())
            {
                Response.Redirect(Globals.NavigateURL(), true);
            }
        }

        private bool Save()
        {
            lError.Visible = false;
            lError.Text = "";
            if (scriptList.SelectedValue.EndsWith(".json") && !string.IsNullOrEmpty(txtSource.Text.Trim()))
            {
                try
                {
                    JObject.Parse(txtSource.Text);
                    if (scriptList.SelectedValue != "manifest.json")
                    {
                        System.Web.Helpers.Json.Decode(txtSource.Text);
                    }
                }
                catch (Exception ex)
                {
                    lError.Visible = true;
                    lError.Text = ex.Message;
                    
                    return false;
                }
            }

            FileUri template = ModuleContext.OpenContentSettings().Template.MainTemplateUri();
            string templateFolder = Path.GetDirectoryName(template.FilePath);
            string scriptFile = templateFolder + "/" + scriptList.SelectedValue;
            string srcFile = Server.MapPath(scriptFile);
            if (string.IsNullOrWhiteSpace(txtSource.Text))
            {
                if (File.Exists(srcFile))
                {
                    File.Delete(srcFile);
                }
            }
            else
            {
                File.WriteAllText(srcFile, txtSource.Text);
            }
            return true;
            //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Save Successful", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
        }


        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(Globals.NavigateURL(), true);
        }

        private void scriptList_SelectedIndexChanged(object sender, EventArgs e)
        {
            FileUri template = ModuleContext.OpenContentSettings().Template.MainTemplateUri();
            var scriptFile = new FileUri(template.UrlFolder, scriptList.SelectedValue);
            DisplayFile(scriptFile);
        }
        #endregion
    }
}

