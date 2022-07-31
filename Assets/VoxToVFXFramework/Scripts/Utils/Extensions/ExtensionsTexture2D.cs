using UnityEngine;

namespace VoxToVFXFramework.Scripts.Utils.Extensions
{
	public static class ExtensionsTexture2D
	{
		public static Texture2D ResampleAndCrop(this Texture2D source, int targetWidth, int targetHeight)
		{
			int sourceWidth = source.width;
			int sourceHeight = source.height;
			float sourceAspect = (float)sourceWidth / sourceHeight;
			float targetAspect = (float)targetWidth / targetHeight;
			int xOffset = 0;
			int yOffset = 0;
			float factor = 1;
			if (sourceAspect > targetAspect)
			{ // crop width
				factor = (float)targetHeight / sourceHeight;
				xOffset = (int)((sourceWidth - sourceHeight * targetAspect) * 0.5f);
			}
			else
			{ // crop height
				factor = (float)targetWidth / sourceWidth;
				yOffset = (int)((sourceHeight - sourceWidth / targetAspect) * 0.5f);
			}
			Color32[] data = source.GetPixels32();
			Color32[] data2 = new Color32[targetWidth * targetHeight];
			for (int y = 0; y < targetHeight; y++)
			{
				float yPos = y / factor + yOffset;
				int y1 = (int)yPos;
				if (y1 >= sourceHeight)
				{
					y1 = sourceHeight - 1;
					yPos = y1;
				}

				int y2 = y1 + 1;
				if (y2 >= sourceHeight)
					y2 = sourceHeight - 1;
				float fy = yPos - y1;
				y1 *= sourceWidth;
				y2 *= sourceWidth;
				for (int x = 0; x < targetWidth; x++)
				{
					float xPos = x / factor + xOffset;
					int x1 = (int)xPos;
					if (x1 >= sourceWidth)
					{
						x1 = sourceWidth - 1;
						xPos = x1;
					}
					int x2 = x1 + 1;
					if (x2 >= sourceWidth)
						x2 = sourceWidth - 1;
					float fx = xPos - x1;
					var c11 = data[x1 + y1];
					var c12 = data[x1 + y2];
					var c21 = data[x2 + y1];
					var c22 = data[x2 + y2];
					float f11 = (1 - fx) * (1 - fy);
					float f12 = (1 - fx) * fy;
					float f21 = fx * (1 - fy);
					float f22 = fx * fy;
					float r = c11.r * f11 + c12.r * f12 + c21.r * f21 + c22.r * f22;
					float g = c11.g * f11 + c12.g * f12 + c21.g * f21 + c22.g * f22;
					float b = c11.b * f11 + c12.b * f12 + c21.b * f21 + c22.b * f22;
					float a = c11.a * f11 + c12.a * f12 + c21.a * f21 + c22.a * f22;
					int index = x + y * targetWidth;

					data2[index].r = (byte)r;
					data2[index].g = (byte)g;
					data2[index].b = (byte)b;
					data2[index].a = (byte)a;
				}
			}

			var tex = new Texture2D(targetWidth, targetHeight);
			tex.SetPixels32(data2);
			tex.Apply(true);
			return tex;
		}
	}
}
