using FileToVoxCore.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
		[SerializeField] private Transform MainCamera;

		[SerializeField] private HDAdditionalLightData DirectionalLight;


		[Header("VisualEffectAssets")]
		[SerializeField] private VisualEffectConfig Config;

		[SerializeField] private VisualEffectDataConfig VisualEffectDataConfig;
		#endregion

		#region ConstStatic


		private const string MAIN_VFX_BUFFER_KEY = "Buffer";
		private const string MATERIAL_VFX_BUFFER_KEY = "MaterialBuffer";
		private const string ROTATION_VFX_BUFFER_KEY = "RotationBuffer";
		private const string DETAIL_LOAD_DISTANCE_KEY = "DetailLoadDistance";
		private const string CUT_OF_MARGIN_KEY = "CutOfMargin";

		#endregion

		#region Fields

		public event Action<float> LoadProgressCallback;
		public event Action LoadFinishedCallback;

		public int DetailLoadDistance { get; set; } = 140;
		public int CutOfMargin { get; set; } = 200;

		private readonly List<GraphicsBuffer> mOpaqueBuffers = new List<GraphicsBuffer>();
		private readonly List<GraphicsBuffer> mTransparencyBuffers = new List<GraphicsBuffer>();
		private readonly List<VisualEffectItem> mVisualEffectItems = new List<VisualEffectItem>();
		private readonly List<VoxelVFX> mOpaqueList = new List<VoxelVFX>();
		private readonly List<VoxelVFX> mTransparencyList = new List<VoxelVFX>();

		private GraphicsBuffer mPaletteBuffer;
		private GraphicsBuffer mRotationBuffer;

		private bool mIsLoaded;
		private int mPreviousDetailLoadDistance;
		private int mPreviousCutOfMargin;

		private Vector3 mPreviousPosition;
		private Vector3 mPreviousAngle;

		//private CustomSchematic mCustomSchematic;

		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			//ChunkDataLoaded += OnChunkDataLoaded;
			DirectionalLight.shadowUpdateMode = ShadowUpdateMode.OnDemand;
			CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.Loading);
		}

		private void OnDestroy()
		{
			//ChunkDataLoaded -= OnChunkDataLoaded;
			Release();
		}

		private void Update()
		{
			if (!mIsLoaded)
			{
				return;
			}

			if (mPreviousDetailLoadDistance != DetailLoadDistance)
			{
				mPreviousDetailLoadDistance = DetailLoadDistance;
				UpdateDetailLoadDistance();
			}

			if (mPreviousCutOfMargin != CutOfMargin)
			{
				mPreviousCutOfMargin = CutOfMargin;
				UpdateCutOfMargin();
			}

			if (MainCamera.transform.position != mPreviousPosition || MainCamera.transform.eulerAngles != mPreviousAngle)
			{
				mPreviousPosition = MainCamera.transform.position;
				mPreviousAngle = MainCamera.transform.eulerAngles;
				DirectionalLight.RequestShadowMapRendering();
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

			foreach (GraphicsBuffer buffer in mTransparencyBuffers)
			{
				buffer.Release();
			}

			mPaletteBuffer?.Release();
			mRotationBuffer?.Release();

			mOpaqueBuffers.Clear();
			mTransparencyBuffers.Clear();
			mPaletteBuffer = null;
			mRotationBuffer = null;
			mVisualEffectItems.Clear();
			//mCustomSchematic = null;
		}


		private void OnLoadFinished(bool success)
		{
			if (!success)
			{
				Debug.LogError("[RuntimeVoxManager] Failed to load vox model");
				return;
			}

			GameObject vfxHolders = new GameObject("VisualItemsParent");
			VoxImporter.CustomSchematic.UpdateRotations();


			mPaletteBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, VoxImporter.Materials.Length, Marshal.SizeOf(typeof(VoxelMaterialVFX)));
			mPaletteBuffer.SetData(VoxImporter.Materials);

			List<VoxelRotationVFX> rotations = GetVoxelRotations();
			mRotationBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, rotations.Count, Marshal.SizeOf(typeof(VoxelRotationVFX)));
			mRotationBuffer.SetData(rotations);

			foreach (Region region in VoxImporter.CustomSchematic.RegionDict.Values)
			{
				mOpaqueList.Clear();
				mTransparencyList.Clear();

				mOpaqueList.AddRange(region.BlockDict.Values.Where(v => !v.IsTransparent(VoxImporter.Materials)));
				mTransparencyList.AddRange(region.BlockDict.Values.Where(v => v.IsTransparent(VoxImporter.Materials)));

				VisualEffectItem visualEffectItem = Instantiate(VisualEffectItemPrefab, vfxHolders.transform, false);
				visualEffectItem.transform.SetParent(vfxHolders.transform);
				mVisualEffectItems.Add(visualEffectItem);
				visualEffectItem.OpaqueVisualEffect.visualEffectAsset = GetVisualEffectAsset(mOpaqueList.Count, Config.OpaqueVisualEffects);
				visualEffectItem.TransparenceVisualEffect.visualEffectAsset = GetVisualEffectAsset(mTransparencyList.Count, Config.TransparenceVisualEffects);

				visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER_KEY, mPaletteBuffer);
				visualEffectItem.TransparenceVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER_KEY, mPaletteBuffer);

				visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(ROTATION_VFX_BUFFER_KEY, mRotationBuffer);
				visualEffectItem.TransparenceVisualEffect.SetGraphicsBuffer(ROTATION_VFX_BUFFER_KEY, mRotationBuffer);

				if (mOpaqueList.Count > 0)
				{
					GraphicsBuffer buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mOpaqueList.Count, Marshal.SizeOf(typeof(VoxelVFX)));
					buffer.SetData(mOpaqueList);
					mOpaqueBuffers.Add(buffer);
					visualEffectItem.OpaqueVisualEffect.SetInt("InitialBurstCount", mOpaqueList.Count);
					visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER_KEY, buffer);
					visualEffectItem.OpaqueVisualEffect.enabled = true;
					visualEffectItem.OpaqueVisualEffect.Play();
				}

				if (mTransparencyList.Count > 0)
				{
					GraphicsBuffer buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mTransparencyList.Count, Marshal.SizeOf(typeof(VoxelVFX)));
					buffer.SetData(mTransparencyList);
					mTransparencyBuffers.Add(buffer);
					visualEffectItem.TransparenceVisualEffect.SetInt("InitialBurstCount", mTransparencyList.Count);
					visualEffectItem.TransparenceVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER_KEY, buffer);
					visualEffectItem.TransparenceVisualEffect.enabled = true;
					visualEffectItem.TransparenceVisualEffect.Play();
				}
			}

			Debug.Log("[RuntimeVoxController] OnLoadFinished");
			int targetPositionX = VoxImporter.CustomSchematic.Width / 2;
			int targetPositionY = VoxImporter.CustomSchematic.Height / 2;
			int targetPositionZ = VoxImporter.CustomSchematic.Length / 2;
			MainCamera.position = new Vector3(targetPositionX, targetPositionY, targetPositionZ);

			
			VoxImporter.Clean();
			GC.Collect();
			
			mIsLoaded = true;
			LoadFinishedCallback?.Invoke();
		}

		private void UpdateDetailLoadDistance()
		{
			foreach (VisualEffectItem item in mVisualEffectItems)
			{
				item.OpaqueVisualEffect.SetInt(DETAIL_LOAD_DISTANCE_KEY, DetailLoadDistance);
				item.TransparenceVisualEffect.SetInt(DETAIL_LOAD_DISTANCE_KEY, DetailLoadDistance);
			}
		}

		private void UpdateCutOfMargin()
		{
			foreach (VisualEffectItem item in mVisualEffectItems)
			{
				item.OpaqueVisualEffect.SetInt(CUT_OF_MARGIN_KEY, CutOfMargin);
				item.TransparenceVisualEffect.SetInt(CUT_OF_MARGIN_KEY, CutOfMargin);
			}
		}

		private static List<VoxelRotationVFX> GetVoxelRotations()
		{
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

			return rotations;
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

