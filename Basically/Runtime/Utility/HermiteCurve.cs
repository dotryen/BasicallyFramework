using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HermiteCurve {
	/// <summary>
	/// Creates a Hermite Curve and samples a point. ALL VECTORS ARE IN WORLD SPACE
	/// </summary>
	/// <param name="p0">Starting position</param>
	/// <param name="dir0">Starting direction</param>
	/// <param name="p1">Ending position</param>
	/// <param name="dir1">Ending direction</param>
	/// <param name="time">A number from a 0-1 range. Basically lerp.</param>
	/// <returns></returns>
	public static Vector3 Sample(Vector3 p0, Vector3 dir0, Vector3 p1, Vector3 dir1, float time) {
		time = Mathf.Clamp01(time);
		if (time == 0f) return p0;
		if (time == 1f) return p1;

		return (2.0f * time * time * time - 3.0f * time * time + 1.0f) * p0
			  + (time * time * time - 2.0f * time * time + time) * dir0
			  + (-2.0f * time * time * time + 3.0f * time * time) * p1
			  + (time * time * time - time * time) * dir1;
	}

	public static void DrawCurve(Vector3 p0, Vector3 dir0, Vector3 p1, Vector3 dir1, Color color) {
		var count = 50;
		for (int i = 0; i < count; i++) {
			Debug.DrawLine(Sample(p0, dir0, p1, dir1, i / (count - 1)), Sample(p0, dir0, p1, dir1, i + 1 / (count - 1)), color, 1);
		}
	}
}
