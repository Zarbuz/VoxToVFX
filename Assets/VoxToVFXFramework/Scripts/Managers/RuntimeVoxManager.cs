using FileToVoxCore.Utils;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;
using VoxToVFXFramework.Scripts.Components;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;
using VoxToVFXFramework.Scripts.Jobs;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.System;
using Plane = UnityEngine.Plane;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class RuntimeVoxManager : ModuleSingleton<RuntimeVoxManager>
	{
		#region SerializeFields

		[SerializeField] private VisualEffect VisualEffectItemPrefab;

		[Header("Lods")]
		public bool DebugLod;

		public Vector3 LodDistance;

		[Range(-1, 3)]
		public int ForcedLevelLod;

		public bool ShowOnlyActiveChunkGizmos;
		public bool ForceLoadAllChunks;

		[Header("Rotation")]
		public float MinDifferenceAngleCameraForRefresh = 10;
		public float MinTimerCheckRotationCamera = 0.4f;

		[Header("Exposure")]
		public float ExposureWeight;
		#endregion

		#region ConstStatic

		private const string VFX_BUFFER_KEY = "Buffer";

		private const string MATERIAL_VFX_BUFFER_KEY = "MaterialBuffer";
		private const string CHUNK_VFX_BUFFER_KEY = "ChunkBuffer";
		private const string DEBUG_LOD_KEY = "DebugLod";
		private const string EXPOSURE_WEIGHT_KEY = "ExposureWeight";
		private const string INITIAL_BURST_COUNT_KEY = "InitialBurstCount";
		#endregion

		#region Fields

		public event Action LoadFinishedCallback;

		[HideInInspector]
		public NativeArray<ChunkVFX> Chunks;

		private UnsafeParallelHashMap<int, UnsafeList<VoxelVFX>> mChunksLoaded;
		private VisualEffect mVisualEffect;
		private GraphicsBuffer mPaletteBuffer;
		private GraphicsBuffer mGraphicsBuffer;
		private GraphicsBuffer mChunkBuffer;
		//private GraphicsBuffer mRotationBuffer;
		private Plane[] mPlanes;
		private Light mDirectionalLight;
		private HDAdditionalLightData mAdditionalLightData;

		private Entity mEntityPrefab;
		private PhysicsShapeQuerySystem mPhysicsShapeQuerySystem;
		private EndSimulationEntityCommandBufferSystem mEndSimulationEntityCommandBufferSystem;
		private EntityManager mEntityManager;
		private EntityArchetype mEntityArchetype;

		private bool mIsLoaded;
		private Transform mVisualItemsParent;

		private bool mDebugLog;
		private Vector3 mLodDistance;
		private int mForcedLevelLod;

		private int mPreviousPlayerChunkIndex;
		private int mPreviousForcedLoadedChunks;
		private bool mPreviousForceLoadAllChunks;
		private int mCurrentChunkWorldIndex;
		private int mCurrentChunkIndex;
		private UnityEngine.Camera mCamera;
		private Quaternion mPreviousRotation;
		private Vector3 mPreviousPosition;
		private float mPreviousCheckTimer;
		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			mCamera = UnityEngine.Camera.main;
			mVisualItemsParent = new GameObject("VisualItemsParent").transform;
			mDirectionalLight = FindObjectOfType<Light>();
			mAdditionalLightData = mDirectionalLight.GetComponent<HDAdditionalLightData>();
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

			if (mDebugLog != DebugLod)
			{
				mDebugLog = DebugLod;
				RefreshDebugLod();
			}

			if (mLodDistance != LodDistance)
			{
				mLodDistance = LodDistance;
				RefreshChunksToRender();
			}

			if (mForcedLevelLod != ForcedLevelLod)
			{
				mForcedLevelLod = ForcedLevelLod;
				RefreshChunksToRender();
			}

			mPlanes = GeometryUtility.CalculateFrustumPlanes(mCamera);
			mCurrentChunkWorldIndex = GetPlayerCurrentChunkIndex(mCamera.transform.position);
			float angle = Quaternion.Angle(mCamera.transform.rotation, mPreviousRotation);
			mPreviousCheckTimer += Time.unscaledDeltaTime;
			bool isAnotherChunk = mPreviousPlayerChunkIndex != mCurrentChunkWorldIndex;
			if (isAnotherChunk || angle > MinDifferenceAngleCameraForRefresh && mPreviousCheckTimer >= MinTimerCheckRotationCamera)
			{
				mPreviousCheckTimer = 0;
				if (isAnotherChunk)
				{
					mPreviousPlayerChunkIndex = mCurrentChunkWorldIndex;
				}
				mPreviousRotation = mCamera.transform.rotation;
				mPreviousForcedLoadedChunks = ForcedLevelLod;
				RefreshChunksToRender();
			}

			if (Vector3.Distance(mCamera.transform.position , mPreviousPosition) > 1.5f)
			{
				mPreviousPosition = mCamera.transform.position;
				RefreshChunksColliders();
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
			//mRotationBuffer?.Release();
			mPaletteBuffer = null;
			mGraphicsBuffer = null;
			mChunkBuffer = null;
			//mRotationBuffer = null;
			if (mVisualEffect != null)
			{
				Destroy(mVisualEffect.gameObject);
			}

			if (Chunks.IsCreated)
			{
				Chunks.Dispose();
			}


			if (mChunksLoaded.IsCreated)
			{
				//TODO Check this dispose
				foreach (KeyValue<int, UnsafeList<VoxelVFX>> item in mChunksLoaded)
				{
					item.Value.Dispose();
				}
				mChunksLoaded.Dispose();
			}
		}

		public void InitEntities()
		{
			mEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			mEntityArchetype = mEntityManager.CreateArchetype(
				typeof(Translation),
				typeof(PhysicsCollider),
				typeof(LocalToWorld),
				typeof(PhysicsWorldIndex),
				typeof(VoxelPrefabTag));

			mPhysicsShapeQuerySystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<PhysicsShapeQuerySystem>();
			mEndSimulationEntityCommandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		public void SetForceLODValue(int value)
		{
			ForcedLevelLod = value;
			RefreshChunksToRender();
		}

		public void SetDebugLodValue(bool value)
		{
			DebugLod = value;
			RefreshDebugLod();
		}

		public void SetDisableCullingChunks(bool value)
		{
			ForceLoadAllChunks = value;
			RefreshChunksToRender();
		}

		public void SetMaterials(VoxelMaterialVFX[] materials)
		{
			mPaletteBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, materials.Length, Marshal.SizeOf(typeof(VoxelMaterialVFX)));
			mPaletteBuffer.SetData(materials);
		}

		public void SetExposureWeight(float exposureWeight)
		{
			ExposureWeight = exposureWeight;
			RefreshExposureWeight();
		}

		public void SetVoxelChunk(int chunkIndex, UnsafeList<VoxelVFX> list)
		{
			if (!mChunksLoaded.IsCreated)
			{
				mChunksLoaded = new UnsafeParallelHashMap<int, UnsafeList<VoxelVFX>>(Chunks.Length, Allocator.Persistent);
			}
			mChunksLoaded[chunkIndex] = list;
		}

		public void OnChunkLoadedFinished()
		{
			InitEntities();
			mVisualEffect = Instantiate(VisualEffectItemPrefab, mVisualItemsParent, false);
			mVisualEffect.transform.SetParent(mVisualItemsParent);
			mVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER_KEY, mPaletteBuffer);
			mVisualEffect.SetGraphicsBuffer(CHUNK_VFX_BUFFER_KEY, mChunkBuffer);
			//mVisualEffectItem.OpaqueVisualEffect.SetGraphicsBuffer(ROTATION_VFX_BUFFER_KEY, mRotationBuffer);
			mVisualEffect.enabled = true;

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
		}

		#endregion

		#region PrivateMethods

		private void RefreshDebugLod()
		{
			mVisualEffect.Reinit();
			mVisualEffect.SetBool(DEBUG_LOD_KEY, DebugLod);
			mVisualEffect.Play();
		}

		private void RefreshExposureWeight()
		{
			mVisualEffect.Reinit();
			mVisualEffect.SetFloat(EXPOSURE_WEIGHT_KEY, ExposureWeight);
			mVisualEffect.Play();
		}

		private void RefreshChunksToRender()
		{
			if (!Chunks.IsCreated)
				return;

			if (ForceLoadAllChunks && mPreviousForceLoadAllChunks && mPreviousForcedLoadedChunks == ForcedLevelLod)
			{
				return;
			}

			NativeList<int> chunkIndex = new NativeList<int>(Allocator.TempJob);
			NativeList<ChunkVFX> activeChunks = new NativeList<ChunkVFX>(Allocator.TempJob);
			for (int index = 0; index < Chunks.Length; index++)
			{
				ChunkVFX chunkVFX = Chunks[index];
				if (ForceLoadAllChunks)
				{
					chunkVFX.IsActive = 1;
				}
				else
				{
					chunkVFX.IsActive = GeometryUtility.TestPlanesAABB(mPlanes, new Bounds(Chunks[index].CenterWorldPosition, Vector3.one * WorldData.CHUNK_SIZE)) ? 1 : 0;
				}
				Chunks[index] = chunkVFX;
				if (chunkVFX.IsActive == 1)
				{
					activeChunks.Add(chunkVFX);
					chunkIndex.Add(index);
				}

				if (chunkVFX.ChunkIndex == mCurrentChunkWorldIndex && chunkVFX.LodLevel == 1)
				{
					mCurrentChunkIndex = index;
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
			mPreviousForceLoadAllChunks = ForceLoadAllChunks;

			if (buffer.Length > 0)
			{
				RefreshRender(buffer);
			}

			buffer.Dispose();
		}

		private void RefreshChunksColliders()
		{
			ChunkVFX? currentChunk = Chunks.Where(chunk => chunk.ChunkIndex == mCurrentChunkWorldIndex && chunk.LodLevel == 1).Cast<ChunkVFX?>().FirstOrDefault();
			if (currentChunk == null)
			{
				mEntityManager.DestroyEntity(mPhysicsShapeQuerySystem.EntityQuery);
				return;
			}
			EntityCommandBuffer ecb = mEndSimulationEntityCommandBufferSystem.CreateCommandBuffer();
			UnsafeList<VoxelVFX> data = mChunksLoaded[mCurrentChunkIndex];

			mEntityManager.DestroyEntity(mPhysicsShapeQuerySystem.EntityQuery);
			mEntityPrefab = mEntityManager.CreateEntity(mEntityArchetype);

			JobHandle createPhysicsEntityJob = new CreatePhysicsEntityJob()
			{
				Chunk = currentChunk.Value,
				ECB = ecb.AsParallelWriter(),
				PrefabEntity = mEntityPrefab,
				Data = data,
				Collider = mPhysicsShapeQuerySystem.ColliderLod1,
				PlayerPosition = new float3(mCamera.transform.position.x, mCamera.transform.position.y, mCamera.transform.position.z),
				DistanceCheckVoxels = 5 //Move to params ? 
			}.Schedule(data.Length, 64);

			createPhysicsEntityJob.Complete();
		}

		private void RefreshRender(NativeList<VoxelVFX> voxels)
		{
			mVisualEffect.Reinit();

			//for (int index = 0; index < voxels.Length && index < 200; index++)
			//{
			//	VoxelVFX voxel = voxels[index];
			//	Debug.Log("pos: " + voxel.DecodePosition());
			//	Debug.Log("additional: " + voxel.DecodeAdditionalData());
			//}

			mGraphicsBuffer?.Release();
			mGraphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxels.Length, Marshal.SizeOf(typeof(VoxelVFX)));
			mGraphicsBuffer.SetData(voxels.AsArray());

			mVisualEffect.SetInt(INITIAL_BURST_COUNT_KEY, voxels.Length);
			mVisualEffect.SetGraphicsBuffer(VFX_BUFFER_KEY, mGraphicsBuffer);
			mVisualEffect.Play();

			mAdditionalLightData.RequestShadowMapRendering();
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

