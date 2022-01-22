using System;

namespace Basically.Serialization {
    /// <summary>
    /// Specifies the size of a field in bits.
    /// </summary>
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
