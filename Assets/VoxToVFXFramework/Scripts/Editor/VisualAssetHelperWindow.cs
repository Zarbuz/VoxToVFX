using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

public class VisualAssetHelperWindow : EditorWindow
{
	#region ConstStatic

	private const string FRAMEWORK_VFX_FOLDER = "VoxToVFXFramework/Content/VFX";

	#endregion

	#region Fields

	private RuntimeVoxController mRuntimeVoxController;

	#endregion

	#region UnityMethods

	private void OnGUI()
	{
		EditorGUILayout.BeginHorizontal();
		mRuntimeVoxController = (RuntimeVoxController)EditorGUILayout.ObjectField(mRuntimeVoxController, typeof(RuntimeVoxController), true);
		EditorGUILayout.EndHorizontal();

		if (GUILayout.Button("GenerateAssets"))
		{
			if (mRuntimeVoxController == null)
			{
				EditorUtility.DisplayDialog("VoxToVFX - Error", "You must link a RuntimeVoxController before generating visual assets", "Ok");
				return;
			}

			GenerateAssets();
		}
	}

	#endregion

	#region PublicMethods

	#endregion

	#region PrivateMethods

	[MenuItem("Tools/VoxToVFX/Visual Assets Helper")]
	private static void Init()
	{
		VisualAssetHelperWindow window = GetWindow<VisualAssetHelperWindow>(typeof(VisualAssetHelperWindow));
		window.Show();
	}

	private void GenerateAssets()
	{
		if (WriteAllVisualAssets(Path.Combine(Application.dataPath, FRAMEWORK_VFX_FOLDER, "VoxImporterV2.vfx"), "Opaque", out List<VisualEffectAsset> l1))
		{
			mRuntimeVoxController.SetOpaqueVisualEffectsList(l1);
			EditorUtility.SetDirty(mRuntimeVoxController.gameObject);
		}

		if (WriteAllVisualAssets(Path.Combine(Application.dataPath, FRAMEWORK_VFX_FOLDER, "VoxImporterV2Transparency.vfx"), "Transparency", out List<VisualEffectAsset> l2))
		{
			mRuntimeVoxController.SetTransparenceVisualEffectsList(l2);
			EditorUtility.SetDirty(mRuntimeVoxController.gameObject);
		}
	}

	private static bool WriteAllVisualAssets(string inputPath, string prefixName, out List<VisualEffectAsset> assets)
	{
		assets = new List<VisualEffectAsset>();
		if (!File.Exists(inputPath))
		{
			Debug.LogError("VFX asset file not found at: " + inputPath);
			return false;
		}

		int capacityLineIndex = 0;
		string[] lines = File.ReadAllLines(inputPath);
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
			return false;
		}

		string pathOutput = Path.Combine(Application.dataPath, FRAMEWORK_VFX_FOLDER, prefixName);
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

		for (int i = 1; i <= RuntimeVoxController.COUNT_ASSETS_TO_GENERATE; i++)
		{
			uint newCapacity = (uint)(i * RuntimeVoxController.STEP_CAPACITY);
			lines[capacityLineIndex] = "  capacity: " + newCapacity;
			string targetFileName = prefixName + "VFX-" + newCapacity + ".vfx";
			File.WriteAllLines(Path.Combine(pathOutput, targetFileName), lines);

			string relativePath = "Assets/" + FRAMEWORK_VFX_FOLDER + "/" + prefixName + "/" + targetFileName;
			AssetDatabase.ImportAsset(relativePath);
			VisualEffectAsset visualEffectAsset = (VisualEffectAsset)AssetDatabase.LoadAssetAtPath(relativePath, typeof(VisualEffectAsset));
			assets.Add(visualEffectAsset);
		}

		return true;
	}


	#endregion
}
