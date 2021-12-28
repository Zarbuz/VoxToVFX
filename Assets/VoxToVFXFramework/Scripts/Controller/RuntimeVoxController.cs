using FileToVoxCore.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;

[RequireComponent(typeof(VisualEffect))]
public class RuntimeVoxController : MonoBehaviour
{
	#region SerializeFields

	[SerializeField] private Transform MainCamera;
	[Range(2, 10)]
	[SerializeField] private int ChunkLoadDistance = 10;
	[SerializeField] private List<VisualEffectAsset> EffectAssets;
	[SerializeField] private bool MatchCapacityAtRuntime;
	#endregion

	#region ConstStatic

	private const int STEP_CAPACITY = 20000;
	private const int COUNT_ASSETS_TO_GENERATE = 500;

	private const string MAIN_VFX_BUFFER = "Buffer";
	private const string MATERIAL_VFX_BUFFER = "MaterialBuffer";
	private const string FRAMEWORK_FOLDER = "VoxToVFXFramework";

	#endregion

	#region Fields

	private VisualEffect mVisualEffect;
	private GraphicsBuffer mVfxBuffer;
	private GraphicsBuffer mPaletteBuffer;

	private CustomSchematic mCustomSchematic;
	private BoxCollider[] mBoxColliders;

	private Vector3 mPreviousPosition;
	private long mPreviousChunkIndex;

	#endregion


	#region UnityMethods

	private void Start()
	{
		bool b = VerifyEffectAssetsList();
		if (!b && MatchCapacityAtRuntime)
		{
			Debug.LogError("[RuntimeVoxController] EffectAssets count is different to COUNT_ASSETS_TO_GENERATE: " + EffectAssets.Count + " expect: " + COUNT_ASSETS_TO_GENERATE);
			return;
		}
		InitBoxColliders();
		mVisualEffect = GetComponent<VisualEffect>();
		mVisualEffect.enabled = false;
		VoxImporter voxImporter = new VoxImporter();
		StartCoroutine(voxImporter.LoadVoxModelAsync(Path.Combine(Application.streamingAssetsPath, "Sydney.vox"), OnLoadProgress, OnLoadFinished));
	}

	private void OnDestroy()
	{
		mVfxBuffer?.Release();
		mPaletteBuffer?.Release();
		
		mVfxBuffer = null;
		mPaletteBuffer = null;
	}

	private void Update()
	{
		if (mCustomSchematic == null)
		{
			return;
		}

		if (mPreviousPosition != MainCamera.position)
		{
			mPreviousPosition = MainCamera.position;
			FastMath.FloorToInt(mPreviousPosition.x / CustomSchematic.CHUNK_SIZE, mPreviousPosition.y / CustomSchematic.CHUNK_SIZE, mPreviousPosition.z / CustomSchematic.CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);
			long chunkIndex = CustomSchematic.GetVoxelIndex(chunkX, chunkY, chunkZ);

			if (mPreviousChunkIndex != chunkIndex)
			{
				mPreviousChunkIndex = chunkIndex;
				CreateBoxColliderForCurrentChunk(chunkIndex);
				LoadVoxelDataAroundCamera(chunkX, chunkY, chunkZ);
			}
		}
	}

	#endregion

	#region UnityEditor

#if UNITY_EDITOR

	[Button]
	private void GenerateAssets()
	{
		string path = Path.Combine(Application.dataPath, FRAMEWORK_FOLDER, "VFX", "VoxImporterV2.vfx");
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

		EffectAssets.Clear();
		string pathOutput = Path.Combine(Application.dataPath, FRAMEWORK_FOLDER, "VFX", "Opaque");
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
			string targetFileName = "OpaqueVFX-" + newCapacity + ".vfx";
			File.WriteAllLines(Path.Combine(pathOutput, targetFileName), lines);

			string relativePath = "Assets/" + FRAMEWORK_FOLDER + "/VFX/OPAQUE/" + targetFileName;
			UnityEditor.AssetDatabase.ImportAsset(relativePath);
			VisualEffectAsset visualEffectAsset = (VisualEffectAsset)UnityEditor.AssetDatabase.LoadAssetAtPath(relativePath, typeof(VisualEffectAsset));
			EffectAssets.Add(visualEffectAsset);
		}
	}

