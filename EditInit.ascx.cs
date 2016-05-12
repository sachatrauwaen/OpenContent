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
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Lucene;
using Satrabel.OpenContent.Components.Lucene.Config;
using Satrabel.OpenContent.Components.Lucene.Index;
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

            if (settings.TabId > 0 && settings.ModuleId > 0) // other module
            {
                ModuleController mc = new ModuleController();
                renderinfo.SetDataSourceModule(settings.TabId, settings.ModuleId,
                    mc.GetModule(renderinfo.ModuleId, renderinfo.TabId, false), null, "");
            }
            else // this module
            {
                renderinfo.SetDataSourceModule(settings.TabId, ModuleContext.ModuleId, ModuleContext.Configuration, null, "");
            }

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