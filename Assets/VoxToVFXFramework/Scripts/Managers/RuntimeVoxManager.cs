using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;
using VoxToVFXFramework.Scripts.Jobs;
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

		public bool ShowOnlyActiveChunkGizmos;
		public float DistanceCheck;
		#endregion

		#region ConstStatic

		private const string VFX_BUFFER_KEY = "Buffer";

		private const string MATERIAL_VFX_BUFFER_KEY = "MaterialBuffer";
		private const string DEBUG_LOD_KEY = "DebugLod";
		private const string INITIAL_BURST_COUNT_KEY = "InitialBurstCount";
		#endregion

		#region Fields

		public event Action LoadFinishedCallback;
		public NativeArray<Chunk> Chunks;

		private NativeMultiHashMap<int, VoxelVFX> mChunksLoaded;
		private VisualEffectItem mVisualEffectItem;
		private GraphicsBuffer mPaletteBuffer;
		private GraphicsBuffer mGraphicsBuffer;
		private Plane[] mPlanes;

		private bool mIsLoaded;
		private bool mCheckDistance;
		private Transform mVisualItemsParent;
		private WorldData mWorldData;

		private UnityEngine.Camera mCamera;
		private Quaternion mPreviousCameraRotation;
		private Vector3 mPreviousCameraPosition;
		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			mCamera = UnityEngine.Camera.main;
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
			mPlanes = GeometryUtility.CalculateFrustumPlanes(mCamera);

			if (Vector3.Distance(mPreviousCameraPosition, mCamera.transform.position) > DistanceCheck && !mCheckDistance)
			{
				mCheckDistance = true;
				mPreviousCameraPosition = mCamera.transform.position;
				mPreviousCameraRotation = mCamera.transform.rotation;
				RefreshLodsDistance();
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			Vector3 position = mCamera.transform.position;
			if (ShowOnlyActiveChunkGizmos)
			{
				Gizmos.color = Color.green;

				foreach (var item in Chunks.Where(t => t.IsActive == 1).GroupBy(t => t.ChunkIndex, t => t.Position, (key, g) => new { ChunkIndex = key, Position = g.First() }))
				{
					Gizmos.DrawWireCube(item.Position, Vector3.one * WorldData.CHUNK_SIZE);
				}
			}
			else
			{
				Gizmos.color = Color.white;
				foreach (var item in Chunks.GroupBy(t => t.ChunkIndex, t => t.Position, (key, g) => new { ChunkIndex = key, Position = g.First() }))
				{
					Gizmos.DrawWireCube(item.Position, Vector3.one * WorldData.CHUNK_SIZE);
				}
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

			if (Chunks.IsCreated)
			{
				Chunks.Dispose();
			}

			if (mChunksLoaded.IsCreated)
			{
				mChunksLoaded.Dispose();
			}
		}


		public void SetForceLODValue(int value)
		{
			ForcedLevelLod = value;
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

		public void SetVoxelChunk(int chunkIndex, int lodLevel, NativeArray<VoxelVFX> array)
		{
			if (array.Length == 0)
			{
				return;
			}

			if (!mChunksLoaded.IsCreated)
			{
				mChunksLoaded = new NativeMultiHashMap<int, VoxelVFX>(256, Allocator.Persistent);
			}

			foreach (VoxelVFX voxel in array)
			{
				mChunksLoaded.Add(chunkIndex, voxel);
			}
		}

		public void OnChunkLoadedFinished()
		{
			mVisualEffectItem = Instantiate(VisualEffectItemPrefab, mVisualItemsParent, false);
			mVisualEffectItem.transform.SetParent(mVisualItemsParent);
			mVisualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER_KEY, mPaletteBuffer);
			mVisualEffectItem.OpaqueVisualEffect.enabled = true;

			Debug.Log("[RuntimeVoxController] OnChunkLoadedFinished");
			mCamera.transform.position = new Vector3(1000, 1000, 1000);
			mIsLoaded = true;
			LoadFinishedCallback?.Invoke();
		}

		public void SetChunks(Chunk[] chunks)
		{
			Chunks = new NativeArray<Chunk>(chunks.Length, Allocator.Persistent);
			for (int i = 0; i < chunks.Length; i++)
			{
				Chunks[i] = chunks[i];
			}
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
			if (!Chunks.IsCreated)
				return;

			NativeList<Chunk> activeChunks = new NativeList<Chunk>(Allocator.TempJob);
			for (int index = 0; index < Chunks.Length; index++)
			{
				Chunk chunk = Chunks[index];
				chunk.IsActive = GeometryUtility.TestPlanesAABB(mPlanes, new Bounds(Chunks[index].Position, Vector3.one * WorldData.CHUNK_SIZE)) ? 1 : 0;
				Chunks[index] = chunk;
				if (chunk.IsActive == 1)
				{
					activeChunks.Add(chunk);
				}
			}
			int totalActive = Chunks.Count(chunk => chunk.IsActive == 1);
			int totalLength = Chunks.Where(chunk => chunk.IsActive == 1).Sum(chunk => chunk.Length);
			NativeList<VoxelVFX> buffer = new NativeList<VoxelVFX>(totalLength, Allocator.TempJob);
			JobHandle computeRenderingChunkJob = new ComputeRenderingChunkJob()
			{
				LodDistance = LodDistance,
				ForcedLevelLod = ForcedLevelLod,
				CameraPosition = mCamera.transform.position,
				Data = mChunksLoaded,
				Chunks = activeChunks,
				Buffer = buffer.AsParallelWriter()
			}.Schedule(totalActive, 64);
			computeRenderingChunkJob.Complete();
			activeChunks.Dispose();

			if (buffer.Length > 0)
			{
				RefreshRender(buffer);
			}

			buffer.Dispose();
			mCheckDistance = false;
		}

		private void RefreshRender(NativeList<VoxelVFX> chunks)
		{
			mVisualEffectItem.OpaqueVisualEffect.Reinit();

			mGraphicsBuffer?.Release();
			mGraphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, chunks.Length, Marshal.SizeOf(typeof(VoxelVFX)));
			mGraphicsBuffer.SetData(chunks.AsArray());

			mVisualEffectItem.OpaqueVisualEffect.SetInt(INITIAL_BURST_COUNT_KEY, chunks.Length);
			mVisualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(VFX_BUFFER_KEY, mGraphicsBuffer);
			mVisualEffectItem.OpaqueVisualEffect.Play();

		}

		#endregion
	}
}

