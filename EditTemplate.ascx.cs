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
            //hlCancel.NavigateUrl = Globals.NavigateURL();
            //cmdSave.NavigateUrl = Globals.NavigateURL();

            cmdSave.Click += cmdSave_Click;
            cmdSaveClose.Click += cmdSaveAndClose_Click;
            cmdCancel.Click += cmdCancel_Click;
            cmdCustom.Click += cmdCustom_Click;
            var js = string.Format("javascript:return confirm('{0}');", Localization.GetSafeJSString(LocalizeString("OverwriteTemplate")));
            cmdCustom.Attributes.Add("onClick", js);

            scriptList.SelectedIndexChanged += scriptList_SelectedIndexChanged;

            //ServicesFramework.Instance.RequestAjaxScriptSupport();
            //ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

        }

        private void cmdCustom_Click(object sender, EventArgs e)
        {
            string Template = ModuleContext.Settings["template"] as string;
            string TemplateFolder = Path.GetDirectoryName(Template);
            string TemplateDir = Server.MapPath(TemplateFolder);
            string ModuleDir = Server.MapPath(ModuleTemplateDirectory);
            if (!Directory.Exists(ModuleDir))
            {
                Directory.CreateDirectory(ModuleDir);
            }
            foreach (var item in Directory.GetFiles(ModuleDir))
            {
                File.Delete(item);
            }
            foreach (var item in Directory.GetFiles(TemplateDir))
            {
                File.Copy(item, ModuleDir + Path.GetFileName(item));
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                LoadFiles();
                DisplayFile();
                string Template = ModuleContext.Settings["template"] as string;
                if (Template.StartsWith(ModuleTemplateDirectory))
                {
                    cmdCustom.Visible = false;
                }
                
            }
        }

        private void DisplayFile()
        {
            string Template = ModuleContext.Settings["template"] as string;
            string TemplateFolder = Path.GetDirectoryName(Template);
            TemplateFolder = OpenContentUtils.ReverseMapPath(TemplateFolder);

            string scriptFile = TemplateFolder + "/" + scriptList.SelectedValue;
            plSource.Text = scriptFile;
            string srcFile = Server.MapPath(scriptFile);
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
        private void LoadFiles()
        {
            scriptList.Items.Clear();
            string Template = ModuleContext.Settings["template"] as string;

            if (!(string.IsNullOrEmpty(Template)))
            {
                scriptList.Items.Add(new ListItem("Template", Path.GetFileName(Template)));
                scriptList.Items.Add(new ListItem("Stylesheet", Path.GetFileNameWithoutExtension(Template) + ".css"));
                scriptList.Items.Add(new ListItem("Javascript", Path.GetFileNameWithoutExtension(Template) + ".js"));

                scriptList.Items.Add(new ListItem("Data Schema", "schema.json"));
                scriptList.Items.Add(new ListItem("Edit Layout Options", "options.json"));
                scriptList.Items.Add(new ListItem("Edit Layout Options - Template File Overides", "options." + Path.GetFileNameWithoutExtension(Template) + ".json"));
                foreach (Locale item in LocaleController.Instance.GetLocales(PortalId).Values)
                {
                    scriptList.Items.Add(new ListItem("Edit Layout Options - Language Overides - " + item.Code , "options." + item.Code + ".json"));
                }
                scriptList.Items.Add(new ListItem("Settings Schema", "settings-schema.json"));
                scriptList.Items.Add(new ListItem("Settings Layout Options", "settings-options.json"));
                foreach (Locale item in LocaleController.Instance.GetLocales(PortalId).Values)
                {
                    scriptList.Items.Add(new ListItem("Settings Layout Options - Language Overides - " + item.Code, "options." + item.Code + ".json"));
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
            string Template = ModuleContext.Settings["template"] as string;
            string TemplateFolder = Path.GetDirectoryName(Template);
            string scriptFile = TemplateFolder + "/" + scriptList.SelectedValue;
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
            DisplayFile();
        }

        #endregion

    }
}

