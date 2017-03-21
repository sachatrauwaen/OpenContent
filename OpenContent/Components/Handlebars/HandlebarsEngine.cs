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
using Satrabel.OpenContent.Components.Dynamic;
using System.Collections;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Logging;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Json;

namespace Satrabel.OpenContent.Components.Handlebars
{
    public class HandlebarsEngine
    {
        private Func<object, string> _template;
        private int _jsOrder = 100;

        public void Compile(string source)
        {
            try
            {
                //register server side helpers
                var hbs = HandlebarsDotNet.Handlebars.Create();
                RegisterDivideHelper(hbs);
                RegisterMultiplyHelper(hbs);
                RegisterAdditionHelper(hbs);
                RegisterSubstractionHelper(hbs);
                RegisterEqualHelper(hbs);
                RegisterFormatNumberHelper(hbs);
                RegisterFormatDateTimeHelper(hbs);
                RegisterImageUrlHelper(hbs);
                RegisterArrayIndexHelper(hbs);
                RegisterArrayTranslateHelper(hbs);
                RegisterIfAndHelper(hbs);
                RegisterConvertHtmlToTextHelper(hbs);
                RegisterConvertToJsonHelper(hbs);
                RegisterTruncateWordsHelper(hbs);
                _template = hbs.Compile(source);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Failed to render Handlebar template source:[{source}]", ex);
                throw new TemplateException("Failed to render Handlebar template " + source, ex, null, source);
            }
        }

        public string Execute(Dictionary<string, object> model)
        {
            try
            {
                var result = _template(model);
                return result;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(string.Format("Failed to execute Handlebar template with model:[{1}]", "", model), ex);
                throw new TemplateException("Failed to render Handlebar template ", ex, model, "");
            }
        }

