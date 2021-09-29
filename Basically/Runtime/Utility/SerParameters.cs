using System;
using System.Collections;
using System.Collections.Generic;

namespace Basically.Utility {
    using Serialization;

    public struct SerParameters : IParameters {
        internal int[] keys;
        internal byte[][] values;

        internal byte index;
        internal bool full;

        public int[] Keys => keys;
        public byte Count => (byte)keys.Length;

        internal SerParameters(byte size) {
            keys = new int[size];
            values = new byte[size][];

            index = 0;
            full = false;
        }

        public void Add<T>(string name, T value) where T : struct {
            if (full) throw new Exception("No more room in the parameter.");

            if (keys.Length == index) {
                Array.Resize(ref keys, keys.Length + 1);
                Array.Resize(ref values, values.Length + 1);
            }

            keys[index] = name.GetStableHashCode();

            var writer = Pool<Writer>.Pull();
            writer.Write(value);
            values[index] = writer.ToArray();
            Pool<Writer>.Push(writer);

            index++;
            if (index == byte.MaxValue) full = true;
        }

        public T Get<T>(string name) where T : struct {
            var hash = name.GetStableHashCode();
            var index = Array.FindIndex(keys, x => x == hash);

            var reader = Pool<Reader>.Pull();
            values[index].CopyTo(reader.ToArray(), 0);

            var value = reader.Read<T>();
            Pool<Reader>.Push(reader);

            return value;
        }
    }
}
