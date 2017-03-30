using Newtonsoft.Json.Linq;
using System;

namespace Satrabel.OpenContent.Components.Lucene.Index
{
    public interface IIndexableItem
    {
        string GetId();
        string GetScope();
        string GetCreatedByUserId();
        DateTime GetCreatedOnDate();
        JToken GetData();
        string GetSource();
    }
}