#endif

	#endregion

	#region PrivateMethods

	private bool VerifyEffectAssetsList()
	{
		return EffectAssets.Count == COUNT_ASSETS_TO_GENERATE;
	}

	private void InitBoxColliders()
	{
		mBoxColliders = new BoxCollider[1000];
		GameObject boxColliderParent = new GameObject("BoxColliders");
		for (int i = 0; i < 1000; i++)
		{
			GameObject go = new GameObject("BoxCollider " + i);
			go.transform.SetParent(boxColliderParent.transform);
			BoxCollider boxCollider = go.AddComponent<BoxCollider>();
			mBoxColliders[i] = boxCollider;
		}
	}

	private void OnLoadProgress(float progress)
	{
		Debug.Log("[RuntimeVoxController] Load progress: " + progress);
	}

	private void OnLoadFinished(VoxelDataVFX voxelData)
	{
		List<VoxelVFX> voxels = voxelData.CustomSchematic.GetAllVoxels();
		int targetPositionX = voxelData.CustomSchematic.Width / 2;
		int targetPositionY = voxelData.CustomSchematic.Height / 2;
		int targetPositionZ = voxelData.CustomSchematic.Length / 2;
		MainCamera.position = new Vector3(targetPositionX, targetPositionY, targetPositionZ);

		Debug.Log("[RuntimeVoxController] OnLoadFinished: " + voxels.Count);

		//mVfxBuffer.SetData(voxels);

		mPaletteBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxelData.Materials.Length, Marshal.SizeOf(typeof(VoxelMaterialVFX)));
		mPaletteBuffer.SetData(voxelData.Materials);
		mVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER, mPaletteBuffer);

		mVisualEffect.enabled = true;
		mCustomSchematic = voxelData.CustomSchematic;
	}


	private void CreateBoxColliderForCurrentChunk(long chunkIndex)
	{
		int i = 0;

		if (mCustomSchematic.RegionDict.TryGetValue(chunkIndex, out Region region))
		{
			foreach (VoxelVFX voxel in region.BlockDict.Values)
			{
				if (i < mBoxColliders.Length)
				{
					BoxCollider boxCollider = mBoxColliders[i];
					boxCollider.transform.position = voxel.position;
					i++;
				}
				else
				{
					Debug.Log("Capacity of box colliders is too small");
					break;
				}
			}
		}


	}

	private void LoadVoxelDataAroundCamera(int chunkX, int chunkY, int chunkZ)
	{
		mVfxBuffer?.Release();

		List<VoxelVFX> list = new List<VoxelVFX>();
		int chunkLoadDistanceRadius = ChunkLoadDistance / 2;
		for (int x = chunkX - chunkLoadDistanceRadius; x <= chunkX + chunkLoadDistanceRadius; x++)
		{
			for (int z = chunkZ - chunkLoadDistanceRadius; z <= chunkZ + chunkLoadDistanceRadius; z++)
			{
				for (int y = chunkY - chunkLoadDistanceRadius; y <= chunkY + chunkLoadDistanceRadius; y++)
				{
					long chunkIndexAt = CustomSchematic.GetVoxelIndex(x, y, z);
					if (mCustomSchematic.RegionDict.ContainsKey(chunkIndexAt))
					{
						list.AddRange(mCustomSchematic.RegionDict[chunkIndexAt].BlockDict.Values);
					}
				}
			}
		}

		if (MatchCapacityAtRuntime)
		{
			mVisualEffect.visualEffectAsset = GetVisualEffectAsset(list.Count);
		}

		mVisualEffect.SetInt("InitialBurstCount", list.Count);
		mVfxBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, list.Count, Marshal.SizeOf(typeof(VoxelVFX)));
		mVfxBuffer.SetData(list);
		mVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER, mVfxBuffer);
		mVisualEffect.Play();
	}


	private VisualEffectAsset GetVisualEffectAsset(int voxels)
	{
		int index = voxels / STEP_CAPACITY;
		if (index > EffectAssets.Count)
		{
			index = EffectAssets.Count - 1;
		}

		return EffectAssets[index];
	}
	#endregion
}

