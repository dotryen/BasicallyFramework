using System;
using System.Collections;
using System.Collections.Generic;

namespace Basically.Utility {
    /// <summary>
    /// Used to ignore objects, can be used for multiple purposes.
    /// WARNING: IGNORED OBJECTS SHOULD NOT BE USED IN NETWORKING
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute {

    }
}
