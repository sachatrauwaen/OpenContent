using System;
using Newtonsoft.Json.Linq;

namespace Satrabel.OpenContent.Components.Lucene.Config
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
