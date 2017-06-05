namespace Satrabel.OpenContent.Components.Lucene.Config
{
    public static class IndexExtentions
    {
        public static bool HasField(this FieldConfig indexConfig, string fieldname)
        {
            return indexConfig?.Fields != null && indexConfig.Fields.ContainsKey(fieldname);
        }
    }
}