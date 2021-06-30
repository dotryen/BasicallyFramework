using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Serialization {
    public class QuaternionSerializer : Serializer<Quaternion> {
        // literally just copying mirror's homework, but they did sum up the article i was reading
        // sooooooo

        const float MIN_RANGE = -0.707107f;
        const float MAX_RANGE = 0.707107f;
        const ushort NINE_BIT_MAX = 511;
        const ushort TEN_BIT_MAX = 0x3FF;

        public override void Write(Buffer buffer, Quaternion value) {
            // normalize because i think it fixes some issues
            value.Normalize();

            // range of 0-3 (2 bits)
            byte largestIndex = 0;
            Vector3 withoutLargest = new Vector3(value.y, value.z, value.w);

            { // find largest value
                Vector4 abs = new Vector4(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z), Mathf.Abs(value.w));
                float largest = abs.x;

                if (abs.y > largest) {
                    largestIndex = 1;
                    largest = abs.y;
                    withoutLargest = new Vector3(value.x, value.z, value.w);
                }

                if (abs.z > largest) {
                    largestIndex = 2;
                    largest = abs.z;
                    withoutLargest = new Vector3(value.x, value.y, value.w);
                }

                if (abs.w > largest) {
                    largestIndex = 3;
                    largest = abs.w;
                    withoutLargest = new Vector3(value.x, value.y, value.z);
                }
            }

            // If negative, negate the vector. (x, y, z, w) == (-x, -y, -z, -w)
            if (value[largestIndex] < 0) withoutLargest = -withoutLargest;

            ushort aScaled = ScaleFloatToUShort(withoutLargest.x, MIN_RANGE, MAX_RANGE, 0, NINE_BIT_MAX);
            ushort bScaled = ScaleFloatToUShort(withoutLargest.y, MIN_RANGE, MAX_RANGE, 0, NINE_BIT_MAX);
            ushort cScaled = ScaleFloatToUShort(withoutLargest.z, MIN_RANGE, MAX_RANGE, 0, NINE_BIT_MAX);

            // result: 29 bits :)
            buffer.Write(largestIndex, 2);
            buffer.Write(aScaled, 9);
            buffer.Write(bScaled, 9);
            buffer.Write(cScaled, 9);
        }

        public override Quaternion Read(Buffer buffer) {
            byte largestIndex = buffer.ReadByte(2);
            ushort aScaled = buffer.ReadUShort(9);
            ushort bScaled = buffer.ReadUShort(9);
            ushort cScaled = buffer.ReadUShort(9);

            float a = ScaleUShortToFloat(aScaled, 0, NINE_BIT_MAX, MIN_RANGE, MAX_RANGE);
            float b = ScaleUShortToFloat(bScaled, 0, NINE_BIT_MAX, MIN_RANGE, MAX_RANGE);
            float c = ScaleUShortToFloat(cScaled, 0, NINE_BIT_MAX, MIN_RANGE, MAX_RANGE);
            float d = Mathf.Sqrt(1 - a*a - b*b - c*c);

            Quaternion value;
            switch (largestIndex) {
                case 0:  value = new Quaternion(d, a, b, c); break;
                case 1:  value = new Quaternion(a, d, b, c); break;
                case 2:  value = new Quaternion(a, b, d, c); break;
                default: value = new Quaternion(a, b, c, d); break;
            }

            return QuaternionNormalize(value);
        }

        private ushort ScaleFloatToUShort(float value, float minValue, float maxValue, ushort minTarget, ushort maxTarget) {
            int targetRange = maxTarget - minTarget;
            float valueRange = maxValue - minValue;
            float valueRelative = value - minValue;
            return (ushort)(minTarget + (ushort)(valueRelative / valueRange * targetRange));
        }

        private float ScaleUShortToFloat(ushort value, ushort minValue, ushort maxValue, float minTarget, float maxTarget) {
            float targetRange = maxTarget - minTarget;
            ushort valueRange = (ushort)(maxValue - minValue);
            ushort valueRelative = (ushort)(value - minValue);
            return minTarget + (valueRelative / (float)valueRange * targetRange);
        }

        private Quaternion QuaternionNormalize(Quaternion value) {
            const float FLT_MIN_NORMAL = 1.175494351e-38F;

            Vector4 v = new Vector4(value.x, value.y, value.z, value.w);
            float length = Vector4.Dot(v, v);
            return length > FLT_MIN_NORMAL ? value.normalized : Quaternion.identity;
        }
    }
}
