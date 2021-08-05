using System;

namespace Basically.Networking {
    [AttributeUsage(AttributeTargets.Class)]
    public class ReceiverClassAttribute : Attribute {
        public ReceiverClassAttribute() {

        }
    }
}
