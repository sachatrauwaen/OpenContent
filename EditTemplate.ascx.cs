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
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Framework;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Localization;
using System.IO;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
                FileUri template = settings.Template.Uri();
                string templateFolder = Path.GetDirectoryName(template.FilePath);
                string scriptFile = templateFolder + "/" + scriptList.SelectedValue.Replace("schema.json", "builder.json");
                string srcFile = Server.MapPath(scriptFile);

                var schema = JsonUtils.LoadJsonFromFile(templateFolder + "/" + scriptList.SelectedValue) as JObject;
                var options = JsonUtils.LoadJsonFromFile(templateFolder + "/" + scriptList.SelectedValue.Replace("schema.json", "options.json")) as JObject;

                JObject builder = new JObject();

                if (schema["items"] != null)
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

            if (schema["properties"] != null)
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
            string templateFolder = template.Uri().UrlFolder;
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
        }

        private void InitEditor(TemplateManifest template)
        {
            LoadFiles(template);
            var scriptFile = new FileUri(template.Uri().UrlFolder, scriptList.SelectedValue);
            DisplayFile(scriptFile);
            if (template.Uri().FilePath.StartsWith(ModuleTemplateDirectory))
            {
                cmdCustom.Visible = false;
            }
        }

        private void DisplayFile(FileUri template)
        {
            //string TemplateFolder = template.Directory;
            //TemplateFolder = OpenContentUtils.ReverseMapPath(TemplateFolder);
            //string scriptFile = TemplateFolder + "/" + scriptList.SelectedValue;
            //plSource.Text = scriptFile;
            //string srcFile = Server.MapPath(scriptFile);
            plSource.Text = template.FilePath;
            string srcFile = template.PhysicalFilePath;

            if (File.Exists(srcFile))
            {
                txtSource.Text = File.ReadAllText(srcFile);
            }
            else
            {
                txtSource.Text = "";
            }
            SetFileType(srcFile);
            cmdBuilder.Visible = scriptList.SelectedValue.EndsWith("schema.json");
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
                    scriptList.Items.Add(new ListItem("Template", template.Main.Template));
                    if (template.Main.PartialTemplates != null)
                    {
                        foreach (var part in template.Main.PartialTemplates)
                        {
                            scriptList.Items.Add(new ListItem("Template - " + Path.GetFileNameWithoutExtension(part.Value.Template), part.Value.Template));
                        }
                    }
                }
                if (template.Detail != null)
                {
                    scriptList.Items.Add(new ListItem("Template - " + Path.GetFileNameWithoutExtension(template.Detail.Template), template.Detail.Template));
                    if (template.Detail.PartialTemplates != null)
                    {
                        foreach (var part in template.Detail.PartialTemplates)
                        {
                            scriptList.Items.Add(new ListItem("Template - " + Path.GetFileNameWithoutExtension(part.Value.Template), part.Value.Template));
                        }
                    }
                }
                scriptList.Items.Add(new ListItem("Stylesheet", template.Key.ShortKey + ".css"));
                scriptList.Items.Add(new ListItem("Javascript", template.Key.ShortKey + ".js"));
                scriptList.Items.Add(new ListItem("Manifest", "manifest.json"));
                if (!OpenContentUtils.BuilderExist(settings.Template.ManifestDir))
                {
                    string title = string.IsNullOrEmpty(template.Manifest.Title) ? "Data " : template.Manifest.Title + " ";
                    scriptList.Items.Add(new ListItem(title + "Schema", "schema.json"));
                    scriptList.Items.Add(new ListItem(title + "Options", "options.json"));
                    //scriptList.Items.Add(new ListItem("Edit Layout Options - Template File Overides", "options." + template.FileNameWithoutExtension + ".json"));
                    foreach (Locale item in LocaleController.Instance.GetLocales(PortalId).Values)
                    {
                        scriptList.Items.Add(new ListItem(title + "Options - " + item.Code, "options." + item.Code + ".json"));
                    }
                }
                if (!OpenContentUtils.BuilderExist(settings.Template.ManifestDir, template.Key.ShortKey))
                {
                    scriptList.Items.Add(new ListItem("Settings Schema", template.Key.ShortKey + "-schema.json"));
                    scriptList.Items.Add(new ListItem("Settings Options", template.Key.ShortKey + "-options.json"));
                    foreach (Locale item in LocaleController.Instance.GetLocales(PortalId).Values)
                    {
                        scriptList.Items.Add(new ListItem("Settings Options - " + item.Code, template.Key.ShortKey + "-options." + item.Code + ".json"));
                    }
                }
                if (template.Manifest.AdditionalData != null)
                {
                    foreach (var addData in template.Manifest.AdditionalData)
                    {
                        if (!OpenContentUtils.BuilderExist(settings.Template.ManifestDir, addData.Key))
                        {
                            string title = string.IsNullOrEmpty(addData.Value.Title) ? addData.Key : addData.Value.Title;
                            scriptList.Items.Add(new ListItem(title + " Schema", addData.Key + "-schema.json"));
                            scriptList.Items.Add(new ListItem(title + " Options", addData.Key + "-options.json"));
                            foreach (Locale item in LocaleController.Instance.GetLocales(PortalId).Values)
                            {
                                scriptList.Items.Add(new ListItem(title + " Options - " + item.Code, addData.Key + "-options." + item.Code + ".json"));
                            }
                        }
                    }
                }
                foreach (Locale item in LocaleController.Instance.GetLocales(PortalId).Values)
                {
                    scriptList.Items.Add(new ListItem("Localization - " + item.Code, item.Code + ".json"));
                }
            }
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
                    System.Web.Helpers.Json.Decode(txtSource.Text);
                }
                catch (Exception ex)
                {
                    lError.Visible = true;
                    lError.Text = ex.Message;
                    return false;
                }
            }


            FileUri template = ModuleContext.OpenContentSettings().Template.Uri();
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
            FileUri template = ModuleContext.OpenContentSettings().Template.Uri();
            var scriptFile = new FileUri(template.UrlFolder, scriptList.SelectedValue);
            DisplayFile(scriptFile);
        }
        #endregion
    }
}

