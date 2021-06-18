using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Entities;
using Basically.Utility;

// namespace Basically.Serialization {
//     public class EntityStateSerializer : Serializer<EntityState> {
//         public override byte[] Write(EntityState value) {
//             Buffer buffer = new Buffer((sizeof(float) * 7) + 1);
//             buffer.AddBytes(SerializeObject(value.position));
//             buffer.AddBytes(SerializeObject(value.rotation));
//             buffer.AddBytes(SerializeObject(value.parameters));
// 
//             return buffer;
//         }
// 
//         public override EntityState Read(Buffer buffer) {
//             EntityState state = new EntityState() {
//                 position = DeserializeObject<Vector3>(buffer),
//                 rotation = DeserializeObject<Quaternion>(buffer),
//                 parameters = DeserializeObject<Parameters>(buffer)
//             };
// 
//             return state;
//         }
//     }
// }
