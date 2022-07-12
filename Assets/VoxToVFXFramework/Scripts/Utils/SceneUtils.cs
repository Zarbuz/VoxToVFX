using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VoxToVFXFramework.Scripts.Utils
{
	public static class SceneUtils
	{
		public static List<T> FindObjectsOfTypeAll<T>()
		{
			List<T> results = new List<T>();
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				Scene s = SceneManager.GetSceneAt(i);
				if (s.isLoaded)
				{
					GameObject[] allGameObjects = s.GetRootGameObjects();
					foreach (GameObject go in allGameObjects)
					{
						results.AddRange(go.GetComponentsInChildren<T>(true));
					}
				}
			}
			return results;
		}

		public static T FindObjectOfType<T>()
		{
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				Scene s = SceneManager.GetSceneAt(i);
				if (s.isLoaded)
				{
					GameObject[] allGameObjects = s.GetRootGameObjects();
					foreach (GameObject go in allGameObjects)
					{
						T component = go.GetComponent<T>();
						if (component != null)
						{
							return component;
						}
					}
				}
			}
			return default;
		}
	}
}