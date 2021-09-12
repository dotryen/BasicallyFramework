using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Basically.Utility {
    using Serialization;

    public struct Parameters {
        internal int[] keys;
        internal object[] values;
        internal byte[][] valuesBytes;

        internal byte index;
        internal bool full;

        public int[] Keys => keys;
        public object[] Values => values;
        public byte Count => index;

        public Parameters(byte size) {
            keys = new int[size];
            values = new string[size];
            valuesBytes = new byte[size][];

            index = 0;
            full = false;
        }

        public void Add<T>(string name, T value) where T : struct {
            if (full) throw new Exception("No more room in the parameter.");

            if (keys.Length == index + 1) {
                Array.Resize(ref keys, keys.Length + 1);
                Array.Resize(ref values, values.Length + 1);
                Array.Resize(ref valuesBytes, valuesBytes.Length + 1);
            }

            keys[index] = name.GetStableHashCode();
            values[index] = value;

            // must write when adding
            var writer = Pool<Writer>.Pull();
            writer.Write(value);
            valuesBytes[index] = writer.ToArray();
            Pool<Writer>.Push(writer);

            index++;
            if (index == byte.MaxValue) full = true;
        }

        internal void Add(string name, byte[] value) {
            if (full) throw new Exception("No more room in the parameter.");

            if (keys.Length == index + 1) {
                Array.Resize(ref keys, keys.Length + 1);
                Array.Resize(ref values, values.Length + 1);
                Array.Resize(ref valuesBytes, valuesBytes.Length + 1);
            }

            keys[index] = name.GetStableHashCode();
            values[index] = null;
            valuesBytes[index] = value;

            index++;
            if (index == byte.MaxValue) full = true;
        }

        public T Get<T>(string name) where T : struct {
            var hash = name.GetStableHashCode();
            var index = Array.FindIndex(keys, x => x == hash);

            if (values[index] == null) {
                var reader = Pool<Reader>.Pull();
                valuesBytes[index].CopyTo(reader.ToArray(), 0);

                var value = reader.Read<T>();
                Pool<Reader>.Push(reader);

                return value;
            } else {
                return (T)values[index];
            }
        }
    }

    public class ParameterEnumerator : IEnumerator<KeyValuePair<int, object>> {
        Parameters parameters;
        byte position;

        public ParameterEnumerator(Parameters param) {
            parameters = param;
        }

        public bool MoveNext() {
            position++;
            return position < parameters.Count;
        }

        public void Reset() {
            position = 0;
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public KeyValuePair<int, object> Current {
            get { return new KeyValuePair<int, object>(parameters.keys[position], parameters.values[position]); }
        }

        object IEnumerator.Current => Current;
    }
}
