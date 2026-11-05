using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components;
using Satrabel.OpenContent.Components.Lucene.Config;
using Satrabel.OpenContent.Components.Lucene.Mapping;

namespace OpenContentTests
{
    [TestClass]
    public class IndexDocumentTests
    {
        [TestMethod]
        public void JsonToDocument_IncludesSortableTitleField()
        {
            // Title sort uses "@Title". Docs without it sort last in Title-asc lists
            // until a full reindex rebuilds the Lucene document.
            var indexConfig = new FieldConfig(true);
            indexConfig.Fields.Add("Title", new FieldConfig
            {
                Index = true,
                Sort = true,
                IndexType = "text"
            });

            var json = JObject.Parse("{\"Title\":\"James Trevenen – An Update\",\"Summary\":\"Info\"}");
            var doc = JsonMappingUtils.JsonToDocument(
                "123",
                "999",
                "1",
                DateTime.UtcNow,
                json,
                json.ToString(),
                indexConfig);

            Assert.IsNotNull(doc.GetField("Title"), "analyzed Title field missing");
            Assert.IsNotNull(doc.GetField("@Title"), "sortable @Title field missing");
            StringAssert.StartsWith(doc.GetField("@Title").StringValue, "james trevenen");
        }

        [TestMethod]
        public void JsonAsJToken_WithoutSetter_DiscardsInPlaceMutations()
        {
            // Getter returns DeepClone: in-place edits (old HydrateDefaultFields path) never stick.
            var content = new OpenContentInfo("{\"Title\":\"James Trevenen – An Update\"}");
            var clone = content.JsonAsJToken as JObject;
            Assert.IsNotNull(clone);
            clone["publishstatus"] = "published";

            var reread = content.JsonAsJToken as JObject;
            Assert.IsNull(reread["publishstatus"], "DeepClone getter must discard in-place mutations");
        }

        [TestMethod]
        public void JsonAsJToken_AssignBack_PersistsPublishDefaultsOntoJson()
        {
            // HydrateDefaultFields must assign the mutated clone via the setter (this pattern).
            var content = new OpenContentInfo("{\"Title\":\"James Trevenen – An Update\"}");
            var json = content.JsonAsJToken as JObject;
            Assert.IsNotNull(json);
            json["publishstartdate"] = DateTime.MinValue;
            json["publishenddate"] = DateTime.MaxValue;
            json["publishstatus"] = "published";
            content.JsonAsJToken = json;

            var reread = content.JsonAsJToken as JObject;
            Assert.IsNotNull(reread);
            Assert.IsNotNull(reread["publishstartdate"], "publishstartdate was not persisted");
            Assert.IsNotNull(reread["publishenddate"], "publishenddate was not persisted");
            Assert.AreEqual("published", (string)reread["publishstatus"]);
            Assert.AreEqual("James Trevenen – An Update", (string)reread["Title"]);
        }

        [TestMethod]
        public void JsonToDocument_WithPersistedPublishDefaults_IncludesPublishStatusKey()
        {
            var content = new OpenContentInfo("{\"Title\":\"James Trevenen – An Update\"}");
            var json = content.JsonAsJToken as JObject;
            json["publishstatus"] = "published";
            content.JsonAsJToken = json;

            var indexConfig = new FieldConfig(true);
            indexConfig.Fields.Add("Title", new FieldConfig { Index = true, Sort = true, IndexType = "text" });
            indexConfig.Fields.Add("publishstatus", new FieldConfig { Index = true, Sort = true, IndexType = "key" });

            var doc = JsonMappingUtils.JsonToDocument(
                "123",
                "999",
                "1",
                DateTime.UtcNow,
                content.GetData(),
                content.GetSource(),
                indexConfig);

            Assert.IsNotNull(doc.GetField("@Title"));
            Assert.IsNotNull(doc.GetField("publishstatus"), "publishstatus missing from Lucene doc after assign-back");
            Assert.AreEqual("published", doc.GetField("publishstatus").StringValue);
        }
    }
}
