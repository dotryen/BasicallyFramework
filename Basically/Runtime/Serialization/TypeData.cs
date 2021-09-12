using System;

namespace Basically.Serialization {
    /// <summary>
    /// Contains writing/reading and other pieces of data.
    /// It is populated by the weaver.
    /// </summary>
    internal static class TypeData<T> {
        // standard write/read
        public static Action<Writer, T> write;
        public static Action<Writer, T, int> writeBit;
        public static Func<Reader, T> read;
        public static Func<Reader, int, T> readBit;

        public static bool delta;

        // delta write/read (These are different due to requiring different parameters)
        public static Action<Writer, T, T> deltaWrite;
        public static Func<Reader, T, T> deltaRead;
    }
}
