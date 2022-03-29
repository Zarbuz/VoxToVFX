using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
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
		[Range(0.1f, 1f)]
		public float VoxelScale = 1;
		#endregion

		#region ConstStatic

		private const string VFX_BUFFER_KEY = "Buffer";

		private const string MATERIAL_VFX_BUFFER_KEY = "MaterialBuffer";
		private const string DEBUG_LOD_KEY = "DebugLod";
		private const string INITIAL_BURST_COUNT_KEY = "InitialBurstCount";
		#endregion

		#region Fields

		public event Action LoadFinishedCallback;

		private NativeList<VoxelVFX> mVFXBuffers;
		//private readonly Dictionary<int, VisualEffectItem> mVisualEffectItems = new Dictionary<int, VisualEffectItem>();
		private VisualEffectItem mVisualEffectItem;
		private Chunk[] mChunks;
		private GraphicsBuffer mPaletteBuffer;
		private GraphicsBuffer mGraphicsBuffer;

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
			if (!Application.isPlaying)
			{
				return;
			}

			Gizmos.color = Color.blue;
			Vector3 position = mMainCamera.transform.position;
			foreach (Chunk item in mChunks.GroupBy(t => t.ChunkIndex).First())
			{
				Gizmos.DrawLine(position, item.Position);
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
			mGraphicsBuffer?.Release();
			mPaletteBuffer?.Release();
			mPaletteBuffer = null;
			mGraphicsBuffer = null;
			if (mVisualEffectItem != null)
			{
				Destroy(mVisualEffectItem.gameObject);
			}
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

		public void SetChunkArrayData(Chunk[] chunks)
		{
			mChunks = chunks;
		}

		public void SetVoxelChunk(VoxelResult voxelResult)
		{
			if (voxelResult.Data.Length == 0)
			{
				return;
			}

			bool isVfxItemCreated = mVisualEffectItem != null;
			if (!isVfxItemCreated)
			{
				mVisualEffectItem = Instantiate(VisualEffectItemPrefab, mVisualItemsParent, false);
				mVisualEffectItem.transform.SetParent(mVisualItemsParent);
				mVisualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER_KEY, mPaletteBuffer);
				mVisualEffectItem.OpaqueVisualEffect.enabled = true;
			}

			if (!mVFXBuffers.IsCreated)
			{
				mVFXBuffers = new NativeList<VoxelVFX>(voxelResult.Data.Length, Allocator.Persistent);
			}
			else
			{
				mVFXBuffers.SetCapacity(mVFXBuffers.Length + voxelResult.Data.Length);
			}

			foreach (Vector4 vector4 in voxelResult.Data)
			{
				mVFXBuffers.AddNoResize(new VoxelVFX()
				{
					lodLevel = voxelResult.LodLevel,
					paletteIndex = (int)vector4.w,
					position = vector4
				});
			}
		}

		public void OnChunkLoadedFinished()
		{
			//foreach (VisualEffectItem item in mVisualEffectItems.Values)
			//{
			//	item.OpaqueVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER_KEY, mPaletteBuffer);
			//	//item.OpaqueVisualEffect.enabled = true;
			//}

			Debug.Log("[RuntimeVoxController] OnChunkLoadedFinished");
			mMainCamera.position = new Vector3(1000, 1000, 1000);
			mIsLoaded = true;
			LoadFinishedCallback?.Invoke();
		}

		#endregion

		#region PrivateMethods

		private void RefreshDebugLod()
		{
			mVisualEffectItem.OpaqueVisualEffect.Reinit();
			mVisualEffectItem.OpaqueVisualEffect.SetBool(DEBUG_LOD_KEY, DebugLod);
			mVisualEffectItem.OpaqueVisualEffect.Play();
		}

		private void RefreshLodsDistance()
		{
			int count = 0;
			foreach (Chunk chunk in mChunks)
			{
				float distance = Vector3.Distance(mMainCamera.transform.position, chunk.Position * VoxelScale);
				if ((distance >= LodDistance.x && distance < LodDistance.y && ForcedLevelLod == -1 || ForcedLevelLod == 0) && chunk.LodLevel == 1)
				{
					count++;
					VoxelDataCreatorManager.Instance.ReadChunkDataFile(chunk.Filename);
				}
				else if ((distance >= LodDistance.y && distance < LodDistance.z && ForcedLevelLod == -1 || ForcedLevelLod == 1) && chunk.LodLevel == 2)
				{
					count++;
					VoxelDataCreatorManager.Instance.ReadChunkDataFile(chunk.Filename);
				}
				else if ((distance >= LodDistance.z && distance < LodDistance.w && ForcedLevelLod == -1 || ForcedLevelLod == 2) && chunk.LodLevel == 4)
				{
					count++;
					VoxelDataCreatorManager.Instance.ReadChunkDataFile(chunk.Filename);
				}
				else if ((distance >= LodDistance.w && distance < int.MaxValue && ForcedLevelLod == -1 || ForcedLevelLod == 3) && chunk.LodLevel == 8)
				{
					count++;
					VoxelDataCreatorManager.Instance.ReadChunkDataFile(chunk.Filename);
				}
			}

			if (count > 0)
			{
				mVisualEffectItem.OpaqueVisualEffect.Reinit();

				GraphicsBuffer bufferLod = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mVFXBuffers.Length, Marshal.SizeOf(typeof(VoxelVFX)));
				bufferLod.SetData(mVFXBuffers.AsArray());

				mVisualEffectItem.OpaqueVisualEffect.SetInt(INITIAL_BURST_COUNT_KEY, mVFXBuffers.Length);
				mVisualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(VFX_BUFFER_KEY, bufferLod);
				mVisualEffectItem.OpaqueVisualEffect.Play();

				mGraphicsBuffer = bufferLod;
				mVFXBuffers.Dispose();
			}
			mCheckDistance = false;
		}

		private void RefreshVoxelScale()
		{
			//foreach (VisualEffectItem item in mVisualEffectItems.Values)
			//{
			//	item.OpaqueVisualEffect.Reinit();
			//	item.OpaqueVisualEffect.SetFloat(VOXEL_SCALE_KEY, VoxelScale);
			//	item.OpaqueVisualEffect.Play();
			//}
		}

		#endregion
	}
}