        public string Execute(string source, dynamic model)
        {
            try
            {
                var hbs = HandlebarsDotNet.Handlebars.Create();
                RegisterHelpers(hbs);
                return CompileTemplate(hbs, source, model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Failed to render Handlebar template source:[{source}], model:[{model}]", ex);
                throw new TemplateException("Failed to render Handlebar template ", ex, model, source);
            }
        }
        public string ExecuteWithoutFaillure(string source, Dictionary<string, object> model, string defaultValue)
        {
            try
            {
                var hbs = HandlebarsDotNet.Handlebars.Create();
                RegisterHelpers(hbs);
                return CompileTemplate(hbs, source, model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Failed to render Handlebar template source:[{source}], model:[{model}]", ex);
            }
            return defaultValue;
        }

        private void RegisterHelpers(IHandlebars hbs)
        {
            RegisterDivideHelper(hbs);
            RegisterMultiplyHelper(hbs);
            RegisterAdditionHelper(hbs);
            RegisterSubstractionHelper(hbs);
            RegisterEqualHelper(hbs);
            RegisterFormatNumberHelper(hbs);
            RegisterFormatDateTimeHelper(hbs);
            RegisterImageUrlHelper(hbs);
            RegisterArrayIndexHelper(hbs);
            RegisterArrayTranslateHelper(hbs);
            RegisterArrayLookupHelper(hbs);
            RegisterIfAndHelper(hbs);
            RegisterIfInHelper(hbs);
            RegisterEachPublishedHelper(hbs);
            RegisterConvertHtmlToTextHelper(hbs);
            RegisterConvertToJsonHelper(hbs);
            RegisterTruncateWordsHelper(hbs);
        }

        private void RegisterTruncateWordsHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("truncateWords", (writer, context, parameters) =>
            {
                try
                {
                    string html = parameters[0].ToString();
                    int maxCharacters = int.Parse(parameters[1].ToString());
                    string trailingText = "";
                    if (parameters.Count() > 2)
                    {
                        trailingText = parameters[2].ToString();
                    }
                    string res = html.TruncateWords(maxCharacters, trailingText);
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, res);
                }
                catch (Exception)
                {
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "");
                }
            });
        }
        public string Execute(Page page, FileUri sourceFileUri, dynamic model)
        {
            try
            {
                string source = File.ReadAllText(sourceFileUri.PhysicalFilePath);
                string sourceFolder = sourceFileUri.UrlFolder; //.Replace("\\", "/") + "/";
                var hbs = HandlebarsDotNet.Handlebars.Create();
                RegisterHelpers(hbs);
                RegisterScriptHelper(hbs);
                RegisterHandlebarsHelper(hbs);
                RegisterRegisterStylesheetHelper(hbs, page, sourceFolder);
                RegisterRegisterScriptHelper(hbs, page, sourceFolder);
                return CompileTemplate(hbs, source, model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(string.Format("Failed to render Handlebar template source:[{0}], model:[{1}]", sourceFileUri, model), ex);
                throw new TemplateException("Failed to render Handlebar template " + sourceFileUri.FilePath, ex, model, sourceFileUri.FilePath);
            }
        }
        public string Execute(Page page, TemplateFiles files, string templateVirtualFolder, dynamic model)
        {
            var sourceFileUri = new FileUri(templateVirtualFolder + "/" + files.Template);
            try
            {
                string source = File.ReadAllText(sourceFileUri.PhysicalFilePath);
                string sourceFolder = sourceFileUri.UrlFolder;
                var hbs = HandlebarsDotNet.Handlebars.Create();
                
                RegisterHelpers(hbs);
                RegisterScriptHelper(hbs);
                RegisterHandlebarsHelper(hbs);
                RegisterRegisterStylesheetHelper(hbs, page, sourceFolder);
                RegisterRegisterScriptHelper(hbs, page, sourceFolder);
                if (files.PartialTemplates != null)
                {
                    foreach (var part in files.PartialTemplates.Where(t => t.Value.ClientSide == false))
                    {
                        RegisterTemplate(hbs, part.Key, templateVirtualFolder + "/" + part.Value.Template);
                    }
                }
                return CompileTemplate(hbs, source, model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(string.Format("Failed to render Handlebar template source:[{0}], model:[{1}]", sourceFileUri.PhysicalFilePath, model), ex);
                throw new TemplateException("Failed to render Handlebar template " + sourceFileUri.PhysicalFilePath, ex, model, sourceFileUri.PhysicalFilePath);
            }
        }

        private string CompileTemplate(IHandlebars hbs, string source, object model)
        {
            var compiledTemplate = hbs.Compile(source);
            return compiledTemplate(model);
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
        private void RegisterAdditionHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("add", (writer, context, parameters) =>
            {
                try
                {
                    int a = int.Parse(parameters[0].ToString());
                    int b = int.Parse(parameters[1].ToString());
                    int c = a + b;
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, c.ToString());
                }
                catch (Exception)
                {
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "0");
                }
            });
        }
        private void RegisterSubstractionHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("substract", (writer, context, parameters) =>
            {
                try
                {
                    int a = int.Parse(parameters[0].ToString());
                    int b = int.Parse(parameters[1].ToString());
                    int c = a - b;
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, c.ToString());
                }
                catch (Exception)
                {
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "0");
                }
            });
        }
        /// <summary>
        /// A block helper.
        /// Returns nothing, executes the template-part if contidions are met.
        /// </summary>
        /// <param name="hbs">The HBS.</param>
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
            hbs.RegisterHelper("equaldate", (writer, options, context, arguments) =>
            {                
                try
                {
                    DateTime datetime1 = DateTime.Parse(arguments[0].ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind);
                    DateTime datetime2 = DateTime.Parse(arguments[1].ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind);
                    if (datetime1.Date == datetime2.Date)
                    {
                        options.Template(writer, (object)context);
                    }
                    else
                    {
                        options.Inverse(writer, (object)context);
                    }
                }
                catch (Exception)
                {
                    options.Inverse(writer, (object)context);
                }
            });
        }
        private void RegisterEachPublishedHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("published", (writer, options, context, parameters) =>
            {
                bool EditMode = PortalSettings.Current.UserMode == PortalSettings.Mode.Edit;
                if (EditMode)
                {
                    options.Template(writer, parameters[0]);
                }
                else
                {
                    var lst = new List<dynamic>();
                    foreach (dynamic item in parameters[0] as IEnumerable)
                    {
                        bool show = true;
                        try
                        {
                            if (item.publishstatus != "published")
                            {
                                show = false;
                            }
                        }
                        catch (Exception)
                        {
                        }
                        try
                        {
                            DateTime publishstartdate = DateTime.Parse(item.publishstartdate, null, System.Globalization.DateTimeStyles.RoundtripKind);
                            if (publishstartdate.Date >= DateTime.Today)
                            {
                                show = false;
                            }
                        }
                        catch (Exception)
                        {
                        }
                        try
                        {
                            DateTime publishenddate = DateTime.Parse(item.publishenddate, null, System.Globalization.DateTimeStyles.RoundtripKind);
                            if (publishenddate.Date <= DateTime.Today)
                            {
                                show = false;
                            }
                        }
                        catch (Exception)
                        {
                        }
                        if (show)
                        {
                            lst.Add(item);
                        }

                    }
                    options.Template(writer, lst);
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
                    DnnUtils.RegisterScript(page, sourceFolder, jsfilename, _jsOrder);
                    _jsOrder++;
                }
            });
            hbs.RegisterHelper("registerform", (writer, context, parameters) =>
            {
                string view = "bootstrap";
                if (parameters.Length == 1)
                {
                    view = parameters[0].ToString();
                }

                FormHelpers.RegisterForm(page, sourceFolder, view, ref _jsOrder);

            });

            hbs.RegisterHelper("registereditform", (writer, context, parameters) =>
            {
                string prefix = "";
                if (parameters.Length == 1)
                {
                    prefix = parameters[0].ToString();
                }
                FormHelpers.RegisterEditForm(page, sourceFolder, PortalSettings.Current.PortalId, prefix, ref _jsOrder);
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
        /*
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
        */

        /// <summary>
        /// Retrieves image URL.
        /// Param1 is imageId, 
        /// Param2 is Size of the image (in Bootstrap 12th), 
        /// Param3 is ratio string (eg '1x1'), 
        /// </summary>
        /// <param name="hbs">The HBS.</param>
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

                    ImageUri imageObject = ImageUriFactory.CreateImageUri(imageId);
                    var imageUrl = imageObject == null ? string.Empty : imageObject.GetImageUrl(width, ratiostring, isMobile);

                    writer.WriteSafeString(imageUrl);
                }
                if (parameters.Length == 2)
                {
                    string imageId = parameters[0] as string;
                    int width = Normalize.DynamicValue(parameters[1], -1);
                    bool isMobile = HttpContext.Current.Request.Browser.IsMobileDevice;

                    ImageUri imageObject = ImageUriFactory.CreateImageUri(imageId);
                    var imageUrl = imageObject == null ? string.Empty : imageObject.GetImageUrl(width, imageObject.RawRatio, isMobile);

                    writer.WriteSafeString(imageUrl);
                }
            });
           
        }

        /// <summary>
        /// Retrieved an element from a list.
        /// First param is List, the second param is the int with the position to retrieve. 
        /// Zero-based retrieval
        /// </summary>
        /// <param name="hbs">The HBS.</param>
        private void RegisterArrayIndexHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("arrayindex", (writer, context, parameters) =>
            {
                try
                {
                    object[] a;
                    if (parameters[0] is IEnumerable<object>)
                    {
                        var en = parameters[0] as IEnumerable<object>;
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

        /// <summary>
        /// Translate an enum value
        /// parameters[0] = enum array (eg from schema)
        /// parameters[1] = label option array (from option file)
        /// parameters[2] = selected enum value
        /// </summary>
        /// <param name="hbs">The HBS.</param>
        private void RegisterArrayTranslateHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("arraytranslate", (writer, context, parameters) =>
            {
                try
                {
                    object[] a;
                    if (parameters[0] is IEnumerable<object>)
                    {
                        var en = parameters[0] as IEnumerable<object>;
                        a = en.ToArray();
                    }
                    else
                    {
                        a = (object[])parameters[0];
                    }
                    object[] b;
                    if (parameters[1] is IEnumerable<object>)
                    {
                        var en = parameters[1] as IEnumerable<object>;
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

        /// <summary>
        /// Registers the array lookup helper.
        /// arguments[0] = 
        /// arguments[1] =
        /// arguments[2] =
        /// </summary>
        /// <param name="hbs">The HBS.</param>
        private void RegisterArrayLookupHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("lookup", (writer, options, context, arguments) =>
            {
                object[] arr;
                if (arguments[0] is IEnumerable<object>)
                {
                    var en = arguments[0] as IEnumerable<object>;
                    arr = en.ToArray();
                }
                else
                {
                    arr = (object[])arguments[0];
                }

                var field = arguments[1].ToString();
                var value = arguments[2].ToString();
                foreach (var arrayItem in arr)
                {
                    object member = DynamicUtils.GetMemberValue(arrayItem, field);
                    if (value.Equals(member))
                    {
                        options.Template(writer, (object)arrayItem);
                    }
                }
                options.Inverse(writer, (object)context);
            });
            /*
            hbs.RegisterHelper("arraylookup", (writer, context, parameters) =>
            {
                try
                {
                    object[] arr;
                    if (parameters[0] is IEnumerable<Object>)
                    {
                        var en = parameters[0] as IEnumerable<Object>;
                        arr = en.ToArray();
                    }
                    else
                    {
                        arr = (object[])parameters[0];
                    }
                    
                    var value = parameters[1].ToString();
                    var text = parameters[2].ToString();
                    var key = parameters[3].ToString();
                    foreach (var item in collection)
                    {
                        
                    }
                    object res = b[i];
                    string c = parameters[2].ToString();
                    HandlebarsDotNet.HandlebarsExtensions.
                    //HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, res.ToString());
                }
                catch (Exception)
                {
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "");
                }
            });
            */
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
                // parameters : datetime iso string, format, culture
                try
                {
                    string res;
                    DateTime datetime = DateTime.Parse(parameters[0].ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind);
                    string format = "dd/MM/yyyy";
                    if (parameters.Count() > 1)
                    {
                        format = parameters[1].ToString();
                    }
                    if (parameters.Count() > 2 && !string.IsNullOrWhiteSpace(parameters[2].ToString()))
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
                        res = datetime.ToString(format, formatprovider);
                    }
                    else
                    {
                        res = datetime.ToString(format);
                    }

                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, res);
                }
                catch (Exception)
                {
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "");
                }
            });

            /*
            hbs.RegisterHelper("format2DateTime", (writer, context, parameters) =>
            {
                // parameters : start datetime iso string, end datetime iso string, date time separator, dates separator, dateformat, time format, culture
                try
                {
                    string res;
                    DateTime datetime1 = DateTime.Parse(parameters[0].ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind);
                    
                    string datetimeSeparator = " ";
                    string datesSeparator = " ";
                    string dateformat = "dd/MM/yyyy";
                    string timeformat = "hh:mm";
                    if (parameters.Count() > 2)
                    {
                        datetimeSeparator = parameters[2].ToString();
                    }
                    if (parameters.Count() > 3)
                    {
                        datesSeparator = parameters[3].ToString();
                    }
                    if (parameters.Count() > 4)
                    {
                        dateformat = parameters[4].ToString();
                    }
                    if (parameters.Count() > 5)
                    {
                        timeformat = parameters[5].ToString();
                    }
                    Func< DateTime, string, string> formatDT = (datetime, format) =>
                    {
                        if (parameters.Count() > 6 && !string.IsNullOrWhiteSpace(parameters[6].ToString()))
                        {
                            string provider = parameters[6].ToString();
                            IFormatProvider formatprovider = null;
                            if (provider.ToLower() == "invariant")
                            {
                                formatprovider = CultureInfo.InvariantCulture;
                            }
                            else
                            {
                                formatprovider = CultureInfo.CreateSpecificCulture(provider);
                            }
                            return datetime.ToString(format, formatprovider);
                        }
                        else
                        {
                            return datetime.ToString(format);
                        }
                    };
                    res = formatDT(datetime1, dateformat);
                    if (datetime1.TimeOfDay.Ticks > 0)
                    {
                        res += datetimeSeparator;
                        res += formatDT(datetime1, timeformat);
                    }
                    if (!string.IsNullOrEmpty(parameters[1].ToString()))
                    {
                        DateTime datetime2 = DateTime.Parse(parameters[1].ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind);
                        res += datesSeparator;
                        if (datetime1.Date != datetime2.Date)
                        {
                            res += formatDT(datetime2, dateformat);
                        }
                        if (datetime2.TimeOfDay.Ticks > 0)
                        {
                            res += datetimeSeparator;
                            res += formatDT(datetime2, timeformat);
                        }
                    }
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, res);
                }
                catch (Exception)
                {
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "");
                }
            });
            */
        }
        private void RegisterIfAndHelper(IHandlebars hbs)
        {
            hbs.RegisterHelper("ifand", (writer, options, context, arguments) =>
            {
                bool res = true;
                foreach (var arg in arguments)
                {
                    res = res && HandlebarsUtils.IsTruthyOrNonEmpty(arg);
                }
                if (res)
                {
                    options.Template(writer, (object)context);
                }
                else
                {
                    options.Inverse(writer, (object)context);
                }
            });
        }
        private void RegisterIfInHelper(IHandlebars hbs)
        {
            hbs.RegisterHelper("ifin", (writer, options, context, arguments) =>
            {
                bool res = false;
                if (arguments.Length > 1)
                {
                    for (int i = 1; i < arguments.Length; i++)
                    {
                        res = res || arguments[0].Equals(arguments[i]);
                    }
                }
                if (res)
                {
                    options.Template(writer, (object)context);
                }
                else
                {
                    options.Inverse(writer, (object)context);
                }
            });
        }
        private void RegisterConvertHtmlToTextHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("convertHtmlToText", (writer, context, parameters) =>
            {
                try
                {
                    string html = parameters[0].ToString();
                    string res = DotNetNuke.Services.Mail.Mail.ConvertToText(html);
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, res);
                }
                catch (Exception)
                {
                    HandlebarsDotNet.HandlebarsExtensions.WriteSafeString(writer, "");
                }
            });
        }
        private void RegisterConvertToJsonHelper(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("convertToJson", (writer, context, parameters) =>
            {
                try
                {
                    var res = System.Web.Helpers.Json.Encode(parameters[0]);
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