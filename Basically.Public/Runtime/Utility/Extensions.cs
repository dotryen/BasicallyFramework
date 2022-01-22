using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Utility {
    public static class Extensions {
        public static int GetStableHashCode(this string text) {
            unchecked {
                int hash = 23;
                foreach (char c in text) {
                    hash = hash * 31 + c;
                }
                return hash;
            }
        }

        public static ushort BindToUShort(this float value, float minValue, float maxValue, ushort minTarget = 0, ushort maxTarget = ushort.MaxValue) {
            int targetRange = maxTarget - minTarget;
            float valueRange = maxValue - minValue;
            float valueRelative = value - minValue;
            return (ushort)(minTarget + (ushort)(valueRelative / valueRange * targetRange));
        }

        public static float UnbindToFloat(this ushort value, float minTarget, float maxTarget, ushort minValue = 0, ushort maxValue = ushort.MaxValue) {
            float targetRange = maxTarget - minTarget;
            ushort valueRange = (ushort)(maxValue - minValue);
            ushort valueRelative = (ushort)(value - minValue);
            return minTarget + (valueRelative / (float)valueRange * targetRange);
        }
    }
}
