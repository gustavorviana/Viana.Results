#if !NET10_0_OR_GREATER
using System;

namespace Viana.Results.OpenApi.Swashbuckle.Schemas;

[Flags]
public enum JsonSchemaType
{
    Null = 1,
    Boolean = 2,
    Integer = 4,
    Number = 8,
    String = 0x10,
    Object = 0x20,
    Array = 0x40
}
#endif