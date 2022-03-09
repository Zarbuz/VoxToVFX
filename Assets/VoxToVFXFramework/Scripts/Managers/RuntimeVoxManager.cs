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

		[Header("Lods")]
		[OnValueChanged(nameof(RefreshDebugLod))]
		public bool DebugLod;
		
		[OnValueChanged(nameof(RefreshLodsDistance))]
		public Vector4 LodDistance;

		[Range(-1, 3)]
		[OnValueChanged(nameof(RefreshLodsDistance))]
		public int ForcedLevelLod;

		[OnValueChanged(nameof(RefreshLodsDistance))]
		[Range(0.1f,1f)]
		public float VoxelScale = 1;
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
		private const string VOXEL_SCALE_KEY = "VoxelScale";
		#endregion

		#region Fields

		public event Action LoadFinishedCallback;

		private readonly List<GraphicsBuffer> mOpaqueBuffers = new List<GraphicsBuffer>();
		private readonly Dictionary<int, VisualEffectItem> mVisualEffectItems = new Dictionary<int, VisualEffectItem>();

		private GraphicsBuffer mPaletteBuffer;

		private bool mIsLoaded;
		private Vector3 mCurrentCameraPosition;
		private bool mCheckDistance;
		private Transform mVisualItemsParent;
		private WorldData mWorldData;
		private Transform mMainCamera;
		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			mMainCamera = UnityEngine.Camera.main.transform;
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

			if (Vector3.Distance(mCurrentCameraPosition, mMainCamera.transform.position) > 10 * VoxelScale && !mCheckDistance)
			{
				mCurrentCameraPosition = mMainCamera.transform.position;
				mCheckDistance = true;

				RefreshLodsDistance();
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.blue;
			Vector3 position = mMainCamera.transform.position;
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

		#region PublicMethods

		public void Release()
		{
			foreach (GraphicsBuffer buffer in mOpaqueBuffers)
			{
				buffer.Release();
			}

			mPaletteBuffer?.Release();
			mOpaqueBuffers.Clear();
			mPaletteBuffer = null;

			foreach (VisualEffectItem visualEffectItem in mVisualEffectItems.Values)
			{
				if (visualEffectItem.gameObject != null)
				{
					Destroy(visualEffectItem.gameObject);
				}
			}

			mVisualEffectItems.Clear();
		}


		public void SetForceLODValue(int value)
		{
			ForcedLevelLod = value;
			RefreshLodsDistance();
		}

		public void SetVoxelScaleValue(float value)
		{
			VoxelScale = value;
			RefreshVoxelScale();
			RefreshLodsDistance();
		}

		public void SetDebugLodValue(bool value)
		{
			DebugLod = value;
			RefreshDebugLod();
		}

		public void SetMaterials(VoxelMaterialVFX[] materials)
		{
			mPaletteBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, materials.Length, Marshal.SizeOf(typeof(VoxelMaterialVFX)));
			mPaletteBuffer.SetData(materials);
		}

		public void SetVoxelChunk(VoxelResult voxelResult)
		{
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
				visualEffectItem.OpaqueVisualEffect.SetFloat(VOXEL_SCALE_KEY, VoxelScale);
				visualEffectItem.OpaqueVisualEffect.enabled = true;
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

		public void OnChunkLoadedFinished()
		{
			foreach (VisualEffectItem item in mVisualEffectItems.Values)
			{
				item.OpaqueVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER_KEY, mPaletteBuffer);
				//item.OpaqueVisualEffect.enabled = true;
			}

			Debug.Log("[RuntimeVoxController] OnChunkLoadedFinished");
			mMainCamera.position = new Vector3(1000, 1000, 1000);
			mIsLoaded = true;
			LoadFinishedCallback?.Invoke();
		}

		#endregion

		#region PrivateMethods

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
				float distance = Vector3.Distance(mMainCamera.transform.position, item.FramePosition * VoxelScale);
				bool updated = false;
				if ((distance >= LodDistance.x && distance < LodDistance.y && ForcedLevelLod == -1 || ForcedLevelLod == 0) && item.InitialBurstLod0 != 0 && item.CurrentLod != 1)
				{
					item.OpaqueVisualEffect.Reinit();
					item.OpaqueVisualEffect.SetInt("InitialBurstCount", item.InitialBurstLod0);
					item.OpaqueVisualEffect.SetInt(SIZE_VFX_KEY, 1);
					item.CurrentLod = 1;
					updated = true;
				}
				else if ((distance >= LodDistance.y && distance < LodDistance.z && ForcedLevelLod == -1 || ForcedLevelLod == 1) && item.InitialBurstLod1 != 0 && item.CurrentLod != 2)
				{
					item.OpaqueVisualEffect.Reinit();
					item.OpaqueVisualEffect.SetInt("InitialBurstCount", item.InitialBurstLod1);
					item.OpaqueVisualEffect.SetInt(SIZE_VFX_KEY, 2);
					item.CurrentLod = 2;
					updated = true;
				}
				else if ((distance >= LodDistance.z && distance < LodDistance.w && ForcedLevelLod == -1 || ForcedLevelLod == 2) && item.InitialBurstLod2 != 0 && item.CurrentLod != 4)
				{
					item.OpaqueVisualEffect.Reinit();
					item.OpaqueVisualEffect.SetInt("InitialBurstCount", item.InitialBurstLod2);
					item.OpaqueVisualEffect.SetInt(SIZE_VFX_KEY, 4);
					item.CurrentLod = 4;
					updated = true;
				}
				else if ((distance >= LodDistance.w && distance < int.MaxValue && ForcedLevelLod == -1 || ForcedLevelLod == 3) && item.InitialBurstLod3 != 0 && item.CurrentLod != 8)
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

		private void RefreshVoxelScale()
		{
			foreach (VisualEffectItem item in mVisualEffectItems.Values)
			{
				item.OpaqueVisualEffect.Reinit();
				item.OpaqueVisualEffect.SetFloat(VOXEL_SCALE_KEY, VoxelScale);
				item.OpaqueVisualEffect.Play();
			}
		}

		#endregion
	}
}

