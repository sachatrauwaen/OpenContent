using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Lucene.Config;
using Satrabel.OpenContent.Components.Querying;

namespace OpenContentTests
{
    [TestClass]
    public class QueryBuilderSelectTests
    {
        [TestMethod]
        public void Build_ParsesSelectFieldsFromQuery()
        {
            var query = JObject.Parse(@"{
                ""MaxResults"": 50,
                ""Select"": [ ""Title"", ""Summary"", ""LinkUrl"" ]
            }");
            var builder = new QueryBuilder(new FieldConfig());
            builder.Build(query, false, -1, "en-US", null);

            Assert.AreEqual(50, builder.Select.PageSize);
            Assert.IsTrue(builder.Select.HasFieldProjection);
            CollectionAssert.AreEqual(new[] { "Title", "Summary", "LinkUrl" }, builder.Select.Fields);
        }

        [TestMethod]
        public void Build_WithoutSelect_HasNoFieldProjection()
        {
            var query = JObject.Parse(@"{ ""MaxResults"": 10 }");
            var builder = new QueryBuilder(new FieldConfig());
            builder.Build(query, false, -1, "en-US", null);

            Assert.IsFalse(builder.Select.HasFieldProjection);
            Assert.AreEqual(0, builder.Select.Fields.Count);
        }
    }
}
