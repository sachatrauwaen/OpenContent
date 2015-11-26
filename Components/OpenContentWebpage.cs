using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI;
using System.Web.WebPages;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;


namespace Satrabel.OpenContent.Components
{
    public abstract class OpenContentWebPage<TModel> : DotNetNuke.Web.Razor.DotNetNukeWebPage<TModel>
    {
        int JSOrder = (int)FileOrder.Js.DefaultPriority;
        int CSSOrder = (int)FileOrder.Css.ModuleCss;

        public void RegisterStyleSheet(string filePath)
        {
            if (!filePath.StartsWith("http") && !filePath.Contains("/"))
            {
                filePath = VirtualPath + filePath;
            }
            if (!filePath.StartsWith("http"))
            {
                var file = new FileUri(filePath);
                filePath = file.UrlFilePath;
            }

            ClientResourceManager.RegisterStyleSheet((Page)HttpContext.Current.CurrentHandler, filePath, CSSOrder);
            CSSOrder++;
        }

        public void RegisterScript(string filePath)
        {
            if (!filePath.StartsWith("http") && !filePath.Contains("/"))
            {
                filePath = VirtualPath + filePath;
            }
            if (!filePath.StartsWith("http"))
            {
                var file = new FileUri(filePath);
                filePath = file.UrlFilePath;
            }

            ClientResourceManager.RegisterScript((Page)HttpContext.Current.CurrentHandler, filePath, JSOrder);
            JSOrder++;
        }



  



    }
}