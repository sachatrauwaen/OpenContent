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
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework.JavaScriptLibraries;
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
            ti.ResourceFile = LocalResourceFile;

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
      
        protected override void OnPreRender(EventArgs e)
        {
            OpenContent.TemplateInit ti = (TemplateInit)TemplateInitControl;
            ti.RenderInitForm();
        }
    }

}