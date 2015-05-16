using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Json;

namespace OpenContentTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

            string expected = "123";
            string dataJson = "{\"lst\":[{\"data\":1},{\"data\":2},{\"data\":3}]}";

            dynamic model = JsonUtils.JsonToDynamic(dataJson);
            string source = "{{#each lst}}{{data}}{{/each}}";
            //dynamic model = new { data = "test"};
            HandlebarsEngine hbEngine = new HandlebarsEngine();
            string res = hbEngine.Execute(source, model);

            Assert.AreEqual(expected, res);

        }
    }
}
