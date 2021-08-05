using System;

namespace Basically.Serialization {
    [AttributeUsage(AttributeTargets.Field)]
    public class BitSizeAttribute : Attribute {
        public int size;

        public BitSizeAttribute(int size) {
            this.size = size;
        }

        private BitSizeAttribute() {

        }
    }
}
