using FileToVoxCore.Utils;
using System;
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

		[SerializeField] private HDAdditionalLightData DirectionalLight;


		[Header("VisualEffectAssets")]
		[SerializeField] private VisualEffectConfig Config;
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

		private GraphicsBuffer mOpaqueBuffer;
		private GraphicsBuffer mTransparencyBuffer;
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
			VoxImporter voxImporter = new VoxImporter();
			CanvasPlayerPCManager.Instance.SetCanvasPlayerState(CanvasPlayerPCState.Loading);
			StartCoroutine(voxImporter.LoadVoxModelAsync(Path.Combine(Application.streamingAssetsPath, "default.vox"), OnLoadProgress, OnLoadFinished));
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
			mOpaqueBuffer?.Release();
			mTransparencyBuffer?.Release();
			mPaletteBuffer?.Release();
			mRotationBuffer?.Release();

			mOpaqueBuffer = null;
			mTransparencyBuffer = null;
			mPaletteBuffer = null;
			mRotationBuffer = null;

			//mCustomSchematic = null;
		}


		private void OnLoadProgress(float progress)
		{
			LoadProgressCallback?.Invoke(progress);
		}

		private void OnLoadFinished(VoxelDataVFX voxelData)
		{
			List<VoxelVFX> voxels = voxelData.CustomSchematic.UpdateRotations();
			int count = voxels.Count;

			Debug.Log("[RuntimeVoxController] OnLoadFinished: " + count);
			int targetPositionX = voxelData.CustomSchematic.Width / 2;
			int targetPositionY = voxelData.CustomSchematic.Height / 2;
			int targetPositionZ = voxelData.CustomSchematic.Length / 2;
			MainCamera.position = new Vector3(targetPositionX, targetPositionY, targetPositionZ);

			List<VoxelVFX> opaqueList = new List<VoxelVFX>();
			List<VoxelVFX> transparencyList = new List<VoxelVFX>();
			opaqueList.AddRange(voxels.Where(v => !v.IsTransparent(voxelData.Materials)));
			transparencyList.AddRange(voxels.Where(v => v.IsTransparent(voxelData.Materials)));

			OpaqueVisualEffect.visualEffectAsset = GetVisualEffectAsset(opaqueList.Count, Config.OpaqueVisualEffects);
			TransparenceVisualEffect.visualEffectAsset = GetVisualEffectAsset(transparencyList.Count, Config.TransparenceVisualEffects);

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

			

			if (opaqueList.Count > 0)
			{
				mOpaqueBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, opaqueList.Count, Marshal.SizeOf(typeof(VoxelVFX)));
				mOpaqueBuffer.SetData(opaqueList);

				OpaqueVisualEffect.SetInt("InitialBurstCount", opaqueList.Count);
				OpaqueVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER_KEY, mOpaqueBuffer);
				OpaqueVisualEffect.Play();
			}

			if (transparencyList.Count > 0)
			{
				mTransparencyBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, transparencyList.Count, Marshal.SizeOf(typeof(VoxelVFX)));
				mTransparencyBuffer.SetData(transparencyList);

				TransparenceVisualEffect.SetInt("InitialBurstCount", transparencyList.Count);
				TransparenceVisualEffect.SetGraphicsBuffer(MAIN_VFX_BUFFER_KEY, mTransparencyBuffer);
				TransparenceVisualEffect.Play();
			}

			OpaqueVisualEffect.enabled = true;
			TransparenceVisualEffect.enabled = true;
			mIsLoaded = true;
			LoadFinishedCallback?.Invoke();
		}

		private void UpdateDetailLoadDistance()
		{
			OpaqueVisualEffect.SetInt(DETAIL_LOAD_DISTANCE_KEY, DetailLoadDistance);
			TransparenceVisualEffect.SetInt(DETAIL_LOAD_DISTANCE_KEY, DetailLoadDistance);
		}

		private void UpdateCutOfMargin()
		{
			OpaqueVisualEffect.SetInt(CUT_OF_MARGIN_KEY, CutOfMargin);
			TransparenceVisualEffect.SetInt(CUT_OF_MARGIN_KEY, CutOfMargin);
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

