using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Serialization {
    /// <summary>
    /// Non-Generic version of the Serializer, do not inherit. Use Serializer<> instead.
    /// </summary>
    public abstract class Serializer {
        /// <summary>
        /// Index of the serialized type.
        /// </summary>
        public byte Index { get; internal set; }

        /// <summary>
        /// Type this serializer uses.
        /// </summary>
        public virtual Type Type => typeof(object);

        public abstract void WriteInternal(Buffer buffer, object value);
        public abstract object ReadInternal(Buffer buffer);

        public sealed override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public sealed override int GetHashCode() {
            return base.GetHashCode();
        }

        public sealed override string ToString() {
            return base.ToString();
        }
    }

    /// <summary>
    /// Inherit to create a new serializer for a type.
    /// </summary>
    /// <typeparam name="T">Type used in serialization</typeparam>
    public abstract class Serializer<T> : Serializer {
        public sealed override Type Type => typeof(T);

        public sealed override void WriteInternal(Buffer buffer, object value) {
            Write(buffer, (T)value);
        }

        public sealed override object ReadInternal(Buffer payload) {
            return Read(payload);
        }

        /// <summary>
        /// Writes the object to the buffer.
        /// </summary>
        /// <param name="buffer">Buffer that needs to be written to.</param>
        /// <param name="value">Object that will be serialized.</param>
        public abstract void Write(Buffer buffer, T value);

        /// <summary>
        /// Reads the object from the buffer.
        /// </summary>
        /// <param name="buffer">Buffer that will be read from.</param>
        /// <returns>Object that was received.</returns>
        public abstract T Read(Buffer buffer);

        #region Get Serializer

        protected Serializer<Type> GetSerializer<Type>() {
            return BasicallyCache.GetSerializer<Type>();
        }

        protected Serializer GetSerializer(byte index) {
            return BasicallyCache.GetSerializer(index);
        }

        protected Serializer GetSerializer(Type type) {
            return BasicallyCache.GetSerializer(type);
        }

        #endregion

        // #region Serialize Object
        // 
        // protected byte[] SerializeObject<Type>(Type value) {
        //     return SerializeObject(typeof(Type), value);
        // }
        // 
        // protected byte[] SerializeObject(Type type, object value) {
        //     if (type == typeof(T)) throw new ArgumentException("Type cannot be the same type as the serializer.");
        //     Serializer serial = BasicallyCache.GetSerializer(type);
        // 
        //     if (serial == null) throw new Exception("Type doesn't have a serialier");
        //     return serial.WriteInternal(value);
        // }
        // 
        // #endregion

        #region Deserialize Object

        protected Type DeserializeObject<Type>(Buffer buffer) {
            return (Type)DeserializeObject(typeof(Type), buffer);
        }

        protected object DeserializeObject(Type type, Buffer buffer) {
            if (type == typeof(T)) throw new ArgumentException("Type cannot be the same type as the serializer.");
            Serializer serial = BasicallyCache.GetSerializer(type);

            if (serial == null) throw new Exception("Type doesn't have a serialier.");
            return serial.ReadInternal(buffer);
        }

        protected object DeserializeObject(byte index, Buffer buffer) {
            Serializer serial = BasicallyCache.GetSerializer(index);
            if (serial == null) throw new Exception("Index doesn't have a serializer.");
            if (serial.Type == Type) throw new Exception("Type cannot be the same type as the serializer.");

            return serial.ReadInternal(buffer);
        }

        #endregion
    }
}
