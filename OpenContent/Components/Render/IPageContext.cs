using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.Client;
using System;

namespace Satrabel.OpenContent.Components.Render
{
    public interface IPageContext
    {
        string Title { get; set; }

        void AddLiteral(string s);
        void AddModuleMessage(string v, ModuleMessage.ModuleMessageType redError);
        void ProcessModuleLoadException(string friendlyMessage, Exception exc);
        void RegisterClientScriptBlock(Type type, string key, string script, bool addScriptTag);
        void RegisterClientVariable(string v1, string normalizedApplicationPath, bool v2);
        void RegisterScript(string filePath, int jsOrder, string provider = "DnnBodyProvider");
        void RegisterScript(string filePath, FileOrder.Js jsOrder = FileOrder.Js.DefaultPriority, string provider = "DnnBodyProvider");
        void RegisterStartupScript(Type type, string key, string script, bool addScriptTags);
        void RegisterStyleSheet(string v, FileOrder.Css portalCss = FileOrder.Css.DefaultPriority);
        string ResolveUrl(string relativeFilePath);
        void SetPageDescription(string metaDescription);
        void SetPageMeta(string metaOther);
    }
}