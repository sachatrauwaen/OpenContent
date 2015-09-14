#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using System.IO;
using System.Web.UI.WebControls;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using Satrabel.OpenContent.Components;

#endregion

namespace Satrabel.Struct
{
    public partial class Settings : ModuleSettingsBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ServicesFramework.Instance.RequestAjaxScriptSupport();
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            //JavaScript.RequestRegistration(CommonJs.DnnPlugins); ;
            //JavaScript.RequestRegistration(CommonJs.jQueryFileUpload);
        }
        public override void LoadSettings()
        {
            var template = OpenContentUtils.GetTemplate(Settings);
            scriptList.Items.AddRange(OpenContentUtils.GetTemplatesFiles(PortalSettings, ModuleId, template, "OpenContent").ToArray());
            base.LoadSettings();
        }
        public override void UpdateSettings()
        {
            ModuleController mc = new ModuleController();
            mc.UpdateModuleSetting(ModuleId, "template", scriptList.SelectedValue);
            mc.UpdateModuleSetting(ModuleId, "data", HiddenField.Value);
        }
    }
}