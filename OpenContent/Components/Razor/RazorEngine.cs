#region Using

using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Compilation;
using System.Web.WebPages;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;

#endregion

namespace Satrabel.OpenContent.Components.Razor
{
    public class RazorEngine
    {
        public RazorEngine(string razorScriptFile, IRenderContext renderContext, string localResourceFile)
        {
            RazorScriptFile = razorScriptFile;
            RenderContext = renderContext;
            LocalResourceFile = localResourceFile ?? Path.Combine(Path.GetDirectoryName(razorScriptFile), DotNetNuke.Services.Localization.Localization.LocalResourceDirectory, Path.GetFileName(razorScriptFile) + ".resx");

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

        [Obsolete("This method is obsolete since aug 2017; use different constructor instead")]
        public RazorEngine(string razorScriptFile, ModuleInstanceContext moduleContext, string localResourceFile) : this(razorScriptFile, new DnnRenderContext(moduleContext), localResourceFile)
        {
        }

        protected string RazorScriptFile { get;  }
        protected IRenderContext RenderContext { get; set; }
        protected string LocalResourceFile { get; set; }
        public OpenContentWebPage Webpage { get; set; }

        protected HttpContextBase HttpContextBase => new HttpContextWrapper(System.Web.HttpContext.Current);

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

        public void Render(TextWriter writer, dynamic model)
        {

            if (Webpage is OpenContentWebPage)
            {
                var mv = (OpenContentWebPage)Webpage;
                mv.Model = model;
            }
            if (Webpage != null)
                Webpage.ExecutePageHierarchy(new WebPageContext(HttpContextBase, Webpage, null), writer, Webpage);
        }

        public void Render(TextWriter writer)
        {
            try
            {
                Webpage.ExecutePageHierarchy(new WebPageContext(HttpContextBase, Webpage, null), writer, Webpage);
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
            if (compiledType != null)
            {
                objectValue = RuntimeHelpers.GetObjectValue(Activator.CreateInstance(compiledType));
            }
            return objectValue;
        }

        private void InitWebpage()
        {

            if (!string.IsNullOrEmpty(RazorScriptFile))
            {
                var objectValue = RuntimeHelpers.GetObjectValue(CreateWebPageInstance());
                if (objectValue == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage found at '{0}' was not created.", new object[] { RazorScriptFile }));
                }
                Webpage = objectValue as OpenContentWebPage;
                if (Webpage == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage at '{0}' must derive from OpenContentWebPage.", new object[] { RazorScriptFile }));
                }
                Webpage.Context = HttpContextBase;
                Webpage.VirtualPath = VirtualPathUtility.GetDirectory(RazorScriptFile);
                if (RenderContext != null) //called from a skin object?  todo: can we improve this? Inithelpers not initialized now.
                    RenderContext.InitHelpers(Webpage, LocalResourceFile);
            }
        }
    }
}