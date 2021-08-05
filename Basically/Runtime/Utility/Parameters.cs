using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Basically.Utility {
    public struct Parameters : IEnumerable<KeyValuePair<int, object>> {
        internal int[] keys;
        internal object[] values;
        internal byte index;
        internal bool full;

        public Parameters(byte size) {
            keys = new int[size];
            values = new string[size];
            index = 0;
            full = false;
        }

        public byte Count => index;

        public void Add<T>(string name, T value) where T : struct {
            if (full) throw new Exception("No more room in the parameter.");

            if (keys.Length == index + 1) {
                Array.Resize(ref keys, keys.Length + 1);
                Array.Resize(ref values, values.Length + 1);
            }

            keys[index] = name.GetStableHashCode();
            values[index] = value;
            index++;
            if (index == byte.MaxValue) full = true;
        }

        public IEnumerator<KeyValuePair<int, object>> GetEnumerator() {
            return new ParameterEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public object this[string key] {
            get {
                var hash = key.GetStableHashCode();
                var result = keys.FirstOrDefault(x => x == hash);

                if (result != default) {
                    return values[result];
                } else {
                    return default;
                }
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
