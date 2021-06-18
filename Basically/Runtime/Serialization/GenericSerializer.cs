// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Runtime.InteropServices;
// using System.Runtime.Serialization.Formatters.Binary;
// using Basically.Utility;
// 
// namespace Basically.Serialization {
//     [Ignore]    
//     public class GenericSerializer : Serializer<object> {
//         Serializer<bool> boolSerial => GetSerializer<bool>();
//         Serializer<string> stringSerial => GetSerializer<string>();
//         Serializer<int> intSerial => GetSerializer<int>();
// 
//         public override byte[] Write(object value) {
//             if (value == null) throw new System.ArgumentNullException();
//             Buffer buffer = new Buffer();
// 
//             var type = value.GetType();
//             buffer.AddBytes(boolSerial.Write(type.IsValueType));
// 
//             if (type.IsValueType) {
//                 int size = Marshal.SizeOf(value);
//                 byte[] arr = new byte[size];
// 
//                 IntPtr ptr = Marshal.AllocHGlobal(size);
//                 Marshal.StructureToPtr(value, ptr, true);
//                 Marshal.Copy(ptr, arr, 0, size);
//                 Marshal.FreeHGlobal(ptr);
// 
//                 buffer.AddBytes(intSerial.Write(size));
//                 buffer.AddBytes(arr);
//                 buffer.AddBytes(stringSerial.Write(type.AssemblyQualifiedName));
//             } else {
//                 BinaryFormatter bf = new BinaryFormatter();
//                 using (MemoryStream ms = new MemoryStream()) {
//                     bf.Serialize(ms, value);
//                     buffer.AddBytes(intSerial.Write((int)ms.Length));
//                     buffer.AddBytes(ms.ToArray());
//                 }
//             }
// 
//             return buffer;
//         }
// 
//         public override object Read(Buffer buffer) {
//             var kind = boolSerial.Read(buffer);
//             var size = intSerial.Read(buffer);
//             var arr = buffer.ReadBytes(size);
// 
//             if (kind) {
//                 IntPtr ptr = Marshal.AllocHGlobal(size);
//                 Marshal.Copy(arr, 0, ptr, size);
// 
//                 var typename = stringSerial.Read(buffer);
//                 var type = Type.GetType(typename, true);
//                 object structure = Marshal.PtrToStructure(ptr, type);
//                 Marshal.FreeHGlobal(ptr);
// 
//                 return structure;
//             } else {
//                 MemoryStream ms = new MemoryStream();
//                 BinaryFormatter bf = new BinaryFormatter();
//                 ms.Write(arr, 0, size);
//                 ms.Seek(0, SeekOrigin.Begin);
// 
//                 return bf.Deserialize(ms);
//             }
//         }
//     }
// }
