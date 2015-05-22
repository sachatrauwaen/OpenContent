using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Satrabel.OpenContent.Components.Handlebars;
using Satrabel.OpenContent.Components.Json;

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
            dynamic model = new { data = 10};
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
            dynamic model1 = new { data = "10"};
            dynamic model2 = new { data = "5" };
            HandlebarsEngine hbEngine = new HandlebarsEngine();
            string res1 = hbEngine.Execute(source, model1);
            string res2 = hbEngine.Execute(source, model2);
            Assert.AreEqual(expected1, res1);
            Assert.AreEqual(expected2, res2);
        }
    }
}
