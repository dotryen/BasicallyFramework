using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Basically.Utility {
    public static class Curves {
		/// <summary>
		/// A Hermite spline
		/// </summary>
		/// <param name="startPos">Starting position</param>
		/// <param name="startDir">Starting direction</param>
		/// <param name="endPos">Ending position</param>
		/// <param name="endDir">Ending direction</param>
		/// <param name="time">A number from a 0-1 range</param>
		/// <returns>Point from curve</returns>
		public static Vector3 Hermite(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, float time) {
			time = Mathf.Clamp01(time);
			if (time == 0f) return startPos;
			if (time == 1f) return endPos;

			float time2 = time * time;
			float time3 = time * time * time;

			return (((2.0f * time3) - (3.0f * time2) + 1.0f) * startPos)
				  + ((time3 - (2.0f * time2) + time) * startDir)
				  + (((-2.0f * time3) + (3.0f * time2)) * endPos)
				  + ((time3 - time2) * endDir);
		}

		/// <summary>
		/// The equivalent of linear interpolation
		/// </summary>
		/// <param name="start">Starting position</param>
		/// <param name="end">Ending position</param>
		/// <param name="time">Normalized distance between the two</param>
		/// <returns>Point from interpolation</returns>
		[System.Obsolete("This is the equivalent of linear interpolation, use Vector3.Lerp instead.")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 LinearBezier(Vector3 start, Vector3 end, float time) {
			return (1f - time) * start + time * end;
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 QuadraticBezier(Vector3 start, Vector3 end, Vector3 tangent, float time) {
			return (1f - time) * Vector3.Lerp(start, tangent, time) + time * Vector3.Lerp(tangent, end, time);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 CubicBezier(Vector3 start, Vector3 startTangent, Vector3 end, Vector3 endTangent, float time) {
			return (1f - time) * QuadraticBezier(start, endTangent, startTangent, time) + time * QuadraticBezier(startTangent, end, endTangent, time);
        }
	}
}

