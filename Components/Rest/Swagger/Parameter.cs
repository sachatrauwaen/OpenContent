namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    public class Parameter : ItemsObject
    {
        public Parameter (){
            In  = Location.Query;
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public Location In { get; set; }
        public bool Required { get; set; }

        #region In=Body

        public SchemaObject Schema { get; set; }

        #endregion

        #region Access: Public

        public void Map(ItemsObject item)
        {
            Default = item.Default;
            Maximum = item.Maximum;
            ExclusiveMaximum = item.ExclusiveMaximum;
            Minimum = item.Minimum;
            ExclusiveMinimum = item.ExclusiveMinimum;
            MaxLength = item.MaxLength;
            MinLength = item.MinLength;
            Pattern = item.Pattern;
            MaxItems = item.MaxItems;
            MinItems = item.MinItems;
            UniqueItems = item.UniqueItems;
            Enum = item.Enum;
            Type = item.Type;
            Items = item.Items;
        }

        #endregion
    }
}