using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Basically.Serialization {
    // public class DictionarySerializer : Serializer<IDictionary> {
    //     public override byte[] Write(IDictionary value) {
    //         Buffer buffer = new Buffer(4);
    //         buffer.AddBytes(GetSerializer<int>().Write(value.Count));
    // 
    //         if (value.Count == 0) return buffer;
    // 
    //         var keys = ICollectionToArray(value.Keys);
    //         var values = ICollectionToArray(value.Values);
    // 
    //         for (int i = 0; i < value.Count; i++) {
    //             var keySerial = GetSerializer(keys[i].GetType());
    //             var valueSerial = GetSerializer(values[i].GetType());
    //             if (keySerial == null || valueSerial == null) continue;
    // 
    //             buffer.AddBytes(keySerial.WriteInternal(keys[i]));
    //             buffer.AddBytes(keySerial.WriteInternal(values[i]));
    //         }
    // 
    //         return buffer;
    //     }
    // 
    //     public override IDictionary Read(Buffer buffer) {
    //         var count = GetSerializer<int>().Read(buffer);
    //         if (count == 0) return null;
    // 
    //         var dictionary = new Dictionary<object, object>();
    //     }
    // 
    //     object[] ICollectionToArray(ICollection col) {
    //         var arr = new object[col.Count];
    //         col.CopyTo(arr, 0);
    //         return arr;
    //     }
    // }
}
