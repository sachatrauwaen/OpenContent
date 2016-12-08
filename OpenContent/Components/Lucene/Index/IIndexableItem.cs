using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Lucene.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
