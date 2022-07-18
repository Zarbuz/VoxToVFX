using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.ScriptableObjets;

public class VFXCapacityEditor : MonoBehaviour
{
	#region ConstStatic

	private const string FRAMEWORK_FOLDER = "VoxToVFXFramework/Content/VFX/";

	#endregion

	#region PublicMethods

	[MenuItem("VoxToVFX/GenerateAssets")]
	public static void GenerateAssets()
	{
		string path = Path.Combine(Application.dataPath, FRAMEWORK_FOLDER, "VoxImporterV3.vfx");
		if (!File.Exists(path))
		{
			Debug.LogError("VFX asset file not found at: " + path);
			return;
		}

		int capacityLineIndex = 0;
		string[] lines = File.ReadAllLines(path);
		for (int index = 0; index < lines.Length; index++)
		{
			string line = lines[index];
			if (line.Contains("capacity:"))
			{
				capacityLineIndex = index;
				break;
			}
		}

		if (capacityLineIndex == 0)
		{
			Debug.LogError("Failed to found capacity line index in vfx asset! Abort duplicate");
			return;
		}

		string pathAssetConfig = Path.Combine(Application.dataPath, FRAMEWORK_FOLDER, "VFXListAsset.asset");
		VFXListAsset listAsset;
		bool assetConfigExist = false;
		if (File.Exists(pathAssetConfig))
		{
			listAsset = AssetDatabase.LoadAssetAtPath<VFXListAsset>("Assets/" + FRAMEWORK_FOLDER + "VFXListAsset.asset");
			listAsset.VisualEffectAssets = new List<VisualEffectAsset>();
			assetConfigExist = true;
		}
		else
		{
			listAsset = ScriptableObject.CreateInstance<VFXListAsset>();
			listAsset.VisualEffectAssets = new List<VisualEffectAsset>();
		}

		string pathOutput = Path.Combine(Application.dataPath, FRAMEWORK_FOLDER, "Variants");
		if (!Directory.Exists(pathOutput))
		{
			Directory.CreateDirectory(pathOutput);
		}
		else
		{
			DirectoryInfo di = new DirectoryInfo(pathOutput);
			foreach (FileInfo file in di.GetFiles())
			{
				file.Delete();
			}
		}

		for (int i = 1; i <= 50; i++)
		{
			uint newCapacity = (uint)(i * RuntimeVoxManager.STEP_CAPACITY);
			lines[capacityLineIndex] = "  capacity: " + newCapacity;
			string targetFileName = "VoxImporterV3-" + newCapacity + ".vfx";
			File.WriteAllLines(Path.Combine(pathOutput, targetFileName), lines);

			string relativePath = "Assets/" + FRAMEWORK_FOLDER;
			AssetDatabase.ImportAsset(relativePath);

			VisualEffectAsset visualEffectAsset = AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(relativePath);
			listAsset.VisualEffectAssets.Add(visualEffectAsset);
		}

		if (!assetConfigExist)
		{
			AssetDatabase.CreateAsset(listAsset, "Assets/" + FRAMEWORK_FOLDER + "VFXListAsset.asset");
		}
		else
		{
			EditorUtility.SetDirty(listAsset);
		}
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
	#endregion

}
