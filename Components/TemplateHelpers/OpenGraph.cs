using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Satrabel.OpenContent.Components.TemplateHelpers
{
    public static class OpenGraph
    {
        public static void SetOpenGraphGeneral(HttpContextBase context)
        {
            var dnnpage = context.CurrentHandler as DotNetNuke.Framework.CDefault;
            if (dnnpage != null)
            {
                var placeholder = (System.Web.UI.WebControls.PlaceHolder)dnnpage.FindControl("Head").FindControl(cMETAHANDLE);
                if (placeholder != null)
                {

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = DnnUtils.GetCurrentCultureCode().Replace('-','_')
                    }, "og:locale"));

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = GetCurrentUrl()
                    }, "og:url"));

                    // indien beschikbaar datum laatst aangepast
                    // <meta property="og:updated_time" content="20150125" />
                }
            }
        }

        public static void SetOpenGraphTwitter(HttpContextBase context, string twitterAccount)
        {
            var dnnpage = context.CurrentHandler as DotNetNuke.Framework.CDefault;
            if (dnnpage != null)
            {
                var placeholder = (System.Web.UI.WebControls.PlaceHolder)dnnpage.FindControl("Head").FindControl("SocialMeta");
                if (placeholder != null)
                {
                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Name = "twitter:domain",
                        Content = HostName()
                    }, "og:site_name"));

                    placeholder.Controls.Add(new HtmlMeta
                    {
                        Name = "twitter:site",
                        Content = twitterAccount
                    });
                    placeholder.Controls.Add(new HtmlMeta
                    {
                        Name = "twitter:creator",
                        Content = twitterAccount
                    });
                }
            }
        }

        #region OpenGraph Articles

        public class OgArticle
        {
            public OgArticle(string title, string description, string imageUrl)
            {
                title = Utils.HtmlDecodeIfNeeded(title);
                title = Utils.HtmlRemoval.StripTagsRegexCompiled(title);
                description = Utils.HtmlDecodeIfNeeded(description);
                description = Utils.HtmlRemoval.StripTagsRegexCompiled(description);

                Title = title;
                Description = description;
                ImageUrl = imageUrl;
            }

            public string Title { get; private set; }
            public string Description { get; private set; }
            public string ImageUrl { get; private set; }
        }

        public static void SetOpenGraphArticle(Page page, OgArticle ogArticle)
        {
            var dnnpage = page as DotNetNuke.Framework.CDefault;
            if (dnnpage != null)
            {
                var head = (HtmlHead)dnnpage.FindControl("Head");
                var placeholder = (System.Web.UI.WebControls.PlaceHolder)head.FindControl(cMETAHANDLE);
                if (placeholder != null)
                {
                    head.Attributes.Add("prefix", "og: http://ogp.me/ns# fb: http://ogp.me/ns/fb#");

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = "article",
                    }, "og:type"));
                    placeholder.Controls.Add(new HtmlMeta
                    {
                        Name = "twitter:card",
                        Content = "summary_large_image"
                    });

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        //Name = "twitter:title",
                        Content = ogArticle.Title
                    }, "og:title"));

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        //Name = "twitter:description",
                        Content = ogArticle.Description
                    }, "og:description"));

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        //Name = "twitter:image",
                        Content = GetBaseUrl() + ogArticle.ImageUrl
                    }, "og:image"));
                }
            }
        }

        public static void SetOpenGraphArticle(HttpContextBase context, OgArticle ogArticle)
        {
            var pageObj = context.CurrentHandler as System.Web.UI.Page;
            if (pageObj != null)
            {
                SetOpenGraphArticle(pageObj, ogArticle);
            }
        }

        #endregion
        
        #region OpenGraph Products

        public class Product
        {
            public string ProductId { get; set; }
            public string ManufacturerProductId { get; set; }
            public string Description { get; set; }
            public string SubGroupDescription { get; set; }
            public string ImageUrl { get; set; }
            public string Price { get; set; }
            public string Brand { get; set; }
            public bool InStock { get; set; }
        }

        public static void SetOpenGraphProduct(Page page, string retailerName, Product product)
        {
            var dnnpage = page as DotNetNuke.Framework.CDefault;
            if (dnnpage != null)
            {
                var head = (HtmlHead)dnnpage.FindControl("Head");
                var placeholder = (System.Web.UI.WebControls.PlaceHolder)head.FindControl(cMETAHANDLE);
                if (placeholder != null)
                {
                    head.Attributes.Add("prefix", "og: http://ogp.me/ns# fb: http://ogp.me/ns/fb# business: http://ogp.me/ns/business# product: http://ogp.me/ns/product#");

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = "product",
                    }, "og:type"));
                    placeholder.Controls.Add(new HtmlMeta
                    {
                        Name = "twitter:card",
                        Content = "summary_large_image"
                    });

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = WebUtility.HtmlEncode(product.Description)
                    }, "og:title"));

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = WebUtility.HtmlEncode(product.SubGroupDescription)
                    }, "og:description"));

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = product.ImageUrl
                    }, "og:image"));

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = "1200"
                    }, "og:image:width"));

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = "630"
                    }, "og:image:height"));

                    // Speciaal voor producten
                    if (product.InStock)
                    {
                        placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                        {
                            Content = "instock"
                        }, "product:availability"));
                    }
                    else
                    {
                        placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                        {
                            Content = "oos"
                        }, "product:availability"));
                    }

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = product.Price
                    }, "product:price:amount"));

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = "EUR"
                    }, "product:price:currency"));

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = retailerName
                    }, "product:retailer_title"));

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = product.ProductId
                    }, "product:retailer_part_no"));

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = product.ManufacturerProductId
                    }, "product:mfr_part_no"));

                    placeholder.Controls.Add(AddPropertyToMeta(new HtmlMeta
                    {
                        Content = product.Brand
                    }, "product:brand"));
                }
            }
        }

        public static void SetOpenGraphProduct(HttpContextBase context, string retailerName, Product product)
        {
            var pageObj = context.CurrentHandler as System.Web.UI.Page;
            if (pageObj != null)
            {
                SetOpenGraphProduct(pageObj, retailerName, product);
            }
        }

        #endregion

        #region Private Methods

        private const string cMETAHANDLE = "metaPanel";

        private static HtmlMeta AddPropertyToMeta(HtmlMeta meta, string value)
        {
            meta.Attributes.Add("property", value);
            return meta;
        }

        private static string GetBaseUrl()
        {
            if (HttpContext.Current == null) return "";

            return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
        }

        private static string GetCurrentUrl()
        {
            if (HttpContext.Current == null) return "";

            return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.RawUrl;
        }
        private static string HostName()
        {
            if (HttpContext.Current == null) return "";

            var host = HttpContext.Current.Request.Url.Host;
            var hostNameSplit = host.Split('.');
            return hostNameSplit.Length > 1 ? string.Concat(hostNameSplit[hostNameSplit.Length - 2], ".", hostNameSplit[hostNameSplit.Length - 1]) : host;
        }

        #endregion
    }
}