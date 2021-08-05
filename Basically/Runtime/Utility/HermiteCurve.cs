using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Utility {
	public static class HermiteCurve {
		/// <summary>
		/// Creates a Hermite Curve and samples a point. ALL VECTORS ARE IN WORLD SPACE
		/// </summary>
		/// <param name="startPos">Starting position</param>
		/// <param name="startDir">Starting direction</param>
		/// <param name="endPos">Ending position</param>
		/// <param name="endDir">Ending direction</param>
		/// <param name="time">A number from a 0-1 range. Basically lerp.</param>
		/// <returns></returns>
		public static Vector3 Sample(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, float time) {
			time = Mathf.Clamp01(time);
			if (time == 0f) return startPos;
			if (time == 1f) return endPos;

			// godot example implementation
			// var t2 = Mathf.Pow(time, 2);
			// var t3 = Mathf.Pow(time, 3);
			// var a = 1 - 3 * t2 + 2 * t3;
			// var b = t2 * (3 - 2 * time);
			// var c = time * Mathf.Pow(time - 1, 2);
			// var d = t2 * (time - 1);
			// return a * startPos + b * endPos + c * startDir + d * endDir;

			float time2 = time * time;
			float time3 = time * time * time;

			return (((2.0f * time3) - (3.0f * time2) + 1.0f) * startPos)
				  + ((time3 - (2.0f * time2) + time) * startDir)
				  + (((-2.0f * time3) + (3.0f * time2)) * endPos)
				  + ((time3 - time2) * endDir);
		}

		public static void DrawCurve(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, float time = 1) {
			var count = 50;
			for (int i = 0; i < count; i++) {
				var start = Sample(startPos, startDir, endPos, endDir, i / (count - 1f));
				var end = Sample(startPos, startDir, endPos, endDir, (i + 1) / (count - 1f));

				Debug.DrawLine(start, end, Color.yellow, time);
			}

			Debug.DrawLine(startPos, startPos + startDir, Color.white, time);
			Debug.DrawLine(endPos, endPos + endDir, Color.white, time);
		}
	}
}
