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
            var _settings = ModuleContext.OpenContentSettings();
            RenderInfo _renderinfo = new RenderInfo();
            _renderinfo.Template = _settings.Template;

            if (_settings.TabId > 0 && _settings.ModuleId > 0) // other module
            {
                ModuleController mc = new ModuleController();
                _renderinfo.SetDataSourceModule(_settings.TabId, _settings.ModuleId,
                    mc.GetModule(_renderinfo.ModuleId, _renderinfo.TabId, false), null, "");
            }
            else // this module
            {
                _renderinfo.SetDataSourceModule(_settings.TabId, ModuleContext.ModuleId, ModuleContext.Configuration,
                    null, "");
            }

            OpenContent.TemplateInit ti = (TemplateInit) TemplateInitControl;
            ti.ModuleContext = ModuleContext;
            ti.Settings = _settings;
            ti.Renderinfo = _renderinfo;
            ti.RenderOnlySaveButton = true;
        }

        protected override void OnPreRender(EventArgs e)
        {
             OpenContent.TemplateInit ti = (TemplateInit) TemplateInitControl;
            ti.RenderInitForm();
        }
    }

}