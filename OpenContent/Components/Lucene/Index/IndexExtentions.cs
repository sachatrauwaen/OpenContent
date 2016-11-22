using Satrabel.OpenContent.Components.Lucene.Config;

namespace Satrabel.OpenContent.Components.Lucene.Index
{
    public static class IndexExtentions
    {
        public static bool HasField(this FieldConfig indexConfig, string fieldname)
        {
            return indexConfig != null && indexConfig.Fields != null && indexConfig.Fields.ContainsKey(fieldname);
        }
    }
}