using System;
using emotitron.Compression;
using emotitron.Compression.Utilities;
using UnityEngine;

namespace Basically.Serialization {
    using Utility;

    public class Reader {
        readonly byte[] arr;
        int pos;

        internal Reader(int size) {
            arr = new byte[size];
            pos = 0;
        }

        internal ByteConverter ReadInternal(int bits) {
            return arr.Read(ref pos, bits);
        }

        public T Read<T>() {
            var act = TypeData<T>.read;
            if (act == null) {
                Debug.LogError($"No reader for {typeof(T)}");
                return default;
            } else {
                return act(this);
            }
        }

        public T Read<T>(int bits) {
            var act = TypeData<T>.readBit;
            if (act == null) {
                Debug.LogError($"No bit reader for {typeof(T)}");
                return default;
            } else {
                return act(this, bits);
            }
        }

        public void Reset() {
            for (int i = 0; i < arr.Length; i++) {
                arr[i] = 0;
            }
            pos = 0;
        }

        internal byte[] ToArray() {
            return arr;
        }
    }

    public static class ReaderExtensions {
        public static bool ReadBool(this Reader reader) {
            return reader.ReadInternal(1);
        }
        public static byte ReadByte(this Reader reader, int bits = 8) {
            return reader.ReadInternal(bits);
        }
        public static sbyte ReadSByte(this Reader reader, int bits = 8) {
            return ReadByte(reader, bits).UnZigZag();
        }
        public static char ReadChar(this Reader reader) {
            return reader.ReadInternal(16);
        }
        public static ushort ReadUShort(this Reader reader, int bits = 16) {
            return reader.ReadInternal(bits);
        }
        public static short ReadShort(this Reader reader, int bits = 16) {
            return ReadUShort(reader, bits).UnZigZag();
        }
        public static uint ReadUInt(this Reader reader, int bits = 32) {
            return reader.ReadInternal(bits);
        }
        public static int ReadInt(this Reader reader, int bits = 32) {
            return ReadUInt(reader, bits).UnZigZag();
        }
        public static float ReadFloat(this Reader reader) {
            return reader.ReadInternal(32);
        }
        public static ulong ReadULong(this Reader reader, int bits = 64) {
            return reader.ReadInternal(bits);
        }
        public static long ReadLong(this Reader reader, int bits = 64) {
            return ReadULong(reader, bits).UnZigZag();
        }
        public static double ReadDouble(this Reader reader) {
            return reader.ReadInternal(64);
        }
        public static Vector2 ReadVector2(this Reader reader) {
            return new Vector2(ReadFloat(reader), ReadFloat(reader));
        }
        public static Vector3 ReadVector3(this Reader reader) {
            return new Vector3(ReadFloat(reader), ReadFloat(reader), ReadFloat(reader));
        }
        public static Quaternion ReadQuaternion(this Reader reader) {
            return QuatCompress.Decompress(ReadByte(reader, 2), ReadUShort(reader, 9), ReadUShort(reader, 9), ReadUShort(reader, 9));
        }
        public static T[] ReadArray<T>(this Reader reader) {
            if (!reader.ReadBool()) return null;
            var length = reader.ReadInt();

            var arr = new T[length];
            for (int i = 0; i < length; i++) {
                arr[i] = reader.Read<T>();
            }
            return arr;
        }
        public static Parameters ReadParemeters(this Reader reader) {
            if (!reader.ReadBool()) return default;

            byte length = reader.ReadByte();
            var param = new Parameters(length);

            for (int i = 0; i < length; i++) {
                param.keys[i] = reader.ReadInt();
                param.valuesBytes[i] = reader.ReadArray<byte>();
            }

            return param;
        }
        public static string ReadString(this Reader reader) {
            return System.Text.Encoding.UTF8.GetString(reader.ReadArray<byte>());
        }
    }
}
