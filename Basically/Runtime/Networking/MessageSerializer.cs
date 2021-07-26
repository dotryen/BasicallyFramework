using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Basically.Networking {
    using Serialization;

    public static class MessagePacker {
        const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.Instance;

        // TODO: Change the packer to use pre-built methods for serializing and deserializing messages and auto types.

        #region Serialize

        public static byte[] SerializeMessage<T>(T message) where T : NetworkMessage {
            Buffer buffer = BufferPool.Get();
            Serializer serializer = null; // pretty much a buffer, it could be faster and easier

            buffer.Write(BasicallyCache.GetMessageIndex(typeof(T)), BasicallyCache.MessageBits);
            foreach (var field in typeof(T).GetFields(BINDING_FLAGS)) {
                SerializeField(buffer, ref serializer, field.GetValue(message));
            }

            var result = buffer.ToArray();
            BufferPool.Return(buffer);
            return result;
        }

        private static void SerializeField(Buffer buffer, ref Serializer serializerRef, object value) {
            var type = value.GetType();

            // check if array (to do special shit)
            if (type.IsArray) {
                var elementType = type.GetElementType();
                var array = (Array)value;

                // write length (for obvious reasons)
                buffer.Write(array.Length);

                // check if auto (optimizations are applied)
                if (IsTypeAuto(elementType)) {
                    var fields = elementType.GetFields(BINDING_FLAGS);
                    for (int i = 0; i < array.Length; i++) {
                        for (int h = 0; h < fields.Length; h++) {
                            SerializeField(buffer, ref serializerRef, fields[h].GetValue(array.GetValue(i)));
                        }
                    }
                } else {
                    for (int i = 0; i < array.Length; i++) {
                        SerializeObject(buffer, ref serializerRef, array.GetValue(i), elementType);
                    }
                }
            } else {
                // Not array
                if (IsTypeAuto(type)) {
                    foreach (var field in type.GetFields(BINDING_FLAGS)) {
                        SerializeField(buffer, ref serializerRef, field.GetValue(value));
                    }
                } else {
                    SerializeObject(buffer, ref serializerRef, value, type);
                }
            }
        }

        private static void SerializeObject(Buffer buffer, ref Serializer serializerRef, object value, Type type) {
            var code = Type.GetTypeCode(type);

            switch (code) {
                default: {
                        if (serializerRef == null) {
                            serializerRef = BasicallyCache.GetSerializer(type);
                        } else if (serializerRef.Type != type) {
                            serializerRef = BasicallyCache.GetSerializer(type);
                        }
                        if (serializerRef == null) break; // there is no serializer for this

                        if (!type.IsValueType) {
                            bool isNull = value == null;
                            buffer.Write(isNull);
                            if (isNull) {
                                break;
                            }
                        }

                        serializerRef.WriteInternal(buffer, value);
                        break;
                    }

                //the dirty work has to be done one way or another
                case System.TypeCode.Boolean: {
                        buffer.Write((bool)value);
                        break;
                    }

                case System.TypeCode.Byte: {
                        buffer.Write((byte)value);
                        break;
                    }

                case System.TypeCode.SByte: {
                        buffer.Write((sbyte)value);
                        break;
                    }

                case System.TypeCode.Int16: {
                        buffer.Write((short)value);
                        break;
                    }

                case System.TypeCode.UInt16: {
                        buffer.Write((ushort)value);
                        break;
                    }

                case System.TypeCode.Int32: {
                        buffer.Write((int)value);
                        break;
                    }

                case System.TypeCode.UInt32: {
                        buffer.Write((uint)value);
                        break;
                    }

                case System.TypeCode.Int64: {
                        buffer.Write((long)value);
                        break;
                    }

                case System.TypeCode.UInt64: {
                        buffer.Write((ulong)value);
                        break;
                    }

                case System.TypeCode.Single: {
                        buffer.Write((float)value);
                        break;
                    }

                case System.TypeCode.Double: {
                        buffer.Write((double)value);
                        break;
                    }

                case System.TypeCode.Char: {
                        buffer.Write((char)value);
                        break;
                    }
            }
        }

        #endregion

        #region Deserialize

        public static NetworkMessage DeserializeMessage(Buffer buffer, out byte index) {
            index = buffer.ReadByte(BasicallyCache.MessageBits);
            Type messageType = BasicallyCache.GetMessage(index);
            Serializer cache = null;

            NetworkMessage message = (NetworkMessage)Activator.CreateInstance(messageType);
            foreach (var field in messageType.GetFields(BINDING_FLAGS)) {
                field.SetValue(message, DeserializeField(buffer, ref cache, field.FieldType));
            }

            return message;
        }

        private static object DeserializeField(Buffer buffer, ref Serializer serializerRef, Type fieldType) {
            if (fieldType.IsArray) {
                // prepare some stuff
                var count = buffer.ReadInt();
                var elementType = fieldType.GetElementType();
                Array array = Array.CreateInstance(elementType, count);

                if (IsTypeAuto(elementType)) {
                    var fields = elementType.GetFields(BINDING_FLAGS);

                    for (int i = 0; i < count; i++) {
                        var newObj = Activator.CreateInstance(elementType);

                        foreach (var field in fields) {
                            // im a little skeptical of this
                            field.SetValue(newObj, DeserializeField(buffer, ref serializerRef, field.FieldType));
                        }

                        array.SetValue(newObj, i);
                    }
                    return array;
                } else {
                    for (int i = 0; i < count; i++) {
                        array.SetValue(DeserializeObject(buffer, ref serializerRef, elementType), i);
                    }
                    return array;
                }
            } else {
                if (IsTypeAuto(fieldType)) {
                    object obj = Activator.CreateInstance(fieldType);

                    foreach (var field in fieldType.GetFields(BINDING_FLAGS)) {
                        field.SetValue(obj, DeserializeField(buffer, ref serializerRef, field.FieldType));
                    }
                    return obj;
                } else {
                    return DeserializeObject(buffer, ref serializerRef, fieldType);
                }
            }
        }

        private static object DeserializeObject(Buffer buffer, ref Serializer serializerRef, Type type) {
            var code = Type.GetTypeCode(type);

            switch (code) {
                default: {
                        if (serializerRef == null) {
                            serializerRef = BasicallyCache.GetSerializer(type);
                        } else if (serializerRef.Type != type) {
                            serializerRef = BasicallyCache.GetSerializer(type);
                        }
                        if (serializerRef == null) return null; // there is no serializer for this

                        if (!type.IsValueType) {
                            bool isNull = buffer.ReadBool();
                            if (isNull) {
                                return null;
                            }
                        }

                        return serializerRef.ReadInternal(buffer);
                    }

                //the dirty work has to be done one way or another
                case System.TypeCode.Boolean: {
                        return buffer.ReadBool();
                    }

                case System.TypeCode.Byte: {
                        return buffer.ReadByte();
                    }

                case System.TypeCode.SByte: {
                        return buffer.ReadSByte();
                    }

                case System.TypeCode.Int16: {
                        return buffer.ReadShort();
                    }

                case System.TypeCode.UInt16: {
                        return buffer.ReadUShort();
                    }

                case System.TypeCode.Int32: {
                        return buffer.ReadInt();
                    }

                case System.TypeCode.UInt32: {
                        return buffer.ReadUInt();
                    }

                case System.TypeCode.Int64: {
                        return buffer.ReadLong();
                    }

                case System.TypeCode.UInt64: {
                        return buffer.ReadULong();
                    }

                case System.TypeCode.Single: {
                        return buffer.ReadFloat();
                    }

                case System.TypeCode.Double: {
                        return buffer.ReadDouble();
                    }

                case System.TypeCode.Char: {
                        return buffer.ReadChar();
                    }
            }
        }

        #endregion

        private static bool IsTypeAuto(Type type) {
            return type.IsDefined(typeof(AutoSerializeAttribute), false);
        }
    }

    public class DeltaKey {
        readonly ushort key;
        public ushort Key => key;

        internal DeltaKey(ushort key) {
            this.key = key;
        }
    }
}
