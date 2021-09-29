using System;
using Basically.Serialization.Bit;
using Basically.Serialization.Bit.Utilities;
using UnityEngine;

namespace Basically.Serialization {
    using Utility;

    public class Writer {
        readonly byte[] arr;
        int pos;

        internal Writer(int size) {
            arr = new byte[size];
            pos = 0;
        }

        internal void WriteInternal(ByteConverter value, int bits) {
            arr.Write(value, ref pos, bits);
        }

        public void Write<T>(T value) {
            Action<Writer, T> act = TypeData<T>.write;
            if (act == null) {
                Debug.LogError($"No writer for {typeof(T)}");
            } else {
                act(this, value);
            }
        }

        public void Write<T>(T value, int bits) {
            Action<Writer, T, int> act = TypeData<T>.writeBit;
            if (act == null) {
                Debug.LogError($"No bit writer for {typeof(T)}");
            } else {
                act(this, value, bits);
            }
        }

        public void Reset() {
            for (int i = 0; i < arr.Length; i++) {
                arr[i] = 0;
            }
            pos = 0;
        }

        public byte[] ToArray() {
            int length = pos / 8;
            if (pos % 8 != 0) length++;

            byte[] array = new byte[length];
            Array.Copy(arr, 0, array, 0, length);

            return array;
        }
    }

    public static class WriterExtensions {
        // sorted by size (almost)

        public static void WriteBool(this Writer writer, bool value) {
            writer.WriteInternal(value, 1);
        }
        public static void WriteSByte(this Writer writer, sbyte value, int bits = 8) {
            writer.WriteInternal(value.ZigZag(), bits);
        }
        public static void WriteByte(this Writer writer, byte value, int bits = 8) {
            writer.WriteInternal(value, bits);
        }
        public static void WriteChar(this Writer writer, char value) {
            writer.WriteInternal(value, 16);
        }
        public static void WriteShort(this Writer writer, short value, int bits = 16) {
            writer.WriteInternal(value.ZigZag(), bits);
        }
        public static void WriteUShort(this Writer writer, ushort value, int bits = 16) {
            writer.WriteInternal(value, bits);
        }
        public static void WriteInt(this Writer writer, int value, int bits = 32) {
            writer.WriteInternal(value.ZigZag(), bits);
        }
        public static void WriteUInt(this Writer writer, uint value, int bits = 32) {
            writer.WriteInternal(value, bits);
        }
        public static void WriteFloat(this Writer writer, float value) {
            writer.WriteInternal(value, 32);
        }
        public static void WriteLong(this Writer writer, long value, int bits = 64) {
            writer.WriteInternal(value.ZigZag(), bits);
        }
        public static void WriteULong(this Writer writer, ulong value, int bits = 64) {
            writer.WriteInternal(value, bits);
        }
        public static void WriteDouble(this Writer writer, double value) {
            writer.WriteInternal(value, 64);
        }
        public static void WriteVector2(this Writer writer, Vector2 value) {
            WriteFloat(writer, value.x);
            WriteFloat(writer, value.y);
        }
        public static void WriteVector3(this Writer writer, Vector3 value) {
            WriteFloat(writer, value.x);
            WriteFloat(writer, value.y);
            WriteFloat(writer, value.z);
        }
        public static void WriteQuaternion(this Writer writer, Quaternion value) {
            QuatCompress.Compress(value, out byte index, out ushort a, out ushort b, out ushort c);
            WriteByte(writer, index, 2);
            WriteUShort(writer, a, 9);
            WriteUShort(writer, b, 9);
            WriteUShort(writer, c, 9);
        }
        public static void WriteArray<T>(this Writer writer, T[] value) {
            if (value == null) {
                writer.WriteBool(false);
            } else {
                writer.WriteBool(true);
                writer.WriteInt(value.Length);
                for (int i = 0; i < value.Length; i++) {
                    writer.Write(value[i]);
                }
            }
        }
        public static void WriteString(this Writer writer, string value) {
            writer.WriteArray(System.Text.Encoding.UTF8.GetBytes(value));
        }
        public static void WriteParameters(this Writer writer, IParameters parameters) {
            var casted = (SerParameters)parameters; // Parameters should never be transferred, only SerParameters

            writer.WriteByte(casted.index);
            for (int i = 0; i < casted.index; i++) {
                writer.WriteInt(casted.keys[i]);
                writer.WriteArray(casted.values[i]);
            }
        }
    }
}
