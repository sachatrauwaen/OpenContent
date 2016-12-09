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

            //  /desktopmodules/OpenContent/api/{controller}/{action}
            mapRouteManager.MapHttpRoute(
                moduleFolderName: "OpenContent",
                routeName: "default",
                url: "{controller}/{action}",
                namespaces: new[] {
                    "Satrabel.OpenContent.Components",
                    "Satrabel.OpenContent.Components.JpList",
                    "Satrabel.OpenContent.Components.Rss",
                    "Satrabel.OpenContent.Components.Rest.Swagger" }
                );

            //  /desktopmodules/OpenContent/api/{controller}/v1/{entity}/{id}/{memberAction}
            mapRouteManager.MapHttpRoute(
                moduleFolderName: "OpenContent",
                routeName: "rest",
                url: "{controller}/v1/{entity}/{id}/{memberAction}",
                defaults: new { id = RouteParameter.Optional, memberAction = RouteParameter.Optional },
                namespaces: new[] { "Satrabel.OpenContent.Components.Rest" }
                );

            DataSourceManager.RegisterDataSources();
        }
    }
}
