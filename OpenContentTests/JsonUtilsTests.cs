using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Json;

namespace OpenContentTests
{
    [TestClass]
    public class JsonUtilsTests
    {
        [TestMethod]
        public void JsonToDictionary_DefaultPath_UsesLegacyJavaScriptSerializer()
        {
            // Default (supportLargeLists omitted/false) → legacy serializer.
            var model = JsonUtils.JsonToDictionary("{\"Title\":\"Hello\",\"Count\":2}");
            Assert.AreEqual("Hello", model["Title"]);
            Assert.AreEqual(2, model["Count"]);
        }

        [TestMethod]
        public void JsonToDynamic_DefaultPath_UsesLegacyJsonDecode_CaseInsensitive()
        {
            dynamic model = JsonUtils.JsonToDynamic("{\"Lst\":[{\"Data\":1},{\"Data\":2}]}");
            var handlebars = new HandlebarsEngine();
            var result = handlebars.Execute("{{#each lst}}{{data}}{{/each}}", model);
            Assert.AreEqual("12", result);
        }

        [TestMethod]
        public void JsonToDynamic_WithSupportLargeLists_UsesLargeListPath()
        {
            dynamic model = JsonUtils.JsonToDynamic("{\"Title\":\"Cook\"}", supportLargeLists: true);
            Assert.AreEqual("Cook", (string)model.Title);
        }

        [TestMethod]
        public void JsonToDictionaryForLargeLists_FromJToken_DoesNotRoundTripThroughString()
        {
            var token = JObject.Parse("{\"Title\":\"Hello\",\"Items\":[{\"Id\":1},{\"Id\":2}]}");
            var model = JsonUtils.JsonToDictionaryForLargeLists(token);

            Assert.AreEqual("Hello", model["Title"]);
            var items = model["Items"] as object[];
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual(1, ((Dictionary<string, object>)items[0])["Id"]);
        }

        [TestMethod]
        public void JsonToDynamicForLargeLists_IsCaseInsensitive()
        {
            dynamic model = JsonUtils.JsonToDynamicForLargeLists(JObject.Parse("{\"Lst\":[{\"Data\":1},{\"Data\":2}]}"));
            var handlebars = new HandlebarsEngine();
            var result = handlebars.Execute("{{#each lst}}{{data}}{{/each}}", model);
            Assert.AreEqual("12", result);
        }

        [TestMethod]
        public void JsonToDictionaryForLargeLists_HandlesPayloadLargerThanJavaScriptSerializerDefault()
        {
            // JavaScriptSerializer default MaxJsonLength is 2_097_152 characters.
            var itemJson = "{\"Title\":\"Person\",\"Summary\":\"" + new string('x', 5000) + "\",\"Attachments\":[{\"Text\":\"" + new string('y', 2000) + "\"}]}";
            var builder = new StringBuilder("{\"Items\":[");
            const int itemCount = 500;
            for (var i = 0; i < itemCount; i++)
            {
                if (i > 0)
                {
                    builder.Append(',');
                }
                builder.Append(itemJson);
            }
            builder.Append("]}");
            var json = builder.ToString();
            Assert.IsTrue(json.Length > 2097152, "Test payload must exceed legacy maxJsonLength");

            var token = JObject.Parse(json);
            var dictionary = JsonUtils.JsonToDictionaryForLargeLists(token);
            dynamic dynamicModel = JsonUtils.JsonToDynamicForLargeLists(token);

            var dictItems = dictionary["Items"] as object[];
            Assert.IsNotNull(dictItems);
            Assert.AreEqual(itemCount, dictItems.Length);

            var handlebars = new HandlebarsEngine();
            var rendered = handlebars.Execute("{{#each Items}}{{Title}}{{/each}}", dynamicModel);
            Assert.AreEqual(string.Concat(Enumerable.Repeat("Person", itemCount)), rendered);
        }

        [TestMethod]
        public void JsonToDynamicForLargeLists_SupportsRazorStylePropertyAccess()
        {
            var token = JObject.Parse("{\"Settings\":{\"ListItemTemplate\":\"_ListItemQuickList.cshtml\"},\"Items\":[{\"Title\":\"Cook\"}]}");
            dynamic model = JsonUtils.JsonToDynamicForLargeLists(token);

            Assert.AreEqual("_ListItemQuickList.cshtml", (string)model.Settings.ListItemTemplate);
            Assert.AreEqual("Cook", (string)model.Items[0].Title);
        }

        [TestMethod]
        [ExpectedException(typeof(System.Exception))]
        public void JsonToDictionary_LegacyPath_ThrowsOnPayloadLargerThanMaxJsonLength()
        {
            var json = "{\"Data\":\"" + new string('x', 2100000) + "\"}";
            JsonUtils.JsonToDictionary(json);
        }
    }
}
