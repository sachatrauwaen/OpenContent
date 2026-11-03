using System;
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
        public void JsonToDynamicForLargeLists_SupportsIndexerAssignment_LikeOcUtilsRegisterDynamic()
        {
            dynamic settings = JsonUtils.JsonToDynamicForLargeLists(JObject.Parse("{\"ListItemTemplate\":\"x.cshtml\"}"));
            Assert.IsNull(settings.GridSettings);

            // HolonCom.OcUtils.RegisterDynamic uses source[name] = new ExpandoObject()
            settings["GridSettings"] = new System.Dynamic.ExpandoObject();
            settings.IsMobile = true;

            Assert.IsNotNull(settings.GridSettings);
            Assert.IsTrue((bool)settings.IsMobile);
        }

        [TestMethod]
        public void JsonToDynamicForLargeLists_ArrayMissingMembersReturnNull_LikeDynamicJsonArray()
        {
            // Templates do dataitem.Attachments.RelatedUrls even when Attachments is [].
            // DynamicJsonArray returns null; raw object[] throws RuntimeBinderException.
            dynamic model = JsonUtils.JsonToDynamicForLargeLists(JObject.Parse("{\"Attachments\":[]}"));
            Assert.AreEqual(0, (int)model.Attachments.Length);
            Assert.IsNull(model.Attachments.RelatedUrls);

            dynamic withUrls = JsonUtils.JsonToDynamicForLargeLists(
                JObject.Parse("{\"Attachments\":{\"RelatedUrls\":[{\"Url\":\"http://x\"}]}}"));
            Assert.AreEqual(1, (int)withUrls.Attachments.RelatedUrls.Length);
            Assert.AreEqual("http://x", (string)withUrls.Attachments.RelatedUrls[0].Url);
        }

        [TestMethod]
        public void JsonToDynamicForLargeLists_ItemsLengthWorksForRazorCondition()
        {
            dynamic empty = JsonUtils.JsonToDynamicForLargeLists(JObject.Parse("{\"Items\":[]}"));
            Assert.IsFalse(empty.Items != null && empty.Items.Length > 0);

            dynamic withItems = JsonUtils.JsonToDynamicForLargeLists(
                JObject.Parse("{\"Items\":[{\"Title\":\"A\"},{\"Title\":\"B\"}]}"));
            Assert.IsTrue(withItems.Items != null && withItems.Items.Length > 0);
            Assert.AreEqual(2, (int)withItems.Items.Length);

            var titles = new List<string>();
            foreach (var item in withItems.Items)
            {
                titles.Add((string)item.Title);
            }
            CollectionAssert.AreEqual(new[] { "A", "B" }, titles);
        }

        [TestMethod]
        [ExpectedException(typeof(System.Exception))]
        public void JsonToDictionary_LegacyPath_ThrowsOnPayloadLargerThanMaxJsonLength()
        {
            var json = "{\"Data\":\"" + new string('x', 2100000) + "\"}";
            JsonUtils.JsonToDictionary(json);
        }

        [TestMethod]
        public void JsonToDictionaryForLargeLists_ThrowsWhenExceedingMaxLargeListJsonLength()
        {
            var previous = JsonUtils.MaxLargeListJsonLength;
            try
            {
                JsonUtils.MaxLargeListJsonLength = 100;
                var token = JObject.Parse("{\"Data\":\"" + new string('x', 200) + "\"}");
                try
                {
                    JsonUtils.JsonToDictionaryForLargeLists(token);
                    Assert.Fail("Expected Exception for oversized large-list payload");
                }
                catch (Exception ex)
                {
                    StringAssert.Contains(ex.Message, "supportLargeLists");
                    StringAssert.Contains(ex.Message, "max=100");
                }
            }
            finally
            {
                JsonUtils.MaxLargeListJsonLength = previous;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.Exception))]
        public void JsonToDictionary_SupportLargeLists_ThrowsWhenStringExceedsMaxLargeListJsonLength()
        {
            var previous = JsonUtils.MaxLargeListJsonLength;
            try
            {
                JsonUtils.MaxLargeListJsonLength = 50;
                JsonUtils.JsonToDictionary("{\"Data\":\"" + new string('x', 80) + "\"}", supportLargeLists: true);
            }
            finally
            {
                JsonUtils.MaxLargeListJsonLength = previous;
            }
        }

        [TestMethod]
        public void GetQuerySelectFields_ReadsSelectArrayFromFilterQuery()
        {
            var query = JObject.Parse("{\"MaxResults\":100,\"Select\":[\"Title\",\"Summary\",\"Title\"]}");
            var fields = JsonUtils.GetQuerySelectFields(query);
            Assert.IsNotNull(fields);
            Assert.AreEqual(2, fields.Count);
            Assert.AreEqual("Title", fields[0]);
            Assert.AreEqual("Summary", fields[1]);
        }

        [TestMethod]
        public void GetQuerySelectFields_ReturnsNullWhenSelectMissingOrEmpty()
        {
            Assert.IsNull(JsonUtils.GetQuerySelectFields(JObject.Parse("{\"MaxResults\":10}")));
            Assert.IsNull(JsonUtils.GetQuerySelectFields(JObject.Parse("{\"Select\":[]}")));
            Assert.IsNull(JsonUtils.GetQuerySelectFields(null));
        }

        [TestMethod]
        public void ProjectFields_KeepsOnlySelectedRootProperties_AndPreservesContext()
        {
            var source = JObject.Parse("{\"Title\":\"Cook\",\"Summary\":\"Bio\",\"Attachments\":[{\"Text\":\"x\"}],\"Context\":{\"Id\":\"1\"}}");
            var projected = JsonUtils.ProjectFields(source, new List<string> { "Title", "Summary" });

            Assert.AreEqual("Cook", projected["Title"].ToString());
            Assert.AreEqual("Bio", projected["Summary"].ToString());
            Assert.IsNull(projected["Attachments"]);
            Assert.IsNotNull(projected["Context"]);
            Assert.AreEqual("1", projected["Context"]["Id"].ToString());
            // Source unchanged
            Assert.IsNotNull(source["Attachments"]);
        }

        [TestMethod]
        public void ProjectFields_WithoutFields_ReturnsSameInstance()
        {
            var source = JObject.Parse("{\"Title\":\"Cook\"}");
            Assert.AreSame(source, JsonUtils.ProjectFields(source, null));
            Assert.AreSame(source, JsonUtils.ProjectFields(source, new List<string>()));
        }
    }
}
