using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Satrabel.OpenContent.Components.Alpaca;
using System.Web.UI;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public class FormHelpers
    {
        public static void RegisterForm(Page page, string sourceFolder, string view, ref int jsOrder)
        {
            string min = ".min";
            if (DotNetNuke.Common.HttpContextSource.Current.IsDebuggingEnabled)
            {
                min = "";
            }
            DotNetNuke.Framework.ServicesFramework.Instance.RequestAjaxScriptSupport();

            DnnUtils.RegisterScript(page, sourceFolder, "/DesktopModules/OpenContent/js/lib/handlebars/handlebars" + min + ".js", jsOrder);
            jsOrder++;
            DnnUtils.RegisterScript(page, sourceFolder, "/DesktopModules/OpenContent/js/alpaca/bootstrap/alpaca" + min + ".js", jsOrder);
            jsOrder++;
            ClientResourceManager.RegisterStyleSheet(page, page.ResolveUrl("/DesktopModules/OpenContent/js/alpaca/bootstrap/alpaca" + min + ".css"), FileOrder.Css.PortalCss);
            DnnUtils.RegisterScript(page, sourceFolder, "/DesktopModules/OpenContent/js/oc.jquery.js", jsOrder);
            jsOrder++;
        }

        public static void RegisterEditForm(Page page, string sourceFolder, int portalId, string prefix, ref int jsOrder)
        {
            DotNetNuke.Framework.ServicesFramework.Instance.RequestAjaxScriptSupport();
            AlpacaEngine alpaca = new AlpacaEngine(page, portalId, sourceFolder, prefix);
            alpaca.RegisterAll(bootstrapLayoutEnabled: true, loadBootstrap: false, loadGlyphicons:false);
            DnnUtils.RegisterScript(page, sourceFolder, "/DesktopModules/OpenContent/js/oc.jquery.js", jsOrder);
            jsOrder++;
        }
    }
}