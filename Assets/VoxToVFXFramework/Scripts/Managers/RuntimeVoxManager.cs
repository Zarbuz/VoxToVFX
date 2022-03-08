using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI;
using VoxToVFXFramework.Scripts.VFXItem;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class RuntimeVoxManager : ModuleSingleton<RuntimeVoxManager>
	{
		#region SerializeFields

		[SerializeField] private VisualEffectItem VisualEffectItemPrefab;

		[Header("Camera Settings")]
		[SerializeField] private Transform MainCamera;
		[SerializeField] private HDAdditionalLightData DirectionalLight;


		[Header("Lods")]
		[OnValueChanged(nameof(RefreshDebugLod))]
		[SerializeField] private bool DebugLod;
		
		[OnValueChanged(nameof(RefreshLodsDistance))]
		[SerializeField] private Vector4 LodDistance;

		[OnValueChanged(nameof(RefreshLodsDistance))]
		[SerializeField] private bool ForceLevelLod;

		[Range(0, 3)]
		[ShowIf(nameof(ForceLevelLod))]
		[OnValueChanged(nameof(RefreshLodsDistance))]
		[SerializeField] private int ForcedLevelLod;
		#endregion

		#region ConstStatic


		private const string MAIN_VFX_BUFFER1_KEY = "Buffer";
		private const string MAIN_VFX_BUFFER2_KEY = "Buffer2";
		private const string MAIN_VFX_BUFFER3_KEY = "Buffer3";
		private const string MAIN_VFX_BUFFER4_KEY = "Buffer4";
		private const string MATERIAL_VFX_BUFFER_KEY = "MaterialBuffer";
		private const string SIZE_VFX_KEY = "Size";
		private const string DEBUG_LOD_KEY = "DebugLod";
		private const string POSITION_CHUNK_KEY = "CenterPosition";
		private const string SIZE_CHUNK_KEY = "ChunkSize";
		#endregion

		#region Fields

		public event Action<int, float> LoadProgressCallback;
		public event Action LoadFinishedCallback;

		private readonly List<GraphicsBuffer> mOpaqueBuffers = new List<GraphicsBuffer>();
		private readonly Dictionary<int, VisualEffectItem> mVisualEffectItems = new Dictionary<int, VisualEffectItem>();

		private GraphicsBuffer mPaletteBuffer;

		private bool mIsLoaded;
		private Vector3 mCurrentCameraPosition;
		private bool mCheckDistance;
		private Transform mVisualItemsParent;
		private WorldData mWorldData;

		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			DirectionalLight.shadowUpdateMode = ShadowUpdateMode.OnDemand;
			CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.Loading);
			StartCoroutine(VoxImporter.LoadVoxModelAsync(Path.Combine(Application.streamingAssetsPath, "default 3.vox"),
				OnLoadFrameProgress, OnVoxLoadFinished));
			mVisualItemsParent = new GameObject("VisualItemsParent").transform;
		}

		private void OnDestroy()
		{
			Release();
		}

		private void Update()
		{
			if (!mIsLoaded)
			{
				return;
			}

			if (Vector3.Distance(mCurrentCameraPosition, MainCamera.transform.position) > 10 && !mCheckDistance)
			{
				mCurrentCameraPosition = MainCamera.transform.position;
				mCheckDistance = true;

				RefreshLodsDistance();
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.blue;
			Vector3 position = MainCamera.transform.position;
			foreach (VisualEffectItem item in mVisualEffectItems.Values)
			{
				Gizmos.DrawLine(position, item.FramePosition);
			}

			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(position, LodDistance.y);

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(position, LodDistance.z);

			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(position, LodDistance.w);
		}

		#endregion

		#region PrivateMethods

		private void Release()
		{
			foreach (GraphicsBuffer buffer in mOpaqueBuffers)
			{
				buffer.Release();
			}

			mPaletteBuffer?.Release();
			mOpaqueBuffers.Clear();
			mPaletteBuffer = null;
			mVisualEffectItems.Clear();
		}

		private void OnLoadFrameProgress(float progress)
		{
			LoadProgressCallback?.Invoke(1, progress);
		}

		private void OnChunkLoadResult(float progress, VoxelResult voxelResult)
		{
			LoadProgressCallback?.Invoke(2, progress);

			if (voxelResult.Data.Length == 0)
			{
				return;
			}

			VisualEffectItem visualEffectItem;
			if (!mVisualEffectItems.ContainsKey(voxelResult.ChunkIndex))
			{
				visualEffectItem = Instantiate(VisualEffectItemPrefab, mVisualItemsParent, false);
				visualEffectItem.FramePosition = voxelResult.FrameWorldPosition;
				visualEffectItem.transform.SetParent(mVisualItemsParent);
				visualEffectItem.ChunkIndex = voxelResult.ChunkIndex;
				visualEffectItem.OpaqueVisualEffect.SetVector3(POSITION_CHUNK_KEY, voxelResult.FrameWorldPosition);
				visualEffectItem.OpaqueVisualEffect.SetVector3(SIZE_CHUNK_KEY, new Vector3(WorldData.CHUNK_SIZE, WorldData.CHUNK_SIZE, WorldData.CHUNK_SIZE));
				mVisualEffectItems.Add(voxelResult.ChunkIndex, visualEffectItem);
			}
			else
			{
				visualEffectItem = mVisualEffectItems[voxelResult.ChunkIndex];
			}

			GraphicsBuffer bufferLod = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxelResult.Data.Length, Marshal.SizeOf(typeof(Vector4)));
			bufferLod.SetData(voxelResult.Data);
			mOpaqueBuffers.Add(bufferLod);

			switch (voxelResult.LodLevel)
			{
				case 1:
					visualEffectItem.OpaqueVisualEffect.SetInt("InitialBurstCount", voxelResult.Data.Length);
					visualEffectItem.InitialBurstLod0 = voxelResult.Data.Length;
					visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER1_KEY, bufferLod);
					break;
				case 2:
					visualEffectItem.InitialBurstLod1 = voxelResult.Data.Length;
					visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER2_KEY, bufferLod);
					break;
				case 3:
					visualEffectItem.InitialBurstLod2 = voxelResult.Data.Length;
					visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER3_KEY, bufferLod);
					break;
				case 4:
					visualEffectItem.InitialBurstLod3 = voxelResult.Data.Length;
					visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER4_KEY, bufferLod);
					break;
			}

		}

		private void OnVoxLoadFinished(WorldData worldData)
		{
			if (worldData == null)
			{
				Debug.LogError("[RuntimeVoxManager] Failed to load vox model");
				return;
			}
			Debug.Log("[RuntimeVoxController] OnVoxLoadFinished");
			mPaletteBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, VoxImporter.Materials.Length, Marshal.SizeOf(typeof(VoxelMaterialVFX)));
			mPaletteBuffer.SetData(VoxImporter.Materials);
			mWorldData = worldData;
			StartCoroutine(worldData.ComputeLodsChunks(OnChunkLoadResult, OnChunkLoadedFinished));
			
		}

		private void OnChunkLoadedFinished()
		{
			mWorldData.Dispose();
			foreach (VisualEffectItem item in mVisualEffectItems.Values)
			{
				item.OpaqueVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER_KEY, mPaletteBuffer);
				item.OpaqueVisualEffect.enabled = true;
				//item.OpaqueVisualEffect.Play();
			}

			Debug.Log("[RuntimeVoxController] OnChunkLoadedFinished");
			//int targetPositionX = VoxImporter.CustomSchematic.Width / 2;
			//int targetPositionY = VoxImporter.CustomSchematic.Height / 2;
			//int targetPositionZ = VoxImporter.CustomSchematic.Length / 2;
			//MainCamera.position = new Vector3(targetPositionX, targetPositionY, targetPositionZ);
			MainCamera.position = new Vector3(1000, 1000, 1000);


			mIsLoaded = true;
			LoadFinishedCallback?.Invoke();
		}

		private void RefreshDebugLod()
		{
			foreach (VisualEffectItem item in mVisualEffectItems.Values)
			{
				item.OpaqueVisualEffect.Reinit();
				item.OpaqueVisualEffect.SetBool(DEBUG_LOD_KEY, DebugLod);
				item.OpaqueVisualEffect.Play();
			}
		}

		private void RefreshLodsDistance()
		{
			foreach (VisualEffectItem item in mVisualEffectItems.Values)
			{
				float distance = Vector3.Distance(MainCamera.transform.position, item.FramePosition);
				bool updated = false;
				if ((distance >= LodDistance.x && distance < LodDistance.y && !ForceLevelLod || ForceLevelLod && ForcedLevelLod == 0) && item.InitialBurstLod0 != 0 && item.CurrentLod != 1)
				{
					item.OpaqueVisualEffect.Reinit();
					item.OpaqueVisualEffect.SetInt("InitialBurstCount", item.InitialBurstLod0);
					item.OpaqueVisualEffect.SetInt(SIZE_VFX_KEY, 1);
					item.CurrentLod = 1;
					updated = true;
				}
				else if ((distance >= LodDistance.y && distance < LodDistance.z && !ForceLevelLod || ForceLevelLod && ForcedLevelLod == 1) && item.InitialBurstLod1 != 0 && item.CurrentLod != 2)
				{
					item.OpaqueVisualEffect.Reinit();
					item.OpaqueVisualEffect.SetInt("InitialBurstCount", item.InitialBurstLod1);
					item.OpaqueVisualEffect.SetInt(SIZE_VFX_KEY, 2);
					item.CurrentLod = 2;
					updated = true;
				}
				else if ((distance >= LodDistance.z && distance < LodDistance.w && !ForceLevelLod || ForceLevelLod && ForcedLevelLod == 2) && item.InitialBurstLod2 != 0 && item.CurrentLod != 4)
				{
					item.OpaqueVisualEffect.Reinit();
					item.OpaqueVisualEffect.SetInt("InitialBurstCount", item.InitialBurstLod2);
					item.OpaqueVisualEffect.SetInt(SIZE_VFX_KEY, 4);
					item.CurrentLod = 4;
					updated = true;
				}
				else if ((distance >= LodDistance.w && distance < int.MaxValue && !ForceLevelLod || ForceLevelLod && ForcedLevelLod == 3) && item.InitialBurstLod3 != 0 && item.CurrentLod != 8)
				{
					item.OpaqueVisualEffect.Reinit();
					item.OpaqueVisualEffect.SetInt("InitialBurstCount", item.InitialBurstLod3);
					item.OpaqueVisualEffect.SetInt(SIZE_VFX_KEY, 8);
					item.CurrentLod = 8;
					updated = true;
				}

				if (updated)
				{
					item.OpaqueVisualEffect.Play();
				}
			}

			mCheckDistance = false;
		}

		

		#endregion
	}
}

