using UnityEngine;

namespace VoxToVFXFramework.Scripts.Utils.Extensions
{
	public static class ExtensionsGameObject
	{
		public static void SetActiveSafe(this GameObject gameObject, bool setActive)
		{
			if (gameObject.activeSelf != setActive)
			{
				gameObject.SetActive(setActive);
			}
		}

		/// <summary>
		/// Returns the full hierarchy name of the game object.
		/// </summary>
		/// <param name="go">The game object.</param>
		public static string GetFullName(this Transform go)
		{
			string name = go.name;
			while (go.transform.parent != null)
			{

				go = go.transform.parent;
				name = go.name + "/" + name;
			}
			return name;
		}
	}
}