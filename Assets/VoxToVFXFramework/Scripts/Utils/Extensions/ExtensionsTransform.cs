using UnityEngine;

namespace VoxToVFXFramework.Scripts.Utils.Extensions
{
	public static class ExtensionsTransform
	{
		public static void DestroyAllChildren(this Transform parent)
		{
			for (int i = 0; i < parent.childCount; ++i)
			{
				Transform child = parent.GetChild(i);
				Object.Destroy(child.gameObject);
			}
		}
	}
}
