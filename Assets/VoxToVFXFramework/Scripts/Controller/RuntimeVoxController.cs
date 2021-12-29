using System;
using System.Collections;
using FileToVoxCore.Utils;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;
using VoxToVFXFramework.Scripts.Managers;

public class RuntimeVoxController : MonoBehaviour
{
	#region SerializeFields

	[Header("Visual Effects")]
	[SerializeField] private VisualEffect OpaqueVisualEffect;
	[SerializeField] private VisualEffect TransparenceVisualEffect;

	[Header("Camera Settings")]
	[SerializeField] private Transform MainCamera;
	[Range(2, 10)]
	[OnValueChanged(nameof(OnChunkLoadDistanceValueChanged))]
	[SerializeField] private int ChunkLoadDistance = 10;

	[Range(10, 200)]
	[SerializeField] private int DetailLoadDistance = 140;
	[Header("Debug Settings")]
	[SerializeField] private bool DebugVisualEffects;

	[Header("VisualEffectAssets")]
	[SerializeField] private List<VisualEffectAsset> OpaqueVisualEffects;
	[SerializeField] private List<VisualEffectAsset> TransparenceVisualEffects;
	#endregion

	#region ConstStatic

	private const int STEP_CAPACITY = 20000;
	private const int COUNT_ASSETS_TO_GENERATE = 500;

	private const string MAIN_VFX_BUFFER_KEY = "Buffer";
	private const string MATERIAL_VFX_BUFFER_KEY = "MaterialBuffer";
	private const string ROTATION_VFX_BUFFER_KEY = "RotationBuffer";
	private const string DETAIL_LOAD_DISTANCE_KEY = "DetailLoadDistance";
	private const string FRAMEWORK_FOLDER = "VoxToVFXFramework";
	private const string FRAMEWORK_VFX_FOLDER = "VFX";

	#endregion

	#region Fields

	private GraphicsBuffer mOpaqueBuffer;
	private GraphicsBuffer mTransparencyBuffer;
	private GraphicsBuffer mPaletteBuffer;
	private GraphicsBuffer mRotationBuffer;

	private CustomSchematic mCustomSchematic;
	private BoxCollider[] mBoxColliders;
	private VoxelMaterialVFX[] mMaterials;

	private Vector3 mPreviousPosition;
	private long mPreviousChunkIndex;
	private int mPreviousDetailLoadDistance;

	private Thread mThread;
	private readonly List<VoxelVFX> mList = new List<VoxelVFX>();
	private readonly List<VoxelVFX> mOpaqueList = new List<VoxelVFX>();
	private readonly List<VoxelVFX> mTransparencyList = new List<VoxelVFX>();
	private static event Action ChunkDataLoaded;
	#endregion

	#region UnityMethods

	private void Start()
	{
		ChunkDataLoaded += OnChunkDataLoaded;
		bool b = VerifyEffectAssetsList();
		if (!b && !DebugVisualEffects)
		{
			Debug.LogError("[RuntimeVoxController] EffectAssets count is different to COUNT_ASSETS_TO_GENERATE: " + OpaqueVisualEffects.Count + " expect: " + COUNT_ASSETS_TO_GENERATE);
			return;
		}
		//InitBoxColliders();
		VoxImporter voxImporter = new VoxImporter();
		StartCoroutine(voxImporter.LoadVoxModelAsync(Path.Combine(Application.streamingAssetsPath, "Sydney.vox"), OnLoadProgress, OnLoadFinished));
	}

	private void OnDestroy()
	{
		ChunkDataLoaded -= OnChunkDataLoaded;
		mOpaqueBuffer?.Release();
		mTransparencyBuffer?.Release();
		mPaletteBuffer?.Release();
		mRotationBuffer?.Release();
	}

