using System;

namespace Basically.Networking {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NetVariableAttribute : Attribute {
        internal Sync kind;

        public NetVariableAttribute() {
            kind = Sync.Snap;
        }
    }
}
