using frame8.Logic.Misc.Other.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Com.TheFallenGames.OSA.Editor
{
	public static class CEditor
	{
		public static List<GameObject> GetSceneRootGameObjects()
		{
			List<GameObject> results = new List<GameObject>();
//			// Introduced in 5.3.2 : https://unity3d.com/unity/whats-new/unity-5.3.2
//#if UNITY_5_3_2 || UNITY_5_3_3 || UNITY_5_3_4 || UNITY_5_3_5 || UNITY_5_3_6 || UNITY_5_4_OR_NEWER
			UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects(results);
//#else
//            var prop = new HierarchyProperty(HierarchyType.GameObjects);
//            var expanded = new int[0];
//            while (prop.Next(expanded))
//                results.Add(prop.pptrValue as GameObject);
//#endif
			return results;
		}

		public static List<GameObject> GetAllGameObjectsInScene()
		{
			var rootGOs = GetSceneRootGameObjects();

			var allGOS = new List<GameObject>();
			foreach (var rootGO in rootGOs)
				foreach (var tr in rootGO.transform.GetDescendants())
					allGOS.Add(tr.gameObject);


			return allGOS;
		}
	}
}
