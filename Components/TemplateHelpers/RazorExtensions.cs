using System;
using System.Collections.Generic;
using System.Web.WebPages;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class RazorExtensions
    {
        /// <summary>
        /// Lists the items by the specified template.
        /// See http://haacked.com/archive/2011/02/27/templated-razor-delegates.aspx/
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TSettings"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="template">The template.</param>
        /// <param name="model"></param>
        /// <param name="settings"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static HelperResult Render<T, TModel, TSettings>(this IEnumerable<T> items, Func<string, T, TModel, TSettings, HelperResult> template, TModel model, TSettings settings, string id)
        {
            return new HelperResult(writer =>
            {
                foreach (var item in items)
                {
                    template(id, item, model, settings).WriteTo(writer);
                }
            });
        }
    }
}