using FileToVoxCore.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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

		[SerializeField] private VisualEffectDataConfig VisualEffectDataConfig;

		#endregion

		#region ConstStatic


		private const string MAIN_VFX_BUFFER0_KEY = "Buffer";
		private const string MAIN_VFX_BUFFER1_KEY = "Buffer2";
		private const string MAIN_VFX_BUFFER2_KEY = "Buffer3";
		private const string MATERIAL_VFX_BUFFER_KEY = "MaterialBuffer";
		private const string SIZE_VFX_KEY = "Size";

		#endregion

		#region Fields

		public event Action<float> LoadProgressCallback;
		public event Action LoadFinishedCallback;

		public int DetailLoadDistance { get; set; } = 140;
		public int CutOfMargin { get; set; } = 200;

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
			StartCoroutine(VoxImporter.LoadVoxModelAsync(Path.Combine(Application.streamingAssetsPath, "default2.vox"),
				OnLoadProgress, OnFrameLoaded, OnLoadFinished));
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

				CheckDistance();
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.blue;
			foreach (VisualEffectItem item in mVisualEffectItems)
			{
				Gizmos.DrawLine(MainCamera.transform.position, item.FramePosition);
			}
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

		private void OnLoadProgress(float progress)
		{
			LoadProgressCallback?.Invoke(progress);
		}

		private void OnFrameLoaded(VoxelResult voxelResult)
		{
			if (voxelResult.DataLod0.Length == 0)
			{
				return;
			}

			VisualEffectItem visualEffectItem = Instantiate(VisualEffectItemPrefab, mVisualItemsParent, false);
			visualEffectItem.FramePosition = voxelResult.FrameWorldPosition;
			visualEffectItem.transform.SetParent(mVisualItemsParent);
			mVisualEffectItems.Add(visualEffectItem);

			GraphicsBuffer bufferLod0 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxelResult.DataLod0.Length, Marshal.SizeOf(typeof(Vector4)));
			bufferLod0.SetData(voxelResult.DataLod0.AsArray());
			mOpaqueBuffers.Add(bufferLod0);

			visualEffectItem.InitialBurstLod0 = voxelResult.DataLod0.Length;
			visualEffectItem.OpaqueVisualEffect.visualEffectAsset = GetVisualEffectAsset(voxelResult.DataLod0.Length, Config.OpaqueVisualEffects);
			visualEffectItem.OpaqueVisualEffect.SetInt("InitialBurstCount", voxelResult.DataLod0.Length); //TODO: Move it in Update method
			visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER0_KEY, bufferLod0);

			if (voxelResult.DataLod1.Length != 0)
			{
				GraphicsBuffer bufferLod1 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxelResult.DataLod1.Length, Marshal.SizeOf(typeof(Vector4)));
				bufferLod1.SetData(voxelResult.DataLod1.AsArray());
				mOpaqueBuffers.Add(bufferLod1);
				visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER1_KEY, bufferLod1);
				visualEffectItem.InitialBurstLod1 = voxelResult.DataLod1.Length;
			}

			if (voxelResult.DataLod2.Length != 0)
			{
				GraphicsBuffer bufferLod2 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxelResult.DataLod2.Length, Marshal.SizeOf(typeof(Vector4)));
				bufferLod2.SetData(voxelResult.DataLod2.AsArray());
				mOpaqueBuffers.Add(bufferLod2);
				visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER2_KEY, bufferLod2);
				visualEffectItem.InitialBurstLod2 = voxelResult.DataLod2.Length;
			}
		}

		private void OnLoadFinished(bool success)
		{
			if (!success)
			{
				Debug.LogError("[RuntimeVoxManager] Failed to load vox model");
				return;
			}

			mPaletteBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, VoxImporter.Materials.Length, Marshal.SizeOf(typeof(VoxelMaterialVFX)));
			mPaletteBuffer.SetData(VoxImporter.Materials);

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

		private void CheckDistance()
		{

			foreach (VisualEffectItem item in mVisualEffectItems)
			{
				float distance = Vector3.Distance(MainCamera.transform.position, item.FramePosition);
				Debug.Log(distance);
				bool updated = false;
				if (distance >= 0 && distance < 300 && item.InitialBurstLod0 != 0 && item.CurrentLod != 1)
				{
					item.OpaqueVisualEffect.Reinit();
					item.OpaqueVisualEffect.SetInt("InitialBurstCount", item.InitialBurstLod0);
					item.OpaqueVisualEffect.SetInt(SIZE_VFX_KEY, 1);
					item.CurrentLod = 1;
					updated = true;
				}
				else if (distance >= 300 && distance < 600 && item.InitialBurstLod1 != 0 && item.CurrentLod != 2)
				{
					item.OpaqueVisualEffect.Reinit();
					item.OpaqueVisualEffect.SetInt("InitialBurstCount", item.InitialBurstLod1);
					item.OpaqueVisualEffect.SetInt(SIZE_VFX_KEY, 2);
					item.CurrentLod = 2;
					updated = true;
				}
				else if (distance >= 600 && distance < int.MaxValue && item.InitialBurstLod2 != 0 && item.CurrentLod != 4)
				{
					item.OpaqueVisualEffect.Reinit();
					item.OpaqueVisualEffect.SetInt("InitialBurstCount", item.InitialBurstLod2);
					item.OpaqueVisualEffect.SetInt(SIZE_VFX_KEY, 4);
					item.CurrentLod = 4;
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
			if (index > assets.Count)
			{
				index = assets.Count - 1;
			}

			return assets[index];
		}

		#endregion
	}
}

