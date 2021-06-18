using System;

namespace Basically.Serialization {
    [AttributeUsage(AttributeTargets.Field)]
    public class BitSizeAttribute : Attribute {
        int size;

        public BitSizeAttribute(int size) {
            this.size = size;
        }

        private BitSizeAttribute() {

        }
    }
}
