using System;
using System.Collections;
using System.Collections.Generic;

namespace Basically.Serialization {

    /// <summary>
    /// Automatically serializes an object, removing the need for creating a serializer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class AutoSerializeAttribute : Attribute {

    }
}
