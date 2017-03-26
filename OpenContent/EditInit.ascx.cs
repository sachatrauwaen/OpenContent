#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using DotNetNuke.Entities.Modules;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Manifest;

#endregion

namespace Satrabel.OpenContent
{
    public partial class EditInit : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var settings = ModuleContext.OpenContentSettings();
            RenderInfo renderinfo = new RenderInfo(settings.Template, settings.IsOtherModule);

            OpenContent.TemplateInit ti = (TemplateInit)TemplateInitControl;
            ti.PageRefresh = true;
            ti.ModuleContext = ModuleContext;
            ti.Settings = settings;
            ti.Renderinfo = renderinfo;
            ti.RenderOnlySaveButton = true;
        }

        protected override void OnPreRender(EventArgs e)
        {
            OpenContent.TemplateInit ti = (TemplateInit)TemplateInitControl;
            ti.RenderInitForm();
        }
    }

}