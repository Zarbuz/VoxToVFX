using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;
using VoxToVFXFramework.Scripts.ScriptableObjects;

[CustomEditor(typeof(VisualEffectDataConfig), true)]
public class VisualEffectDataConfigEditor : Editor
{
	#region Fields

	private VisualEffectDataConfig mConfig;

	#endregion

	#region UnityMethods

	public override void OnInspectorGUI()
	{
		mConfig = (VisualEffectDataConfig)target;
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Load Data from VOX file"))
		{
			string path = EditorUtility.OpenFilePanel("Select vox file", "", "vox");
			if (!string.IsNullOrEmpty(path))
			{
				//EditorCoroutineUtility.StartCoroutine(VoxImporter.LoadVoxModelAsync(path, OnLoadProgress, OnLoadFinished), this);
			}
		}

		EditorGUILayout.EndHorizontal();
		DrawDefaultInspector();

	}

	#endregion

	#region PrivateMethods

	private void OnLoadProgress(float progress)
	{
		EditorUtility.DisplayProgressBar("Load vox file", "Parsing...", progress);
	}

	private void OnLoadFinished(bool success)
	{
		EditorUtility.ClearProgressBar();
		if (!success)
		{
			EditorUtility.DisplayDialog("Error", "Failed to load vox file", "Ok");
			return;
		}

		mConfig.Material = VoxImporter.Materials;
		mConfig.DataOpaque = new List<ListWrapper>();

		foreach (Region region in VoxImporter.CustomSchematic.RegionDict.Values)
		{
			//mConfig.DataOpaque.Add(new ListWrapper()
			//{
			//	List = region.BlockDict.Values.ToList()
			//});
		}

		if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mConfig, out string guid, out long localId))
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			VisualEffectDataConfig asset = (VisualEffectDataConfig)AssetDatabase.LoadAssetAtPath(path, typeof(VisualEffectDataConfig));
			EditorUtility.SetDirty(asset);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			Debug.Log("Saved!");
		}
		else
		{
			Debug.LogError("Failed to save");
		}
	}

	#endregion
}
