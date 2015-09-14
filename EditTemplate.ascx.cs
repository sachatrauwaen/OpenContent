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
            //var js = string.Format("javascript:return confirm('{0}');", Localization.GetSafeJSString(LocalizeString("OverwriteTemplate")));
            //cmdCustom.Attributes.Add("onClick", js);
            scriptList.SelectedIndexChanged += scriptList_SelectedIndexChanged;
        }

        private void cmdCustom_Click(object sender, EventArgs e)
        {
            FileUri template = OpenContentUtils.GetTemplate(ModuleContext.Settings);
            string templateFolder = template.Directory ;
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
            template = new FileUri(ModuleTemplateDirectory + "schema.json");
            mc.UpdateModuleSetting(ModuleId, "template", template.FilePath);
            InitEditor(template);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                FileUri template = OpenContentUtils.GetTemplate(ModuleContext.Settings);
                InitEditor(template);
            }
        }

        private void InitEditor(FileUri template)
        {
            LoadFiles(template);
            //var scriptFile = new FileUri(template.Directory + "/" + scriptList.SelectedValue);
            DisplayFile(template);
            if (template.FilePath.StartsWith(ModuleTemplateDirectory))
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
        private void LoadFiles(FileUri template)
        {
            scriptList.Items.Clear();
            if (template != null)
            {
                //string templateFolder = template.DirectoryName;
                TemplateManifest manifest = OpenContentUtils.GetTemplateManifest(template);
                if (manifest != null )
                {
                    if (manifest.Main != null){
                        scriptList.Items.Add(new ListItem("Template", manifest.Main.Template));
                    }
                    if (manifest.Main.PartialTemplates != null)
                    {
                        foreach (var part in manifest.Main.PartialTemplates)
                        {
                            scriptList.Items.Add(new ListItem("Template - " + Path.GetFileNameWithoutExtension(part.Value.Template), part.Value.Template));
                        }
                    }
                    if (manifest.Detail != null)
                    {
                        scriptList.Items.Add(new ListItem("Template - " + Path.GetFileNameWithoutExtension(manifest.Detail.Template), manifest.Detail.Template));
                    }
                    if (manifest.Detail.PartialTemplates != null)
                    {
                        foreach (var part in manifest.Detail.PartialTemplates)
                        {
                            scriptList.Items.Add(new ListItem("Template - " + Path.GetFileNameWithoutExtension(part.Value.Template), part.Value.Template));
                        }
                    }
                    scriptList.Items.Add(new ListItem("Manifest", "manifest.json"));
                }
                else
                {
                    scriptList.Items.Add(new ListItem("Template",template.FileName ));
                }

                scriptList.Items.Add(new ListItem("Stylesheet", template.FileNameWithoutExtension + ".css"));
                scriptList.Items.Add(new ListItem("Javascript", template.FileNameWithoutExtension + ".js"));
                scriptList.Items.Add(new ListItem("Schema", "schema.json"));
                scriptList.Items.Add(new ListItem("Layout Options", "options.json"));
                //scriptList.Items.Add(new ListItem("Edit Layout Options - Template File Overides", "options." + template.FileNameWithoutExtension + ".json"));
                foreach (Locale item in LocaleController.Instance.GetLocales(PortalId).Values)
                {
                    scriptList.Items.Add(new ListItem("Layout Options - " + item.Code, "options." + item.Code + ".json"));
                }
                scriptList.Items.Add(new ListItem("Settings Schema", template.FileNameWithoutExtension + "-schema.json"));
                scriptList.Items.Add(new ListItem("Settings Layout Options", template.FileNameWithoutExtension + "-options.json"));
                foreach (Locale item in LocaleController.Instance.GetLocales(PortalId).Values)
                {
                    scriptList.Items.Add(new ListItem("Settings Layout Options - " + item.Code, template.FileNameWithoutExtension + "-options." + item.Code + ".json"));
                }
            }
        }

        protected void cmdSave_Click(object sender, EventArgs e)
        {
            Save();
        }
        protected void cmdSaveAndClose_Click(object sender, EventArgs e)
        {
            Save();
            Response.Redirect(Globals.NavigateURL(), true);
        }

        private void Save()
        {
            FileUri template = OpenContentUtils.GetTemplate(ModuleContext.Settings);
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
            //DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Save Successful", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
        }


        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(Globals.NavigateURL(), true);
        }

        private void scriptList_SelectedIndexChanged(object sender, EventArgs e)
        {
            FileUri template = OpenContentUtils.GetTemplate(ModuleContext.Settings);
            var scriptFile = new FileUri(template.Directory + "/" + scriptList.SelectedValue);
            DisplayFile(scriptFile); 
        }
        #endregion
    }
}

