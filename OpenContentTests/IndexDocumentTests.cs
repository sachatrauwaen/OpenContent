using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components;

namespace OpenContentTests
{
    [TestClass]
    public class IndexDocumentTests
    {
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
    }
}
