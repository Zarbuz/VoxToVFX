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
	private readonly List<VoxelVFX> mOpaqueList = new List<VoxelVFX>();
	private readonly List<VoxelVFX> mTransparencyList = new List<VoxelVFX>();

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
				EditorCoroutineUtility.StartCoroutine(VoxImporter.LoadVoxModelAsync(path, OnLoadProgress, OnLoadFinished), this);
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
		mConfig.DataTransparent = new List<ListWrapper>();

		VoxImporter.CustomSchematic.UpdateRotations();

		foreach (Region region in VoxImporter.CustomSchematic.RegionDict.Values)
		{
			mOpaqueList.Clear();
			mTransparencyList.Clear();

			mOpaqueList.AddRange(region.BlockDict.Values.Where(v => !v.IsTransparent(VoxImporter.Materials)));
			mTransparencyList.AddRange(region.BlockDict.Values.Where(v => v.IsTransparent(VoxImporter.Materials)));

			
			if (mOpaqueList.Count > 0)
			{
				mConfig.DataOpaque.Add(new ListWrapper()
				{
					List = mOpaqueList
				});
			}

			if (mTransparencyList.Count > 0)
			{
				mConfig.DataTransparent.Add(new ListWrapper()
				{
					List = mTransparencyList
				});
			}
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

		VoxImporter.Clean();
		GC.Collect();
	}

	#endregion
}
