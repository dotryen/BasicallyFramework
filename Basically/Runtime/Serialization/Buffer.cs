using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using emotitron.Compression;
using emotitron.Compression.Utilities;
using UnityEngine;

namespace Basically.Serialization {
    /// <summary>
    /// Literally byte[] but better.
    /// </summary>
    public class Buffer {
        byte[] array;

        /// <summary>
        /// Which spot will be read.
        /// </summary>
        int readPos = 0;

        /// <summary>
        /// Which spot will be written to.
        /// </summary>
        int writePos = 0;

        /// <summary>
        /// Gets the current read position in bits.
        /// </summary>
        public int CurrentReadPosition => readPos;

        /// <summary>
        /// Gets the current write position in bits.
        /// </summary>
        public int CurrentWritePosition => writePos;

        public Buffer() {
            array = new byte[0];
        }

        public Buffer(int size) {
            array = new byte[size];
        }

        public Buffer(IEnumerable<byte> arr) {
            array = arr.ToArray();
            writePos = array.Length * 8;
        }

        public byte this[int i] {
            get {
                return array[i];
            }

            set {
                array[i] = value;
            }
        }

        public static implicit operator byte[](Buffer buffer) => buffer.ToArray();
        public static implicit operator Buffer(byte[] arr) => new Buffer(arr);

        #region Write

        private void WriteInternal(ByteConverter value, int bits) {
            var maxBits = array.Length * 8;
            // the resulting position is the position after the spot we just wrote to
            var newPos = writePos + bits + 1;
            if (newPos > maxBits) {
                var remainingBits = newPos - maxBits;
                Array.Resize(ref array, array.Length + Mathf.CeilToInt(remainingBits / 8f));
            }

            array.Write(value, ref writePos, bits);
        }

        public void Write(bool value) {
            WriteInternal(value, 1);
        }

        public void Write(sbyte value, int bits = 8) {
            WriteInternal(value.ZigZag(), bits);
        }

        public void Write(byte value, int bits = 8) {
            WriteInternal(value, bits);
        }

        public void Write(byte[] value) {
            // write length
            WriteInternal(value.Length.ZigZag(), 32);

            for (int i = 0; i < value.Length; i++) {
                WriteInternal(value[i], 8);
            }
        }

        public void Write(char value, int bits = 16) {
            WriteInternal(value, bits);
        }

        public void Write(short value, int bits = 16) {
            WriteInternal(value.ZigZag(), bits);
        }

        public void Write(ushort value, int bits = 16) {
            WriteInternal(value, bits);
        }

        public void Write(int value, int bits = 32) {
            WriteInternal(value.ZigZag(), bits);
        }

        public void Write(uint value, int bits = 32) {
            WriteInternal(value, bits);
        }

        public void Write(float value) {
            WriteInternal(value, 32);
        }

        public void Write(long value, int bits = 64) {
            WriteInternal(value.ZigZag(), bits);
        }

        public void Write(ulong value, int bits = 64) {
            WriteInternal(value, bits);
        }

        public void Write(double value) {
            WriteInternal(value, 64);
        }

        #endregion

        #region Read

        private ByteConverter ReadInternal(int bits) {
            return array.Read(ref readPos, bits);
        }

        public bool ReadBool() {
            return ReadInternal(1);
        }

        public byte ReadByte(int bits = 8) {
            return ReadInternal(bits);
        }

        public sbyte ReadSByte(int bits = 8) {
            return ReadByte(bits).UnZigZag();
        }

        public byte[] ReadByteArray() {
            var buf = new byte[ReadInt()];

            for (int i = 0; i < buf.Length; i++) {
                buf[i] = ReadByte();
            }

            return buf;
        }

        public char ReadChar(int bits = 16) {
            return ReadInternal(bits);
        }

        public ushort ReadUShort(int bits = 16) {
            return ReadInternal(bits);
        }

        public short ReadShort(int bits = 16) {
            return ReadUShort(bits).UnZigZag();
        }

        public uint ReadUInt(int bits = 32) {
            return ReadInternal(bits);
        }

        public int ReadInt(int bits = 32) {
            return ReadUInt(bits).UnZigZag();
        }

        public float ReadFloat() {
            return ReadInternal(32);
        }

        public ulong ReadULong(int bits = 64) {
            return ReadInternal(bits);
        }

        public long ReadLong(int bits = 64) {
            return ReadULong(bits).UnZigZag();
        }

        public double ReadDouble() {
            return ReadInternal(64);
        }

        #endregion

        public byte[] ToArray() {
            return array;
        }
    }
}
