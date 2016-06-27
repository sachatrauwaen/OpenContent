#region Copyright

// 
// Copyright (c) 2015-2016
// by Satrabel
// 

#endregion

#region Using Statements

using DotNetNuke.Web.Api;
using Satrabel.OpenContent.Components.Datasource;
using System.Web.Http;

#endregion

namespace Satrabel.OpenContent.Components
{
    public class StructRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("OpenContent", "default", "{controller}/{action}", new[] { "Satrabel.OpenContent.Components", "Satrabel.OpenContent.Components.JpList", "Satrabel.OpenContent.Components.Rss" });
            mapRouteManager.MapHttpRoute("OpenContent", "rest", "{controller}/v1/{entity}/{id}", new { id = RouteParameter.Optional }, new[] { "Satrabel.OpenContent.Components.Rest" });
            DataSourceManager.RegisterDataSources();
        }
    }
}

