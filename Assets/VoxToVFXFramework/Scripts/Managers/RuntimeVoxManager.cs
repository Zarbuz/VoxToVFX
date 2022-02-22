using FileToVoxCore.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;
using VoxToVFXFramework.Scripts.ScriptableObjects;
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
		[SerializeField]
		private Transform MainCamera;

		[SerializeField] private HDAdditionalLightData DirectionalLight;

		[Header("VisualEffectAssets")]
		[SerializeField]
		private VisualEffectConfig Config;

		[Header("Lods")]
		[OnValueChanged(nameof(RefreshDebugLod))]
		[SerializeField] private bool DebugLod;
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
		#endregion

		#region Fields

		public event Action<int, float> LoadProgressCallback;
		public event Action LoadFinishedCallback;

		private readonly List<GraphicsBuffer> mOpaqueBuffers = new List<GraphicsBuffer>();
		private readonly List<VisualEffectItem> mVisualEffectItems = new List<VisualEffectItem>();

		private GraphicsBuffer mPaletteBuffer;

		private bool mIsLoaded;
		private Vector3 mCurrentCameraPosition;
		private bool mCheckDistance;
		private Transform mVisualItemsParent;

		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			DirectionalLight.shadowUpdateMode = ShadowUpdateMode.OnDemand;
			CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.Loading);
			StartCoroutine(VoxImporter.LoadVoxModelAsync(Path.Combine(Application.streamingAssetsPath, "default 3.vox"),
				OnLoadFrameProgress, OnLoadFinished));
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
			foreach (VisualEffectItem item in mVisualEffectItems)
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
			Debug.Log(progress);
			if (voxelResult.DataLod0.Length == 0)
			{
				return;
			}

			VisualEffectItem visualEffectItem = Instantiate(VisualEffectItemPrefab, mVisualItemsParent, false);
			visualEffectItem.FramePosition = voxelResult.FrameWorldPosition;
			visualEffectItem.transform.SetParent(mVisualItemsParent);
			mVisualEffectItems.Add(visualEffectItem);

			GraphicsBuffer bufferLod0 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxelResult.DataLod0.Length, Marshal.SizeOf(typeof(Vector4)));
			bufferLod0.SetData(voxelResult.DataLod0);
			mOpaqueBuffers.Add(bufferLod0);

			visualEffectItem.InitialBurstLod0 = voxelResult.DataLod0.Length;
			visualEffectItem.OpaqueVisualEffect.visualEffectAsset = GetVisualEffectAsset(voxelResult.DataLod0.Length, Config.OpaqueVisualEffects);
			visualEffectItem.OpaqueVisualEffect.SetInt("InitialBurstCount", voxelResult.DataLod0.Length); //TODO: Move it in Update method
			visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER1_KEY, bufferLod0);

			if (voxelResult.DataLod1.Length != 0)
			{
				GraphicsBuffer bufferLod1 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxelResult.DataLod1.Length, Marshal.SizeOf(typeof(Vector4)));
				bufferLod1.SetData(voxelResult.DataLod1);
				mOpaqueBuffers.Add(bufferLod1);
				visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER2_KEY, bufferLod1);
				visualEffectItem.InitialBurstLod1 = voxelResult.DataLod1.Length;
			}

			if (voxelResult.DataLod2.Length != 0)
			{
				GraphicsBuffer bufferLod2 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxelResult.DataLod2.Length, Marshal.SizeOf(typeof(Vector4)));
				bufferLod2.SetData(voxelResult.DataLod2);
				mOpaqueBuffers.Add(bufferLod2);
				visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER3_KEY, bufferLod2);
				visualEffectItem.InitialBurstLod2 = voxelResult.DataLod2.Length;
			}

			if (voxelResult.DataLod3.Length != 0)
			{
				GraphicsBuffer bufferLod3 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxelResult.DataLod3.Length, Marshal.SizeOf(typeof(Vector4)));
				bufferLod3.SetData(voxelResult.DataLod3);
				mOpaqueBuffers.Add(bufferLod3);
				visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER4_KEY, bufferLod3);
				visualEffectItem.InitialBurstLod3 = voxelResult.DataLod3.Length;
			}
		}

		private void OnLoadFinished(WorldData worldData)
		{
			if (worldData == null)
			{
				Debug.LogError("[RuntimeVoxManager] Failed to load vox model");
				return;
			}
			Debug.Log("OnLoadFinished");

			mPaletteBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, VoxImporter.Materials.Length, Marshal.SizeOf(typeof(VoxelMaterialVFX)));
			mPaletteBuffer.SetData(VoxImporter.Materials);
			worldData.ComputeLodsChunks(OnChunkLoadResult);

			foreach (VisualEffectItem item in mVisualEffectItems)
			{
				item.OpaqueVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER_KEY, mPaletteBuffer);
				item.OpaqueVisualEffect.enabled = true;
				//item.OpaqueVisualEffect.Play();
			}

			Debug.Log("[RuntimeVoxController] OnLoadFinished");
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
			foreach (VisualEffectItem item in mVisualEffectItems)
			{
				item.OpaqueVisualEffect.Reinit();
				item.OpaqueVisualEffect.SetBool(DEBUG_LOD_KEY, DebugLod);
				item.OpaqueVisualEffect.Play();
			}
		}

		private void RefreshLodsDistance()
		{
			foreach (VisualEffectItem item in mVisualEffectItems)
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

		private VisualEffectAsset GetVisualEffectAsset(int voxels, List<VisualEffectAsset> assets)
		{
			int index = voxels / Config.StepCapacity;
			if (index >= assets.Count)
			{
				index = assets.Count - 1;
			}

			return assets[index];
		}

		#endregion
	}
}

