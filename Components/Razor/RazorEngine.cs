#region Using

using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Compilation;
using System.Web.WebPages;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Razor.Helpers;

#endregion

namespace Satrabel.OpenContent.Components.Razor
{
    public class RazorEngine
    {
        public RazorEngine(string razorScriptFile, ModuleInstanceContext moduleContext, string localResourceFile)
        {
            RazorScriptFile = razorScriptFile;
            ModuleContext = moduleContext;
            LocalResourceFile = localResourceFile ?? Path.Combine(Path.GetDirectoryName(razorScriptFile), Localization.LocalResourceDirectory, Path.GetFileName(razorScriptFile) + ".resx");

            try
            {
                InitWebpage();
            }
            catch (HttpParseException)
            {
                throw;
            }
            catch (HttpCompileException)
            {
                throw;
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        protected string RazorScriptFile { get; set; }
        protected ModuleInstanceContext ModuleContext { get; set; }
        protected string LocalResourceFile { get; set; }
        public OpenContentWebPage Webpage { get; set; }

        protected HttpContextBase HttpContext
        {
            get { return new HttpContextWrapper(System.Web.HttpContext.Current); }
        }

        public Type RequestedModelType()
        {
            if (Webpage != null)
            {
                var webpageType = Webpage.GetType();
                if (webpageType.BaseType.IsGenericType)
                {
                    return webpageType.BaseType.GetGenericArguments()[0];
                }
            }
            return null;
        }

        public void Render<T>(TextWriter writer, T model)
        {
            try
            {
                if ((Webpage) is OpenContentWebPage<T>)
                {
                    var mv = (OpenContentWebPage<T>)Webpage;
                    mv.Model = model;
                }
                Webpage.ExecutePageHierarchy(new WebPageContext(HttpContext, Webpage, null), writer, Webpage);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        public void Render(TextWriter writer)
        {
            try
            {
                Webpage.ExecutePageHierarchy(new WebPageContext(HttpContext, Webpage, null), writer, Webpage);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        private object CreateWebPageInstance()
        {
            var compiledType = BuildManager.GetCompiledType(RazorScriptFile);
            object objectValue = null;
            if (((compiledType != null)))
            {
                objectValue = RuntimeHelpers.GetObjectValue(Activator.CreateInstance(compiledType));
            }
            return objectValue;
        }

        private void InitHelpers(OpenContentWebPage webPage)
        {
            webPage.Dnn = new DnnHelper(ModuleContext);
            webPage.Html = new HtmlHelper(ModuleContext, LocalResourceFile);
            webPage.Url = new UrlHelper(ModuleContext);
        }

        private void InitWebpage()
        {
            if (!string.IsNullOrEmpty(RazorScriptFile))
            {
                var objectValue = RuntimeHelpers.GetObjectValue(CreateWebPageInstance());
                if ((objectValue == null))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage found at '{0}' was not created.", new object[] { RazorScriptFile }));
                }
                Webpage = objectValue as OpenContentWebPage;
                if ((Webpage == null))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage at '{0}' must derive from OpenContentWebPage.", new object[] { RazorScriptFile }));
                }
                Webpage.Context = HttpContext;
                Webpage.VirtualPath = VirtualPathUtility.GetDirectory(RazorScriptFile);
                InitHelpers(Webpage);
            }
        }
    }
}