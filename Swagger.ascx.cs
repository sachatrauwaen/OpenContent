#region Copyright

// 
// Copyright (c) 2015-2016
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Common;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Manifest;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;

#endregion

namespace Satrabel.OpenContent
{
    public partial class Swagger : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ServicesFramework.Instance.RequestAjaxScriptSupport();
            //ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            JavaScript.RequestRegistration(CommonJs.DnnPlugins); // dnnPanels
        }

    }
}

