using UnityEngine;

namespace VoxToVFXFramework.Scripts.Utils.Extensions
{
	public static class ExtensionsTransform
	{
		public static void DestroyAllChildren(this Transform parent, bool ignoreFirst = false)
		{
			for (int i = 0; i < parent.childCount; ++i)
			{
				if (ignoreFirst && i == 0)
					continue;

				Transform child = parent.GetChild(i);
				Object.Destroy(child.gameObject);
			}
		}

		public static int CountActiveChild(this Transform parent)
		{
			int count = 0;
			for (int i = 0; i < parent.childCount; ++i)
			{
				Transform child = parent.GetChild(i);
				if (child.gameObject.activeSelf)
				{
					count++;
				}
			}
			return count;
		}
	}
}
