using FileToVoxCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.VFX;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;
using VoxToVFXFramework.Scripts.ScriptableObjects;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class RuntimeVoxManager : ModuleSingleton<RuntimeVoxManager>
	{
		#region SerializeFields

		[Header("Visual Effects")]
		[SerializeField] private VisualEffect OpaqueVisualEffect;
		[SerializeField] private VisualEffect TransparenceVisualEffect;

		[Header("Camera Settings")]
		[SerializeField] private Transform MainCamera;

		[Header("Debug Settings")]
		[SerializeField] private bool DebugVisualEffects;

		[Header("VisualEffectAssets")]
		[SerializeField] private VisualEffectConfig Config;
		#endregion

		#region ConstStatic


		private const string MAIN_VFX_BUFFER_KEY = "Buffer";
		private const string MATERIAL_VFX_BUFFER_KEY = "MaterialBuffer";
		private const string ROTATION_VFX_BUFFER_KEY = "RotationBuffer";
		private const string DETAIL_LOAD_DISTANCE_KEY = "DetailLoadDistance";

		#endregion

		#region Fields

		public event Action<float> LoadProgressCallback;
		public event Action LoadFinishedCallback;

		public int ChunkLoadDistance { get; set; } = 10;
		public int DetailLoadDistance { get; set; } = 140;
		public bool ActiveTransparency { get; set; } = true;

		private GraphicsBuffer mOpaqueBuffer;
		private GraphicsBuffer mTransparencyBuffer;
		private GraphicsBuffer mPaletteBuffer;
		private GraphicsBuffer mRotationBuffer;

		private CustomSchematic mCustomSchematic;
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

		protected override void OnStart()
		{
			ChunkDataLoaded += OnChunkDataLoaded;
			VoxImporter voxImporter = new VoxImporter();
			CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.Loading);
			StartCoroutine(voxImporter.LoadVoxModelAsync(Path.Combine(Application.streamingAssetsPath, "default.vox"), OnLoadProgress, OnLoadFinished));
		}

		private void OnDestroy()
		{
			ChunkDataLoaded -= OnChunkDataLoaded;
			Release();
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

		public void SetChunkLoadDistance(int distance)
		{
			ChunkLoadDistance = distance;
			OnChunkLoadDistanceValueChanged();
		}

		public void SetActiveTransparency(bool active)
		{
			ActiveTransparency = active;
			OnChunkLoadDistanceValueChanged();
		}

		#endregion

		#region PrivateMethods

		private void Release()
		{
			mOpaqueBuffer?.Release();
			mTransparencyBuffer?.Release();
			mPaletteBuffer?.Release();
			mRotationBuffer?.Release();

			mOpaqueBuffer = null;
			mTransparencyBuffer = null;
			mPaletteBuffer = null;
			mRotationBuffer = null;

			mCustomSchematic = null;
		}


		private void OnLoadProgress(float progress)
		{
			LoadProgressCallback?.Invoke(progress);
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
			Debug.Log("[RuntimeVoxController] OnUpdateRotationFinished");

			OpaqueVisualEffect.enabled = true;
			TransparenceVisualEffect.enabled = true;
			mCustomSchematic = voxelData.CustomSchematic;
			LoadFinishedCallback?.Invoke();
		}

		private void OnChunkLoadDistanceValueChanged()
		{
			mPreviousPosition = MainCamera.position;
			FastMath.FloorToInt(mPreviousPosition.x / CustomSchematic.CHUNK_SIZE, mPreviousPosition.y / CustomSchematic.CHUNK_SIZE, mPreviousPosition.z / CustomSchematic.CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);
			LoadVoxelDataAroundCamera(chunkX, chunkY, chunkZ);
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
				if (ActiveTransparency)
				{
					mOpaqueList.AddRange(mList.Where(v => !v.IsTransparent(mMaterials)));
					mTransparencyList.AddRange(mList.Where(v => v.IsTransparent(mMaterials)));
				}
				else
				{
					mOpaqueList.AddRange(mList);
				}

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
				OpaqueVisualEffect.visualEffectAsset = GetVisualEffectAsset(mOpaqueList.Count, Config.OpaqueVisualEffects);
				TransparenceVisualEffect.visualEffectAsset = GetVisualEffectAsset(mTransparencyList.Count, Config.TransparenceVisualEffects);
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
			int index = voxels / Config.StepCapacity;
			if (index > assets.Count)
			{
				index = assets.Count - 1;
			}

			return assets[index];
		}
		#endregion

		
	}
}

