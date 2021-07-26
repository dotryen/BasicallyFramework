using System;
using emotitron.Compression;
using emotitron.Compression.Utilities;
using UnityEngine;

// god mirror does a lot of clever things
namespace Basically.Serialization {
    using Networking;
    using Utility;

    /// <summary>
    /// Writing shit.
    /// </summary>
    /// <typeparam name="T">Message type.</typeparam>
    internal static class Writer<T> {
        public static Action<Writer, T> write;
    }

    public class Writer {
        readonly byte[] arr;
        int pos;

        internal Writer(int size) {
            arr = new byte[size];
            pos = 0;
        }

        internal void Write(ByteConverter value, int bits) {
            arr.Write(value, ref pos, bits);
        }

        public byte[] ToArray() {
            return arr;
        }
    }

    public static class WriterExtensions {
        // sorted by size (almost)

        public static void WriteBool(this Writer writer, bool value) {
            writer.Write(value, 1);
        }
        public static void WriteSByte(this Writer writer, sbyte value, int bits = 8) {
            writer.Write(value.ZigZag(), bits);
        }
        public static void WriteByte(this Writer writer, byte value, int bits = 8) {
            writer.Write(value, bits);
        }
        public static void WriteByteArray(this Writer writer, byte[] value) {
            // write length
            writer.Write(value.Length.ZigZag(), 32);

            for (int i = 0; i < value.Length; i++) {
                writer.Write(value[i], 8);
            }
        }
        public static void WriteChar(this Writer writer, char value, int bits = 16) {
            writer.Write(value, bits);
        }
        public static void WriteShort(this Writer writer, short value, int bits = 16) {
            writer.Write(value.ZigZag(), bits);
        }
        public static void WriteUShort(this Writer writer, ushort value, int bits = 16) {
            writer.Write(value, bits);
        }
        public static void WriteInt(this Writer writer, int value, int bits = 32) {
            writer.Write(value.ZigZag(), bits);
        }
        public static void WriteUInt(this Writer writer, uint value, int bits = 32) {
            writer.Write(value, bits);
        }
        public static void WriteFloat(this Writer writer, float value) {
            writer.Write(value, 32);
        }
        public static void WriteLong(this Writer writer, long value, int bits = 64) {
            writer.Write(value.ZigZag(), bits);
        }
        public static void WriteULong(this Writer writer, ulong value, int bits = 64) {
            writer.Write(value, bits);
        }
        public static void WriteDouble(this Writer writer, double value) {
            writer.Write(value, 64);
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
        public static void WriteParameters(this Writer writer, Parameters value) {
            if (value.Count == 0) {
                WriteBool(writer, false);
                return;
            } else {
                WriteBool(writer, true);
                WriteByte(writer, value.Count);

                for (int i = 0; i < value.Count; i++) {
                    // TODO: FINISH OR DELETE
                }
            }
        }
    }
}
