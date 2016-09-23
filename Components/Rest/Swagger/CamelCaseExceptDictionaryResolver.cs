using Newtonsoft.Json.Serialization;

namespace Satrabel.OpenContent.Components.Rest.Swagger
{

    internal class CamelCaseExceptDictionaryResolver : CamelCasePropertyNamesContractResolver
    {
        #region Overrides of DefaultContractResolver

        //protected override string ResolveDictionaryKey(string dictionaryKey)
        //{
        //    return dictionaryKey;
        //}

        #endregion
    }
}
