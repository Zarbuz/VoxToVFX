using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Extensions;
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
		public Vector3 LodDistance;

		[Range(-1, 3)]
		[OnValueChanged(nameof(RefreshLodsDistance))]
		public int ForcedLevelLod;

		public bool ShowOnlyActiveChunkGizmos;

		[Header("Rotation")]
		public float MinDifferenceAngleCameraForRefresh = 10;
		public float MinTimerCheckRotationCamera = 0.4f;
		#endregion

		#region ConstStatic

		private const string VFX_BUFFER_KEY = "Buffer";

		private const string MATERIAL_VFX_BUFFER_KEY = "MaterialBuffer";
		private const string CHUNK_VFX_BUFFER_KEY = "ChunkBuffer";
		private const string ROTATION_VFX_BUFFER_KEY = "RotationBuffer";
		private const string DEBUG_LOD_KEY = "DebugLod";
		private const string INITIAL_BURST_COUNT_KEY = "InitialBurstCount";
		#endregion

		#region Fields

		public event Action LoadFinishedCallback;

		[HideInInspector]
		public NativeArray<ChunkVFX> Chunks;

		private UnsafeHashMap<int, UnsafeList<VoxelData>> mChunksLoaded;
		private VisualEffectItem mVisualEffectItem;
		private GraphicsBuffer mPaletteBuffer;
		private GraphicsBuffer mGraphicsBuffer;
		private GraphicsBuffer mChunkBuffer;
		private GraphicsBuffer mRotationBuffer;
		private Plane[] mPlanes;

		private bool mIsLoaded;
		private Transform mVisualItemsParent;
		private WorldData mWorldData;

		private int mPreviousPlayerChunkIndex;
		private UnityEngine.Camera mCamera;
		private Quaternion mPreviousRotation;
		private float mPreviousCheckTimer;
		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			mCamera = UnityEngine.Camera.main;
			mVisualItemsParent = new GameObject("VisualItemsParent").transform;
			VoxelFace face1 = Enum.Parse<VoxelFace>(5.ToString());
			VoxelFace face2 = (VoxelFace)5;

			Debug.Log(face1);
			Debug.Log(face2);

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
			int chunkIndex = GetPlayerCurrentChunkIndex(mCamera.transform.position);
			float angle = Quaternion.Angle(mCamera.transform.rotation, mPreviousRotation);
			mPreviousCheckTimer += Time.unscaledDeltaTime;
			if (mPreviousPlayerChunkIndex != chunkIndex || angle > MinDifferenceAngleCameraForRefresh && mPreviousCheckTimer >= MinTimerCheckRotationCamera)
			{
				mPreviousCheckTimer = 0;
				mPreviousPlayerChunkIndex = chunkIndex;
				mPreviousRotation = mCamera.transform.rotation;
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

				foreach (var item in Chunks.Where(t => t.IsActive == 1).GroupBy(t => t.ChunkIndex, t => t.CenterWorldPosition, (key, g) => new { ChunkIndex = key, Position = g.First() }))
				{
					Gizmos.DrawWireCube(item.Position, Vector3.one * WorldData.CHUNK_SIZE);
				}
			}
			else
			{
				Gizmos.color = Color.white;
				foreach (var item in Chunks.GroupBy(t => t.ChunkIndex, t => t.CenterWorldPosition, (key, g) => new { ChunkIndex = key, Position = g.First() }))
				{
					Gizmos.DrawWireCube(item.Position, Vector3.one * WorldData.CHUNK_SIZE);
				}
			}


			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(position, LodDistance.y);

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(position, LodDistance.z);

			//Gizmos.color = Color.red;
			//Gizmos.DrawWireSphere(position, LodDistance.w);
		}

		#endregion

		#region PublicMethods

		public void Release()
		{
			mIsLoaded = false;
			mGraphicsBuffer?.Release();
			mPaletteBuffer?.Release();
			mChunkBuffer?.Release();
			mRotationBuffer?.Release();
			mPaletteBuffer = null;
			mGraphicsBuffer = null;
			mChunkBuffer = null;
			mRotationBuffer = null;
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
				//TODO Check this dispose
				foreach (KeyValue<int, UnsafeList<VoxelData>> item in mChunksLoaded)
				{
					item.Value.Dispose();
				}
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

		public void SetVoxelChunk(int chunkIndex, int lodLevel, NativeArray<VoxelData> array)
		{
			if (!mChunksLoaded.IsCreated)
			{
				mChunksLoaded = new UnsafeHashMap<int, UnsafeList<VoxelData>>(Chunks.Length, Allocator.Persistent);
			}

			int uniqueIndex = GetUniqueChunkIndexWithLodLevel(chunkIndex, lodLevel);
			UnsafeList<VoxelData> unsafeList = new UnsafeList<VoxelData>(array.Length, Allocator.Persistent);
			unsafe
			{
				unsafeList.AddRangeNoResize(array.GetUnsafePtr(), array.Length);
				mChunksLoaded[uniqueIndex] = unsafeList;
			}
		}

		public static int GetUniqueChunkIndexWithLodLevel(int chunkIndex, int lodLevel)
		{
			return chunkIndex + lodLevel * 10000;
		}

		public void OnChunkLoadedFinished()
		{
			mVisualEffectItem = Instantiate(VisualEffectItemPrefab, mVisualItemsParent, false);
			mVisualEffectItem.transform.SetParent(mVisualItemsParent);
			mVisualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER_KEY, mPaletteBuffer);
			mVisualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(CHUNK_VFX_BUFFER_KEY, mChunkBuffer);
			mVisualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(ROTATION_VFX_BUFFER_KEY, mRotationBuffer);
			mVisualEffectItem.OpaqueVisualEffect.enabled = true;

			Debug.Log("[RuntimeVoxController] OnChunkLoadedFinished");
			mCamera.transform.position = new Vector3(1000, 1000, 1000);
			mIsLoaded = true;
			LoadFinishedCallback?.Invoke();
		}

		public void SetChunks(NativeArray<ChunkVFX> chunks)
		{
			Chunks = chunks;

			mChunkBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, chunks.Length, Marshal.SizeOf(typeof(ChunkVFX)));
			mChunkBuffer.SetData(chunks);

			mRotationBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 6, Marshal.SizeOf(typeof(Vector3)));
			Vector3[] rotations = new Vector3[6];
			rotations[0] = new Vector3(90, 0, 0); //Good
			rotations[1] = new Vector3(0, -90, 0); //Good
			rotations[2] = new Vector3(270, 0, 0); //Good
			rotations[3] = new Vector3(0, 90, 0); //Good
			rotations[4] = new Vector3(0, 180, 0); //Good
			rotations[5] = new Vector3(0, 0, 0);	
			mRotationBuffer.SetData(rotations);
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

			NativeList<int> chunkIndex = new NativeList<int>(Allocator.TempJob);
			NativeList<ChunkVFX> activeChunks = new NativeList<ChunkVFX>(Allocator.TempJob);
			for (int index = 0; index < Chunks.Length; index++)
			{
				ChunkVFX chunkVFX = Chunks[index];
				chunkVFX.IsActive = GeometryUtility.TestPlanesAABB(mPlanes, new Bounds(Chunks[index].CenterWorldPosition, Vector3.one * WorldData.CHUNK_SIZE)) ? 1 : 0;
				Chunks[index] = chunkVFX;
				if (chunkVFX.IsActive == 1)
				{
					activeChunks.Add(chunkVFX);
					chunkIndex.Add(index);
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
				Buffer = buffer.AsParallelWriter(),
				ChunkIndex = chunkIndex,
			}.Schedule(totalActive, 64);
			computeRenderingChunkJob.Complete();
			activeChunks.Dispose();
			chunkIndex.Dispose();

			if (buffer.Length > 0)
			{
				RefreshRender(buffer);
			}

			buffer.Dispose();
		}

		private void RefreshRender(NativeList<VoxelVFX> voxels)
		{
			mVisualEffectItem.OpaqueVisualEffect.Reinit();

			//for (int index = 0; index < voxels.Length && index < 200; index++)
			//{
			//	VoxelVFX voxel = voxels[index];
			//	Debug.Log("pos: " + voxel.DecodePosition());
			//	Debug.Log("additional: " + voxel.DecodeAdditionalData());
			//}

			mGraphicsBuffer?.Release();
			mGraphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxels.Length, Marshal.SizeOf(typeof(VoxelVFX)));
			mGraphicsBuffer.SetData(voxels.AsArray());

			mVisualEffectItem.OpaqueVisualEffect.SetInt(INITIAL_BURST_COUNT_KEY, voxels.Length);
			mVisualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(VFX_BUFFER_KEY, mGraphicsBuffer);
			mVisualEffectItem.OpaqueVisualEffect.Play();
		}

		private int GetPlayerCurrentChunkIndex(Vector3 position)
		{
			FastMath.FloorToInt(position.x / WorldData.CHUNK_SIZE, position.y / WorldData.CHUNK_SIZE, position.z / WorldData.CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);
			int chunkIndex = VoxImporter.GetGridPos(chunkX, chunkY, chunkZ, WorldData.RelativeWorldVolume);
			return chunkIndex;
		}

		#endregion
	}
}

