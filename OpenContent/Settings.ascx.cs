#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using System.Linq;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using Satrabel.OpenContent.Components;

#endregion

namespace Satrabel.OpenContent
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
            var template = ModuleContext.OpenContentSettings().Template;
            scriptList.Items.AddRange(OpenContentUtils.ListOfTemplatesFiles(PortalSettings, ModuleId, template, App.Config.Opencontent)
                                        .Select(i => new ListItem(i.Text, i.Value)
                                        {
                                            Selected = i.Selected,
                                        }).ToArray());
            base.LoadSettings();
        }
        public override void UpdateSettings()
        {
            ModuleController mc = new ModuleController();
            mc.UpdateModuleSetting(ModuleId, "template", scriptList.SelectedValue);
            if (!string.IsNullOrEmpty(HiddenField.Value))
                mc.UpdateModuleSetting(ModuleId, "data", HiddenField.Value);
        }
    }
}