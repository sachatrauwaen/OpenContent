using System;

namespace Satrabel.OpenContent.Components.Rest.Swagger
{
    [Flags]
    public enum SchemaType
    {
        None = 0,
        String = 1,
        Number = 2,
        Integer = 4,
        Boolean = 8,
        Object = 16,
        Array = 32,
        Null = 64,
        File = 128
    }
}