	private void FixedUpdate()
	{
		if (mCustomSchematic == null)
			return;

		if (mPreviousDetailLoadDistance != DetailLoadDistance)
		{
			mPreviousDetailLoadDistance = DetailLoadDistance;
			UpdateDetailLoadDistance();
		}

		if (mPreviousPosition != MainCamera.position)
		{
			mPreviousPosition = MainCamera.position;
			FastMath.FloorToInt(mPreviousPosition.x / CustomSchematic.CHUNK_SIZE, mPreviousPosition.y / CustomSchematic.CHUNK_SIZE, mPreviousPosition.z / CustomSchematic.CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);
			long chunkIndex = CustomSchematic.GetVoxelIndex(chunkX, chunkY, chunkZ);

			if (mPreviousChunkIndex != chunkIndex)
			{
				mPreviousChunkIndex = chunkIndex;
				//CreateBoxColliderForCurrentChunk(chunkIndex);
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
		if (WriteAllVisualAssets(Path.Combine(Application.dataPath, FRAMEWORK_FOLDER, FRAMEWORK_VFX_FOLDER, "VoxImporterV2.vfx"), "Opaque", out List<VisualEffectAsset> l1))
		{
			OpaqueVisualEffects.Clear();
			OpaqueVisualEffects.AddRange(l1);
		}

		if (WriteAllVisualAssets(Path.Combine(Application.dataPath, FRAMEWORK_FOLDER, FRAMEWORK_VFX_FOLDER, "VoxImporterV2Transparency.vfx"), "Transparency", out List<VisualEffectAsset> l2))
		{
			TransparenceVisualEffects.Clear();
			TransparenceVisualEffects.AddRange(l2);
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

		string pathOutput = Path.Combine(Application.dataPath, FRAMEWORK_FOLDER, FRAMEWORK_VFX_FOLDER, prefixName);
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

			string relativePath = "Assets/" + FRAMEWORK_FOLDER + "/" + FRAMEWORK_VFX_FOLDER + "/" + prefixName + "/" + targetFileName;
			UnityEditor.AssetDatabase.ImportAsset(relativePath);
			VisualEffectAsset visualEffectAsset = (VisualEffectAsset)UnityEditor.AssetDatabase.LoadAssetAtPath(relativePath, typeof(VisualEffectAsset));
			assets.Add(visualEffectAsset);
		}

		return true;
	}

#endif

	#endregion

	#region PrivateMethods

	private bool VerifyEffectAssetsList()
	{
		return OpaqueVisualEffects.Count == COUNT_ASSETS_TO_GENERATE;
	}

	//private void InitBoxColliders()
	//{
	//	mBoxColliders = new BoxCollider[1000];
	//	GameObject boxColliderParent = new GameObject("BoxColliders");
	//	for (int i = 0; i < 1000; i++)
	//	{
	//		GameObject go = new GameObject("BoxCollider " + i);
	//		go.transform.SetParent(boxColliderParent.transform);
	//		BoxCollider boxCollider = go.AddComponent<BoxCollider>();
	//		mBoxColliders[i] = boxCollider;
	//	}
	//}

	private void OnLoadProgress(float progress)
	{
		Debug.Log("[RuntimeVoxController] Load progress: " + progress * 100);
	}

	private void OnLoadFinished(VoxelDataVFX voxelData)
	{
		int count = voxelData.CustomSchematic.UpdateRotations();

		Debug.Log("[RuntimeVoxController] OnLoadFinished: " + count);
		int targetPositionX = voxelData.CustomSchematic.Width / 2;
		int targetPositionY = voxelData.CustomSchematic.Height / 2;
		int targetPositionZ = voxelData.CustomSchematic.Length / 2;
		MainCamera.position = new Vector3(targetPositionX, targetPositionY, targetPositionZ);

		mPaletteBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxelData.Materials.Length, Marshal.SizeOf(typeof(VoxelMaterialVFX)));
		mPaletteBuffer.SetData(voxelData.Materials);
		OpaqueVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER_KEY, mPaletteBuffer);
		TransparenceVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER_KEY, mPaletteBuffer);

		List<VoxelRotationVFX> rotations = new List<VoxelRotationVFX>();
		rotations.Add(new VoxelRotationVFX()
		{
			pivot = Vector3.zero,
			rotation = Vector3.zero
		});

		rotations.Add(new VoxelRotationVFX()
		{
			pivot = new Vector3(0, 0, 0.5f),
			rotation = new Vector3(90, 0, 0)
		});

		rotations.Add(new VoxelRotationVFX()
		{
			pivot = new Vector3(0, 0, 0.5f),
			rotation = new Vector3(0, 180, 0)
		});

		rotations.Add(new VoxelRotationVFX()
		{
			pivot = new Vector3(0, 0, 0.5f),
			rotation = new Vector3(0, 90, 0)
		});

		mRotationBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, rotations.Count, Marshal.SizeOf(typeof(VoxelRotationVFX)));
		mRotationBuffer.SetData(rotations);

		OpaqueVisualEffect.SetGraphicsBuffer(ROTATION_VFX_BUFFER_KEY, mRotationBuffer);
		TransparenceVisualEffect.SetGraphicsBuffer(ROTATION_VFX_BUFFER_KEY, mRotationBuffer);
		mMaterials = voxelData.Materials;
		voxelData.CustomSchematic.UpdateRotations();
		Debug.Log("[RuntimeVoxController] OnUpdateRotationFinished");

		OpaqueVisualEffect.enabled = true;
		TransparenceVisualEffect.enabled = true;
		mCustomSchematic = voxelData.CustomSchematic;
	}

	private void OnChunkLoadDistanceValueChanged()
	{
		mPreviousPosition = MainCamera.position;
		FastMath.FloorToInt(mPreviousPosition.x / CustomSchematic.CHUNK_SIZE, mPreviousPosition.y / CustomSchematic.CHUNK_SIZE, mPreviousPosition.z / CustomSchematic.CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);
		LoadVoxelDataAroundCamera(chunkX, chunkY, chunkZ);
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

	private void UpdateDetailLoadDistance()
	{
		OpaqueVisualEffect.SetInt(DETAIL_LOAD_DISTANCE_KEY, DetailLoadDistance);
		TransparenceVisualEffect.SetInt(DETAIL_LOAD_DISTANCE_KEY, DetailLoadDistance);
	}

	private void LoadVoxelDataAroundCamera(int chunkX, int chunkY, int chunkZ)
	{
		if (mThread != null && mThread.IsAlive)
		{
			mThread.Abort();
		}

		mThread = new Thread(() => DoMainThreadWork(chunkX, chunkY, chunkZ));
		mThread.Start();
	}

	private void DoMainThreadWork(int chunkX, int chunkY, int chunkZ)
	{
		mList.Clear();
		mOpaqueList.Clear();
		mTransparencyList.Clear();

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
						mList.AddRange(mCustomSchematic.RegionDict[chunkIndexAt].BlockDict.Values);
					}
				}
			}
		}

		if (mList.Count > 0)
		{
			mOpaqueList.AddRange(mList.Where(v => !v.IsTransparent(mMaterials)));
			mTransparencyList.AddRange(mList.Where(v => v.IsTransparent(mMaterials)));

			UnityMainThreadManager.Instance.Enqueue(() => ChunkDataLoaded?.Invoke());
		}
		else
		{
			Debug.Log("[RuntimeVoxController] List is empty, abort");
		}
	}

	private void OnChunkDataLoaded()
	{
		mOpaqueBuffer?.Release();
		mTransparencyBuffer?.Release();

		if (!DebugVisualEffects)
		{
			OpaqueVisualEffect.visualEffectAsset = GetVisualEffectAsset(mOpaqueList.Count, OpaqueVisualEffects);
			TransparenceVisualEffect.visualEffectAsset = GetVisualEffectAsset(mTransparencyList.Count, TransparenceVisualEffects);
		}

		if (mOpaqueList.Count > 0)
		{
			mOpaqueBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mOpaqueList.Count, Marshal.SizeOf(typeof(VoxelVFX)));
			mOpaqueBuffer.SetData(mOpaqueList);

			OpaqueVisualEffect.SetInt("InitialBurstCount", mOpaqueList.Count);
			OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER_KEY, mOpaqueBuffer);
			OpaqueVisualEffect.Play();
		}

		if (mTransparencyList.Count > 0)
		{
			mTransparencyBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mTransparencyList.Count, Marshal.SizeOf(typeof(VoxelVFX)));
			mTransparencyBuffer.SetData(mTransparencyList);

			TransparenceVisualEffect.SetInt("InitialBurstCount", mTransparencyList.Count);
			TransparenceVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER_KEY, mTransparencyBuffer);
			TransparenceVisualEffect.Play();
		}
	}
	private VisualEffectAsset GetVisualEffectAsset(int voxels, List<VisualEffectAsset> assets)
	{
		int index = voxels / STEP_CAPACITY;
		if (index > assets.Count)
		{
			index = assets.Count - 1;
		}

		return assets[index];
	}
	#endregion
}

