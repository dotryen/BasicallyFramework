using System;
using System.Collections;
using System.Collections.Generic;

namespace Basically.Utility {
    public class Parameters : IEnumerable<KeyValuePair<string, object>> {
        internal Dictionary<string, object> dictionary;

        public Parameters() {
            dictionary = new Dictionary<string, object>();
        }

        public Parameters(byte size) {
            dictionary = new Dictionary<string, object>(size);
        }

        public byte Count => (byte)dictionary.Count;

        public void Add(string name, object value) {
            if (dictionary.Count == byte.MaxValue) throw new Exception("No more room in the parameter.");
            dictionary.Add(name, value);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public object this[string key] {
            get {
                if (dictionary.TryGetValue(key, out var obj)) {
                    return obj;
                } else {
                    return null;
                }
            }

            set {
                dictionary[key] = value;
            }
        }
    }
}
