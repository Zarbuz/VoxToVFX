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

		[Header("Camera Settings")] [SerializeField]
		private Transform MainCamera;

		[SerializeField] private HDAdditionalLightData DirectionalLight;


		[Header("VisualEffectAssets")] [SerializeField]
		private VisualEffectConfig Config;

		[SerializeField] private VisualEffectDataConfig VisualEffectDataConfig;

		#endregion

		#region ConstStatic


		private const string MAIN_VFX_BUFFER_KEY = "Buffer";
		private const string MATERIAL_VFX_BUFFER_KEY = "MaterialBuffer";
		private const string ROTATION_VFX_BUFFER_KEY = "RotationBuffer";
		private const string ROTATION_INDEX_VFX_BUFFER_KEY = "IndexRotationBuffer";
		private const string DETAIL_LOAD_DISTANCE_KEY = "DetailLoadDistance";
		private const string CUT_OF_MARGIN_KEY = "CutOfMargin";

		#endregion

		#region Fields

		public event Action<float> LoadProgressCallback;
		public event Action LoadFinishedCallback;

		public int DetailLoadDistance { get; set; } = 140;
		public int CutOfMargin { get; set; } = 200;

		private readonly List<GraphicsBuffer> mOpaqueBuffers = new List<GraphicsBuffer>();
		private readonly List<VisualEffectItem> mVisualEffectItems = new List<VisualEffectItem>();

		private GraphicsBuffer mPaletteBuffer;
		private GraphicsBuffer mRotationBuffer;

		private bool mIsLoaded;
		private int mPreviousDetailLoadDistance;
		private int mPreviousCutOfMargin;

		private Vector3 mPreviousPosition;
		private Vector3 mPreviousAngle;

		private Transform mVisualItemsParent;
		//private CustomSchematic mCustomSchematic;

		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			//ChunkDataLoaded += OnChunkDataLoaded;
			DirectionalLight.shadowUpdateMode = ShadowUpdateMode.OnDemand;
			CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.Loading);
			StartCoroutine(VoxImporter.LoadVoxModelAsync(Path.Combine(Application.streamingAssetsPath, "default2.vox"),
				OnLoadProgress, OnFrameLoaded, OnLoadFinished));
			mVisualItemsParent = new GameObject("VisualItemsParent").transform;
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

			if (MainCamera.transform.position != mPreviousPosition ||
			    MainCamera.transform.eulerAngles != mPreviousAngle)
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

			mPaletteBuffer?.Release();
			mRotationBuffer?.Release();
			mOpaqueBuffers.Clear();
			mPaletteBuffer = null;
			mVisualEffectItems.Clear();
			//mCustomSchematic = null;
		}

		private void OnLoadProgress(float progress)
		{
			LoadProgressCallback?.Invoke(progress);
		}

		private void OnFrameLoaded(NativeArray<Vector4> frameArray, NativeArray<int> indexRotationArray)
		{
			if (frameArray.Length == 0)
			{
				return;
			}

			VisualEffectItem visualEffectItem = Instantiate(VisualEffectItemPrefab, mVisualItemsParent, false);
			visualEffectItem.transform.SetParent(mVisualItemsParent);
			mVisualEffectItems.Add(visualEffectItem);

			GraphicsBuffer buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, frameArray.Length, Marshal.SizeOf(typeof(VoxelVFX)));
			buffer.SetData(frameArray);
			mOpaqueBuffers.Add(buffer);

			GraphicsBuffer indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, indexRotationArray.Length, Marshal.SizeOf(typeof(int)));
			indexBuffer.SetData(indexRotationArray);
			mOpaqueBuffers.Add(indexBuffer);

			visualEffectItem.OpaqueVisualEffect.visualEffectAsset = GetVisualEffectAsset(frameArray.Length, Config.OpaqueVisualEffects);
			visualEffectItem.OpaqueVisualEffect.SetInt("InitialBurstCount", frameArray.Length);
			visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER_KEY, buffer);
			visualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(ROTATION_INDEX_VFX_BUFFER_KEY, indexBuffer);
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

			mRotationBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 4, Marshal.SizeOf(typeof(VoxelRotationVFX)));
			VoxelRotationVFX[] rotationArray = GetRotationArray();
			mRotationBuffer.SetData(rotationArray);
			foreach (VisualEffectItem item in mVisualEffectItems)
			{
				item.OpaqueVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER_KEY, mPaletteBuffer);
				item.OpaqueVisualEffect.SetGraphicsBuffer(ROTATION_VFX_BUFFER_KEY, mRotationBuffer);
				item.OpaqueVisualEffect.enabled = true;
				item.OpaqueVisualEffect.Play();
			}

			//rotationArray.Dispose();
			Debug.Log("[RuntimeVoxController] OnLoadFinished");
			//int targetPositionX = VoxImporter.CustomSchematic.Width / 2;
			//int targetPositionY = VoxImporter.CustomSchematic.Height / 2;
			//int targetPositionZ = VoxImporter.CustomSchematic.Length / 2;
			//MainCamera.position = new Vector3(targetPositionX, targetPositionY, targetPositionZ);
			MainCamera.position = new Vector3(1000, 1000, 1000);


			mIsLoaded = true;
			LoadFinishedCallback?.Invoke();
		}

		private void UpdateDetailLoadDistance()
		{
			foreach (VisualEffectItem item in mVisualEffectItems)
			{
				item.OpaqueVisualEffect.SetInt(DETAIL_LOAD_DISTANCE_KEY, DetailLoadDistance);
			}
		}

		private void UpdateCutOfMargin()
		{
			foreach (VisualEffectItem item in mVisualEffectItems)
			{
				item.OpaqueVisualEffect.SetInt(CUT_OF_MARGIN_KEY, CutOfMargin);
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


		private static VoxelRotationVFX[] GetRotationArray()
		{
			var rotationArray = new VoxelRotationVFX[4];
			rotationArray[0] = new VoxelRotationVFX()
			{
				pivot = Vector3.zero,
				rotation = Vector3.zero
			};

			rotationArray[1] = new VoxelRotationVFX()
			{
				pivot = new Vector3(0, 0, 0.5f),
				rotation = new Vector3(90, 0, 0)
			};

			rotationArray[2] = new VoxelRotationVFX()
			{
				pivot = new Vector3(0, 0, 0.5f),
				rotation = new Vector3(0, 180, 0)
			};

			rotationArray[3] = new VoxelRotationVFX()
			{
				pivot = new Vector3(0, 0, 0.5f),
				rotation = new Vector3(0, 90, 0)
			};

			return rotationArray;
		}

		#endregion
	}
}

