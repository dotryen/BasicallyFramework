using System;

namespace Basically.Networking {
    /// <summary>
    /// Specifies that a message is delta-compressed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public class DeltaEncodedAttribute : Attribute {
        public DeltaEncodedAttribute() {

        }
    }
}
