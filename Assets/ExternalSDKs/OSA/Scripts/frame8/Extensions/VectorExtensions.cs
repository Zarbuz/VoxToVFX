using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace frame8.Logic.Misc.Other.Extensions
{
    public static class VectorExtensions
	{
		public static bool HasComponentsWithin01(this Vector2 v)
		{
			if (v.x < 0f || v.x > 1f || v.y < 0f || v.y > 1f)
				return false;
			return true;
		}

		public static Vector2 Rotate(this Vector2 v, float degreesCounterClockwise)
		{
			float radians = degreesCounterClockwise * Mathf.Deg2Rad;
			float cos = Mathf.Cos(radians);
			float sin = Mathf.Sin(radians);
			float x = v.x, y = v.y;
			v.x = cos * x - sin * y;
			v.y = sin * x + cos * y;

			return v;
		}

	}
}
