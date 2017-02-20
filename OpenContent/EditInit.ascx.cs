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
            RenderInfo renderinfo = new RenderInfo();
            renderinfo.Template = settings.Template;
            renderinfo.IsOtherModule = (settings.TabId > 0 && settings.ModuleId>0);


            OpenContent.TemplateInit ti = (TemplateInit)TemplateInitControl;
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