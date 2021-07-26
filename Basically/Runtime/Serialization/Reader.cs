﻿using System;
using emotitron.Compression;
using emotitron.Compression.Utilities;
using UnityEngine;

namespace Basically.Serialization {
    internal static class Reader<T> {
        public static Func<Reader, T> read;
    }

    public class Reader {
        readonly byte[] arr;
        int pos;

        internal Reader(int size) {
            arr = new byte[size];
            pos = 0;
        }

        internal ByteConverter Read(int bits) {
            return arr.Read(ref pos, bits);
        }
    }

    public static class ReaderExtensions {
        public static bool ReadBool(this Reader reader) {
            return reader.Read(1);
        }
        public static byte ReadByte(this Reader reader, int bits = 8) {
            return reader.Read(bits);
        }
        public static sbyte ReadSByte(this Reader reader, int bits = 8) {
            return ReadByte(reader, bits).UnZigZag();
        }
        public static byte[] ReadByteArray(this Reader reader) {
            var buf = new byte[ReadInt(reader)];

            for (int i = 0; i < buf.Length; i++) {
                buf[i] = ReadByte(reader);
            }

            return buf;
        }
        public static char ReadChar(this Reader reader, int bits = 16) {
            return reader.Read(bits);
        }
        public static ushort ReadUShort(this Reader reader, int bits = 16) {
            return reader.Read(bits);
        }
        public static short ReadShort(this Reader reader, int bits = 16) {
            return ReadUShort(reader, bits).UnZigZag();
        }
        public static uint ReadUInt(this Reader reader, int bits = 32) {
            return reader.Read(bits);
        }
        public static int ReadInt(this Reader reader, int bits = 32) {
            return ReadUInt(reader, bits).UnZigZag();
        }
        public static float ReadFloat(this Reader reader) {
            return reader.Read(32);
        }
        public static ulong ReadULong(this Reader reader, int bits = 64) {
            return reader.Read(bits);
        }
        public static long ReadLong(this Reader reader, int bits = 64) {
            return ReadULong(reader, bits).UnZigZag();
        }
        public static double ReadDouble(this Reader reader) {
            return reader.Read(64);
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
    }
}