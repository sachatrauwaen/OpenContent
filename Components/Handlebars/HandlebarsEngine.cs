using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using HandlebarsDotNet;
using DotNetNuke.UI.Modules;
using System.Globalization;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.TemplateHelpers;


namespace Satrabel.OpenContent.Components.Handlebars
{
    public class HandlebarsEngine
    {
        private int JSOrder = 100;
        public string Execute(string source, dynamic model)
        {
            var hbs = HandlebarsDotNet.Handlebars.Create();
            RegisterDivideHelper(hbs);
            RegisterMultiplyHelper(hbs);
            RegisterEqualHelper(hbs);
            RegisterFormatNumberHelper(hbs);
            RegisterFormatDateTimeHelper(hbs);
            RegisterImageUrlHelper(hbs);
            RegisterArrayIndexHelper(hbs);
            RegisterArrayTranslateHelper(hbs);
            var template = hbs.Compile(source);
            var result = template(model);
            return result;
        }
        public string Execute(Page page, FileUri sourceFilename, dynamic model)
        {
            string source = File.ReadAllText(sourceFilename.PhysicalFilePath);
            string sourceFolder = sourceFilename.UrlFolder.Replace("\\", "/") + "/";
            var hbs = HandlebarsDotNet.Handlebars.Create();
            RegisterDivideHelper(hbs);
            RegisterMultiplyHelper(hbs);
            RegisterEqualHelper(hbs);
            RegisterFormatNumberHelper(hbs);
            RegisterFormatDateTimeHelper(hbs);
            RegisterImageUrlHelper(hbs);
            RegisterScriptHelper(hbs);
            RegisterHandlebarsHelper(hbs);
            RegisterRegisterStylesheetHelper(hbs, page, sourceFolder);
            RegisterRegisterScriptHelper(hbs, page, sourceFolder);
            RegisterArrayIndexHelper(hbs);
            RegisterArrayTranslateHelper(hbs);
            var template = hbs.Compile(source);
            var result = template(model);
            return result;
        }
        public string Execute(Page page, IModuleControl module, TemplateFiles files, string templateVirtualFolder, dynamic model)
        {
            string source = File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(templateVirtualFolder + "/" + files.Template));
            string sourceFolder = templateVirtualFolder.Replace("\\", "/") + "/";
            var hbs = HandlebarsDotNet.Handlebars.Create();
            if (files.PartialTemplates != null)
            {
                foreach (var part in files.PartialTemplates)
                {
                    RegisterTemplate(hbs, part.Key, templateVirtualFolder + "/" + part.Value.Template);
                }
            }
            RegisterDivideHelper(hbs);
            RegisterMultiplyHelper(hbs);
            RegisterEqualHelper(hbs);
            RegisterFormatNumberHelper(hbs);
            RegisterFormatDateTimeHelper(hbs);
            RegisterImageUrlHelper(hbs);
            RegisterScriptHelper(hbs);
            RegisterHandlebarsHelper(hbs);
            RegisterRegisterStylesheetHelper(hbs, page, sourceFolder);
            RegisterRegisterScriptHelper(hbs, page, sourceFolder);
            //RegisterEditUrlHelper(hbs, module);
            RegisterArrayIndexHelper(hbs);
            RegisterArrayTranslateHelper(hbs);
            var template = hbs.Compile(source);
            var result = template(model);
            return result;
        }
        private void RegisterTemplate(HandlebarsDotNet.IHandlebars hbs, string name, string sourceFilename)
        {
            string fileName = System.Web.Hosting.HostingEnvironment.MapPath(sourceFilename);
            if (File.Exists(fileName))
            {
                using (var reader = new StreamReader(fileName))
                {
                    var partialTemplate = hbs.Compile(reader);
                    hbs.RegisterTemplate(name, partialTemplate);
                }
            }
        }
        private void RegisterMultiplyHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("multiply", (writer, context, parameters) =>
            {
                try
                {
                    int a = int.Parse(parameters[0].ToString());
                    int b = int.Parse(parameters[1].ToString());
                    int c = a * b;
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, c.ToString());
                }
                catch (Exception)
                {
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "0");
                }
            });
        }
        private void RegisterDivideHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("divide", (writer, context, parameters) =>
            {
                try
                {
                    int a = int.Parse(parameters[0].ToString());
                    int b = int.Parse(parameters[1].ToString());
                    int c = a / b;
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, c.ToString());
                }
                catch (Exception)
                {
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "0");
                }
            });
        }
        private void RegisterEqualHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("equal", (writer, options, context, arguments) =>
            {
                if (arguments.Length == 2 && arguments[0].Equals(arguments[1]))
                {
                    options.Template(writer, (object)context);
                }
                else
                {
                    options.Inverse(writer, (object)context);
                }
            });
        }
        private void RegisterScriptHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("script", (writer, options, context, arguments) =>
            {
                HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "<script>");
                options.Template(writer, (object)context);
                HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "</script>");
            });

        }
        private void RegisterHandlebarsHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("handlebars", (writer, options, context, arguments) =>
            {
                HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "<script id=\"jplist-templatex\" type=\"text/x-handlebars-template\">");
                HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, context);

                //options.Template(writer, (object)context);
                HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "</script>");
            });

        }
        private void RegisterRegisterScriptHelper(HandlebarsDotNet.IHandlebars hbs, Page page, string sourceFolder)
        {
            hbs.RegisterHelper("registerscript", (writer, context, parameters) =>
            {
                if (parameters.Length == 1)
                {
                    string jsfilename = parameters[0].ToString();
                    if (!jsfilename.StartsWith("/") && !jsfilename.Contains("//"))
                    {
                        jsfilename = sourceFolder + jsfilename;
                    }
                    ClientResourceManager.RegisterScript(page, page.ResolveUrl(jsfilename), JSOrder++/*FileOrder.Js.DefaultPriority*/);
                    //writer.WriteSafeString(Page.ResolveUrl(jsfilename));
                }
            });
        }
        private void RegisterRegisterStylesheetHelper(HandlebarsDotNet.IHandlebars hbs, Page page, string sourceFolder)
        {
            hbs.RegisterHelper("registerstylesheet", (writer, context, parameters) =>
            {
                if (parameters.Length == 1)
                {
                    string cssfilename = parameters[0].ToString();
                    if (!cssfilename.StartsWith("/") && !cssfilename.Contains("//"))
                    {
                        cssfilename = sourceFolder + cssfilename;
                    }
                    ClientResourceManager.RegisterStyleSheet(page, page.ResolveUrl(cssfilename), FileOrder.Css.PortalCss);
                }
            });
        }
        private void RegisterEditUrlHelper(HandlebarsDotNet.IHandlebars hbs, IModuleControl module)
        {
            hbs.RegisterHelper("editurl", (writer, context, parameters) =>
            {
                if (parameters.Length == 1)
                {
                    string id = parameters[0] as string;
                    writer.WriteSafeString(module.ModuleContext.EditUrl("itemid", id));
                }
            });
        }
        private void RegisterImageUrlHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("imageurl", (writer, context, parameters) =>
            {
                if (parameters.Length == 3)
                {
                    string imageId = parameters[0] as string;
                    int width = Normalize.DynamicValue(parameters[1], -1);
                    string ratiostring = parameters[2] as string;
                    bool isMobile = HttpContext.Current.Request.Browser.IsMobileDevice;

                    var imageObject = Convert.ToInt32(imageId) == 0 ? null : new ImageUri(Convert.ToInt32(imageId));
                    var imageUrl = imageObject == null ? string.Empty : imageObject.GetImageUrl(width, ratiostring, isMobile);

                    writer.WriteSafeString(imageUrl);
                }
            });
        }
        private void RegisterArrayIndexHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("arrayindex", (writer, context, parameters) =>
            {
                try
                {
                    object[] a;
                    if (parameters[0] is IEnumerable<Object>)
                    {
                        var en = parameters[0] as IEnumerable<Object>;
                        a = en.ToArray();
                    }
                    else
                    {
                        a = (object[])parameters[0];
                    }


                    int b = int.Parse(parameters[1].ToString());
                    object c = a[b];
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, c.ToString());
                }
                catch (Exception)
                {
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "");
                }
            });

        }

        private void RegisterArrayTranslateHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("arraytranslate", (writer, context, parameters) =>
            {
                try
                {
                    object[] a;
                    if (parameters[0] is IEnumerable<Object>)
                    {
                        var en = parameters[0] as IEnumerable<Object>;
                        a = en.ToArray();
                    }
                    else
                    {
                        a = (object[])parameters[0];
                    }
                    object[] b;
                    if (parameters[1] is IEnumerable<Object>)
                    {
                        var en = parameters[1] as IEnumerable<Object>;
                        b = en.ToArray();
                    }
                    else
                    {
                        b = (object[])parameters[1];
                    }
                    string c = parameters[2].ToString();
                    int i = Array.IndexOf(a, c);

                    object res = b[i];
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, res.ToString());
                }
                catch (Exception)
                {
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "");
                }
            });

        }

        private void RegisterFormatNumberHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("formatNumber", (writer, context, parameters) =>
            {
                try
                {
                    decimal? number = parameters[0] as decimal?;
                    string format = parameters[1].ToString();
                    string provider = parameters[2].ToString();

                    IFormatProvider formatprovider = null;
                    if (provider.ToLower() == "invariant")
                    {
                        formatprovider = CultureInfo.InvariantCulture;
                    }
                    else if (!string.IsNullOrWhiteSpace(provider))
                    {
                        formatprovider = CultureInfo.CreateSpecificCulture(provider);
                    }

                    string res = number.Value.ToString(format, formatprovider);
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, res);
                }
                catch (Exception)
                {
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "");
                }
            });
        }
        private void RegisterFormatDateTimeHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("formatDateTime", (writer, context, parameters) =>
            {
                try
                {
                    string res;
                    DateTime? datetime = parameters[0] as DateTime?;
                    string format = parameters[1].ToString();
                    if (parameters.Count() > 1 && !string.IsNullOrWhiteSpace(parameters[2].ToString()))
                    {
                        string provider = parameters[2].ToString();
                        IFormatProvider formatprovider = null;
                        if (provider.ToLower() == "invariant")
                        {
                            formatprovider = CultureInfo.InvariantCulture;
                        }
                        else
                        {
                            formatprovider = CultureInfo.CreateSpecificCulture(provider);
                        }
                        res = datetime.Value.ToString(format, formatprovider);
                    }
                    else
                    {
                        res = datetime.Value.ToString(format);
                    }

                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, res);
                }
                catch (Exception)
                {
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "");
                }
            });
        }
    }
}