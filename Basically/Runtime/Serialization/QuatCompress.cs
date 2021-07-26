﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Serialization {
    public static class QuatCompress {
        const float MIN_RANGE = -0.707107f;
        const float MAX_RANGE = 0.707107f;
        const ushort NINE_BIT_MAX = 511;
        const ushort TEN_BIT_MAX = 0x3FF;

        public static void Compress(Quaternion value, out byte index, out ushort aScaled, out ushort bScaled, out ushort cScaled) {
            // range of 0-3 (2 bits)
            index = 0;
            Vector3 withoutLargest = new Vector3(value.y, value.z, value.w);

            { // find largest value
                Vector4 abs = new Vector4(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z), Mathf.Abs(value.w));
                float largest = abs.x;

                if (abs.y > largest) {
                    index = 1;
                    largest = abs.y;
                    withoutLargest = new Vector3(value.x, value.z, value.w);
                }

                if (abs.z > largest) {
                    index = 2;
                    largest = abs.z;
                    withoutLargest = new Vector3(value.x, value.y, value.w);
                }

                if (abs.w > largest) {
                    index = 3;
                    withoutLargest = new Vector3(value.x, value.y, value.z);
                }
            }

            // If negative, negate the vector. (x, y, z, w) == (-x, -y, -z, -w)
            if (value[index] < 0) withoutLargest = -withoutLargest;

            aScaled = ScaleFloatToUShort(withoutLargest.x, MIN_RANGE, MAX_RANGE, 0, NINE_BIT_MAX);
            bScaled = ScaleFloatToUShort(withoutLargest.y, MIN_RANGE, MAX_RANGE, 0, NINE_BIT_MAX);
            cScaled = ScaleFloatToUShort(withoutLargest.z, MIN_RANGE, MAX_RANGE, 0, NINE_BIT_MAX);
        }

        public static Quaternion Decompress(byte index, ushort aScaled, ushort bScaled, ushort cScaled) {
            float a = ScaleUShortToFloat(aScaled, 0, NINE_BIT_MAX, MIN_RANGE, MAX_RANGE);
            float b = ScaleUShortToFloat(bScaled, 0, NINE_BIT_MAX, MIN_RANGE, MAX_RANGE);
            float c = ScaleUShortToFloat(cScaled, 0, NINE_BIT_MAX, MIN_RANGE, MAX_RANGE);
            float d = Mathf.Sqrt(1 - a*a - b*b - c*c);

            Quaternion value;
            switch (index) {
                case 0:  value = new Quaternion(d, a, b, c); break;
                case 1:  value = new Quaternion(a, d, b, c); break;
                case 2:  value = new Quaternion(a, b, d, c); break;
                default: value = new Quaternion(a, b, c, d); break;
            }

            return QuaternionNormalize(value);
        }

        private static ushort ScaleFloatToUShort(float value, float minValue, float maxValue, ushort minTarget, ushort maxTarget) {
            int targetRange = maxTarget - minTarget;
            float valueRange = maxValue - minValue;
            float valueRelative = value - minValue;
            return (ushort)(minTarget + (ushort)(valueRelative / valueRange * targetRange));
        }

        private static float ScaleUShortToFloat(ushort value, ushort minValue, ushort maxValue, float minTarget, float maxTarget) {
            float targetRange = maxTarget - minTarget;
            ushort valueRange = (ushort)(maxValue - minValue);
            ushort valueRelative = (ushort)(value - minValue);
            return minTarget + (valueRelative / (float)valueRange * targetRange);
        }

        private static Quaternion QuaternionNormalize(Quaternion value) {
            const float FLT_MIN_NORMAL = 1.175494351e-38F;

            Vector4 v = new Vector4(value.x, value.y, value.z, value.w);
            float length = Vector4.Dot(v, v);
            return length > FLT_MIN_NORMAL ? value.normalized : Quaternion.identity;
        }
    }
}
