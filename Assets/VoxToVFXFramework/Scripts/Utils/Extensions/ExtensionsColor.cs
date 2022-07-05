using UnityEngine;

namespace VoxToVFXFramework.Scripts.Utils.Extensions
{
	public static class ExtensionsColor
	{
		#region Methods

		public static Color R(this Color color, float red)
		{
			color.r = red;
			return color;
		}

		public static Color G(this Color color, float green)
		{
			color.g = green;
			return color;
		}

		public static Color B(this Color color, float blue)
		{
			color.b = blue;
			return color;
		}

		public static Color A(this Color color, float alpha)
		{
			color.a = alpha;
			return color;
		}

		#endregion
	}
}