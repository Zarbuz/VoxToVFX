using UnityEngine;

namespace ColorConversion
{
	public static class ColorConversion
	{
		public static Color ToUnityColor(this FileToVoxCore.Drawing.Color color)
		{
			Color result = new Color(color.R / (float)255, color.G / (float)255, color.B / (float)255, color.A / (float)255);
			return result;
		}
	}
}
