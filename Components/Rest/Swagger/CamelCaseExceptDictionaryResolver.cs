using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
