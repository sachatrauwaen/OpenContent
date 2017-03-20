using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OpenContentTests
{
    [TestClass]
    public class HandlebarsTests
    {
        [TestMethod]
        public void EachFromJson()
        {
            string expected = "123";
            string dataJson = "{\"lst\":[{\"data\":1},{\"data\":2},{\"data\":3}]}";
            dynamic model = JsonUtils.JsonToDynamic(dataJson);
            string source = "{{#each lst}}{{data}}{{/each}}";
            HandlebarsEngine hbEngine = new HandlebarsEngine();
            string res = hbEngine.Execute(source, model);
            Assert.AreEqual(expected, res);
        }

        [TestMethod]
        public void DivideHelper()
        {
            string expected = "2";
            string source = "{{divide data \"5\"}}";
            dynamic model = new { data = 10 };
            HandlebarsEngine hbEngine = new HandlebarsEngine();
            string res = hbEngine.Execute(source, model);
            Assert.AreEqual(expected, res);
        }
        [TestMethod]
        public void MultiplyHelper()
        {
            string expected = "50";
            string source = "{{multiply data \"5\"}}";
            dynamic model = new { data = 10 };
            HandlebarsEngine hbEngine = new HandlebarsEngine();
            string res = hbEngine.Execute(source, model);
            Assert.AreEqual(expected, res);
        }
        [TestMethod]
        public void EqualHelper()
        {
            string expected1 = "no";
            string expected2 = "yes";
            string source = "{{#equal data \"5\"}}yes{{else}}no{{/equal}}";
            dynamic model1 = new { data = "10" };
            dynamic model2 = new { data = "5" };
            HandlebarsEngine hbEngine = new HandlebarsEngine();
            string res1 = hbEngine.Execute(source, model1);
            string res2 = hbEngine.Execute(source, model2);
            Assert.AreEqual(expected1, res1);
            Assert.AreEqual(expected2, res2);
        }

        [TestMethod]
        public void ParseDate()
        {
            JTokenType expected1 = JTokenType.Date;
            string source = "{ \"d\":\"2009-02-15T00:00:00\" }";
            JObject obj = JObject.Parse(source);
            var token = obj["d"];
            JValue value = token as JValue;
            var t = value.Type;
            Assert.AreEqual(expected1, t);
        }

        [TestMethod]
        public void JavaScriptSerializer()
        {
            // article
            string dataJson = "{\r\n  \"Category\": \"b1f2b3b4-c0f8-4c35-b81d-6bc08c023f48\",\r\n  \"Title\": \"Flex slider\",\r\n  \"Summary\": \"\\n\\nAn awesome, fully responsive jQuery slider toolkit.\\n\\n<br>Slider &amp; Carousel (1 of the 2 or both)<br>\",\r\n  \"Description\": \"\\n\\n<p>An awesome, fully responsive jQuery slider toolkit.</p><ul><li>Simple, semantic markup</li><li>Supported in all major browsers</li><li>Horizontal/vertical slide and fade animations</li><li>Multiple slider support, Callback API, and more</li><li>Hardware accelerated touch swipe support</li><li>Custom navigation options</li><li>Compatible with the latest version of jQuery</li></ul>Slider &amp; Carousel (1 of the 2 or both)<br><b>More info</b>: <a rel=\\\"nofollow\\\" target=\\\"_blank\\\" href=\\\"http://www.woothemes.com/flexslider/\\\">http://www.woothemes.com/flexslider/</a><br><b>Requirements</b>: OpenContent 1.2 or higher\\n\\n\\n<br>\",\r\n  \"Image\": \"/Portals/0/OpenContent/Files/691/flexslider.JPG\",\r\n  \"Gallery\": [\r\n    {\r\n      \"Image\": \"/Portals/0/OpenContent/Files/691/Chrysanthemum.jpg\"\r\n    },\r\n    {\r\n      \"Image\": \"/Portals/0/OpenContent/Files/691/Desert.jpg\"\r\n    },\r\n    {\r\n      \"Image\": \"/Portals/0/OpenContent/Files/691/Hydrangeas.jpg\"\r\n    },\r\n    {\r\n      \"Image\": \"/Portals/0/OpenContent/Files/691/Jellyfish.jpg\"\r\n    },\r\n    {\r\n      \"Image\": \"/Portals/0/OpenContent/Files/691/Koala.jpg\"\r\n    },\r\n    {\r\n      \"Image\": \"/Portals/0/OpenContent/Files/691/Penguins.jpg\"\r\n    },\r\n    {\r\n      \"Image\": \"/Portals/0/OpenContent/Files/691/Tulips.jpg\"\r\n    }\r\n  ],\r\n  \"Documents\": [],\r\n  \"Seo\": {\r\n    \"Slug\": \"Flex slider\",\r\n    \"MetaTitle\": \"Flex slider Template for DNN Open Content | Open Extensions\"\r\n  },\r\n  \"Tags\": [\r\n    \"502ec9fb-33f5-4c3f-ba4a-5c8defbeaf57\"\r\n  ],\r\n  \"Featured\": false,\r\n  \"publishstartdate\": \"2016-01-26T00:00:00+01:00\",\r\n  \"publishenddate\": \"2099-12-31T00:00:00+01:00\",\r\n  \"publishstatus\": \"published\"\r\n}";
            StringBuilder data = new StringBuilder("{ \"Items\": [");
            int itemCount = 10;
            for (int i = 0; i < itemCount; i++)
            {
                data.Append(dataJson);
                if (i < itemCount - 1)
                    data.Append(", ");
            }
            data.Append("] }");

            dynamic expectedModel = JsonUtils.JsonToDynamic(data.ToString());
            var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            Dictionary<string, object> model = (Dictionary<string, object>)jsSerializer.DeserializeObject(data.ToString());
            string source = "{{#each Items}}\r\n    <!-- item {{@index}} -->\r\n        <div class=\"row article\">\r\n            <div class=\"col-sm-12 col-md-12\">\r\n                <div class=\"row\">\r\n                    <div class=\"col-md-4\">\r\n                        <a href=\"{{Context.DetailUrl}}\"><img src=\"{{Image}}\" alt=\"\" title=\"\" class=\"img-thumbnail img-responsive\" /></a>\r\n                    </div>\r\n                    <div class=\"caption col-md-8\">\r\n                        <h3 class=\"title\"><a href=\"{{Context.DetailUrl}}\">{{Title}}</a></h3>\r\n                        <p class=\"desc\">{{{Summary}}}</p>\r\n                        <p><a href=\"{{Context.DetailUrl}}\" class=\"btn btn-default\" role=\"button\">Read more</a></p>\r\n                        <p class=\"theme\">\r\n                            <span class=\"fa fa-calendar\" aria-hidden=\"true\"></span>\r\n                            {{formatDateTime publishstartdate \"DD/MM/YYYY\"}}\r\n                            <span class=\"fa fa-tags\" aria-hidden=\"true\"></span>\r\n                            {{#if Category}}\r\n                            <span>{{Category.Title}}</span>\r\n                            {{/if}}\r\n                        </p>\r\n                    </div>\r\n                </div>\r\n               \r\n            </div>\r\n        </div>\r\n    </div>\r\n    {{/each}}";
            HandlebarsEngine hbEngine = new HandlebarsEngine();
            string expectedRes = hbEngine.Execute(source, expectedModel);
            string res = hbEngine.Execute(source, model);
            Assert.AreEqual(expectedRes, res);
        }

        [TestMethod]
        public void HandlebarsPerformance()
        {
            bool optimized = false;
            DateTime now = DateTime.Now;
            // article
            string dataJson = "{\r\n  \"Category\": \"b1f2b3b4-c0f8-4c35-b81d-6bc08c023f48\",\r\n  \"Title\": \"Flex slider\",\r\n  \"Summary\": \"\\n\\nAn awesome, fully responsive jQuery slider toolkit.\\n\\n<br>Slider &amp; Carousel (1 of the 2 or both)<br>\",\r\n  \"Description\": \"\\n\\n<p>An awesome, fully responsive jQuery slider toolkit.</p><ul><li>Simple, semantic markup</li><li>Supported in all major browsers</li><li>Horizontal/vertical slide and fade animations</li><li>Multiple slider support, Callback API, and more</li><li>Hardware accelerated touch swipe support</li><li>Custom navigation options</li><li>Compatible with the latest version of jQuery</li></ul>Slider &amp; Carousel (1 of the 2 or both)<br><b>More info</b>: <a rel=\\\"nofollow\\\" target=\\\"_blank\\\" href=\\\"http://www.woothemes.com/flexslider/\\\">http://www.woothemes.com/flexslider/</a><br><b>Requirements</b>: OpenContent 1.2 or higher\\n\\n\\n<br>\",\r\n  \"Image\": \"/Portals/0/OpenContent/Files/691/flexslider.JPG\",\r\n  \"Gallery\": [\r\n    {\r\n      \"Image\": \"/Portals/0/OpenContent/Files/691/Chrysanthemum.jpg\"\r\n    },\r\n    {\r\n      \"Image\": \"/Portals/0/OpenContent/Files/691/Desert.jpg\"\r\n    },\r\n    {\r\n      \"Image\": \"/Portals/0/OpenContent/Files/691/Hydrangeas.jpg\"\r\n    },\r\n    {\r\n      \"Image\": \"/Portals/0/OpenContent/Files/691/Jellyfish.jpg\"\r\n    },\r\n    {\r\n      \"Image\": \"/Portals/0/OpenContent/Files/691/Koala.jpg\"\r\n    },\r\n    {\r\n      \"Image\": \"/Portals/0/OpenContent/Files/691/Penguins.jpg\"\r\n    },\r\n    {\r\n      \"Image\": \"/Portals/0/OpenContent/Files/691/Tulips.jpg\"\r\n    }\r\n  ],\r\n  \"Documents\": [],\r\n  \"Seo\": {\r\n    \"Slug\": \"Flex slider\",\r\n    \"MetaTitle\": \"Flex slider Template for DNN Open Content | Open Extensions\"\r\n  },\r\n  \"Tags\": [\r\n    \"502ec9fb-33f5-4c3f-ba4a-5c8defbeaf57\"\r\n  ],\r\n  \"Featured\": false,\r\n  \"publishstartdate\": \"2016-01-26T00:00:00+01:00\",\r\n  \"publishenddate\": \"2099-12-31T00:00:00+01:00\",\r\n  \"publishstatus\": \"published\"\r\n}";
            StringBuilder data = new StringBuilder("{ \"Items\": [");
            int itemCount = 1000;
            for (int i = 0; i < itemCount; i++)
            {
                data.Append(dataJson);
                if (i < itemCount - 1)
                    data.Append(", ");
            }
            data.Append("] }");
            string source = "{{#each Items}}\r\n    <!-- item {{@index}} -->\r\n        <div class=\"row article\">\r\n            <div class=\"col-sm-12 col-md-12\">\r\n                <div class=\"row\">\r\n                    <div class=\"col-md-4\">\r\n                        <a href=\"{{Context.DetailUrl}}\"><img src=\"{{Image}}\" alt=\"\" title=\"\" class=\"img-thumbnail img-responsive\" /></a>\r\n                    </div>\r\n                    <div class=\"caption col-md-8\">\r\n                        <h3 class=\"title\"><a href=\"{{Context.DetailUrl}}\">{{Title}}</a></h3>\r\n                        <p class=\"desc\">{{{Summary}}}</p>\r\n                        <p><a href=\"{{Context.DetailUrl}}\" class=\"btn btn-default\" role=\"button\">Read more</a></p>\r\n                        <p class=\"theme\">\r\n                            <span class=\"fa fa-calendar\" aria-hidden=\"true\"></span>\r\n                            {{formatDateTime publishstartdate \"DD/MM/YYYY\"}}\r\n                            <span class=\"fa fa-tags\" aria-hidden=\"true\"></span>\r\n                            {{#if Category}}\r\n                            <span>{{Category.Title}}</span>\r\n                            {{/if}}\r\n                        </p>\r\n                    </div>\r\n                </div>\r\n               \r\n            </div>\r\n        </div>\r\n    </div>\r\n    {{/each}}";
            HandlebarsEngine hbEngine = new HandlebarsEngine();
            //var model = JObject.Parse(data.ToString()).ToDictionary();
            //var model = deserializeToDictionary(data.ToString());
            if (optimized)
            {
                var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, object> model = (Dictionary<string, object>)jsSerializer.DeserializeObject(data.ToString());
                string res = hbEngine.Execute(source, model);
            }
            else
            {
                dynamic model = JsonUtils.JsonToDynamic(data.ToString());
                string res = hbEngine.Execute(source, model);
            }
            DateTime later = DateTime.Now;
            Assert.IsTrue(later < now.AddMilliseconds(500));
        }

        private Dictionary<string, object> deserializeToDictionary(string jo)
        {
            var values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(jo);
            var values2 = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> d in values)
            {
                if (d.Value != null && d.Value.GetType().FullName.Contains("Newtonsoft.Json.Linq.JObject"))
                {
                    values2.Add(d.Key, deserializeToDictionary(d.Value.ToString()));
                }
                else if (d.Value != null && d.Value.GetType().FullName.Contains("Newtonsoft.Json.Linq.JArray"))
                {
                    var lst = new List<object>();
                    foreach (var item in d.Value as JArray)
                    {
                        if (item is JValue)
                            lst.Add(item.ToString());
                        else
                            lst.Add(deserializeToDictionary(item.ToString()));

                    }

                    values2.Add(d.Key, lst);
                }
                else
                {
                    values2.Add(d.Key, d.Value);
                }
            }
            return values2;
        }


    }

    public static class JObjectExtensions
    {
        public static IDictionary<string, object> ToDictionary(this JObject @object)
        {
            var result = @object.ToObject<Dictionary<string, object>>();

            var JObjectKeys = (from r in result
                               let key = r.Key
                               let value = r.Value
                               where value.GetType() == typeof(JObject)
                               select key).ToList();

            var JArrayKeys = (from r in result
                              let key = r.Key
                              let value = r.Value
                              where value.GetType() == typeof(JArray)
                              select key).ToList();

            //JArrayKeys.ForEach(key => result[key] = ((JArray)result[key]).Values().Select(x => ((JProperty)x).Value).ToArray());

            foreach (var key in JArrayKeys)
            {
                var lst = new List<object>();
                foreach (var item in (JArray)result[key])
                {
                    if (item is JValue)
                        lst.Add(((JValue)item).Value);
                    else
                        lst.Add(ToDictionary((JObject)item));

                }

                result[key] = lst;

                //result[key] = ((JArray)result[key]).Values().Select(x => ((JProperty)x).Value).ToArray();
            }

            JObjectKeys.ForEach(key => result[key] = ToDictionary(result[key] as JObject));

            return result;
        }
    }

}
