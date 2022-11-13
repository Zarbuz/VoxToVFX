using FileToVoxCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using FileToVoxCore.Vox;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;
using VoxToVFXFramework.Scripts.Core;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Extensions;
using VoxToVFXFramework.Scripts.Importer;
using VoxToVFXFramework.Scripts.Jobs;
using VoxToVFXFramework.Scripts.ScriptableObjets;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI.Popups;
using Plane = UnityEngine.Plane;

namespace VoxToVFXFramework.Scripts.Managers
{
	public class RuntimeVoxManager : ModuleSingleton<RuntimeVoxManager>
	{
		#region SerializeFields

		[SerializeField] private VisualEffect VisualEffectItemPrefab;
		[SerializeField] private VFXListAsset VFXListAsset;
		[SerializeField] private bool ShowOnlyActiveChunkGizmos;
		[SerializeField] private Transform FirstPersonTransform;
		[SerializeField] private Transform FreeCameraTransform;

		#endregion

		#region ConstStatic

		private const string VFX_BUFFER_KEY = "Buffer";

		private const string MATERIAL_VFX_BUFFER_KEY = "MaterialBuffer";
		private const string CHUNK_VFX_BUFFER_KEY = "ChunkBuffer";
		private const string DEBUG_LOD_KEY = "DebugLod";
		private const string EXPOSURE_WEIGHT_KEY = "ExposureWeight";
		private const string INITIAL_BURST_COUNT_KEY = "InitialBurstCount";

		private const float MIN_DIFF_ANGLE_CAMERA = 1f;
		private const float MIN_TIMER_CHECK_CAMERA = 0.1f;

		private const int MAX_CAPACITY_VFX = 5000000;
		private const int BUFFER_COLLIDERS_SIZE = 1000;
		public const int STEP_CAPACITY = 100000;

		#endregion

		#region Fields

		private Transform PlayerPosition
		{
			get
			{
				if (CameraManager.Instance.CameraState == eCameraState.FIRST_PERSON)
				{
					return FirstPersonTransform;
				}

				return FreeCameraTransform;
			}
		}

		public event Action LoadFinishedCallback;
		public event Action UnloadFinishedCallback; 
		public Vector2 MinMaxX { get; set; }
		public Vector2 MinMaxY { get; set; }
		public Vector2 MinMaxZ { get; set; }
		public bool IsReady { get; private set; }

		public Wrapped<bool> DebugLod = new Wrapped<bool>(false);
		//public Wrapped<float> ExposureWeight = new Wrapped<float>(-15);
		public Wrapped<float> ColliderDistance = new Wrapped<float>(5);
		public Wrapped<int> LodDistanceLod0 = new Wrapped<int>(300);
		public Wrapped<int> LodDistanceLod1 = new Wrapped<int>(600);
		public Wrapped<int> ExposureWeight = new Wrapped<int>(-15);

		[HideInInspector] public NativeArray<ChunkVFX> Chunks;

		private UnsafeHashMap<int, UnsafeList<VoxelVFX>> mChunksLoaded;
		private UnsafeHashMap<int, UnsafeList<Matrix4x4>> mChunkPerMaterial;
		//private VisualEffect mVisualEffect;
		//private GraphicsBuffer mPaletteBuffer;
		//private GraphicsBuffer mGraphicsBuffer;

		//private GraphicsBuffer mChunkBuffer;

		private Plane[] mPlanes;
		private Light mDirectionalLight;
		private HDAdditionalLightData mAdditionalLightData;

		private List<GameObject> mBoxColliders;

		private int mPreviousPlayerChunkIndex;
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
			mDirectionalLight = FindObjectOfType<Light>();
			mAdditionalLightData = mDirectionalLight.GetComponent<HDAdditionalLightData>();

			AssemblyFileVersionAttribute runtimeVersion = typeof(VoxModel)
				.GetTypeInfo()
				.Assembly
				.GetCustomAttribute<AssemblyFileVersionAttribute>();

			Debug.Log("FileToVoxCore version: " + runtimeVersion.Version);


			ExposureWeight.OnValueChanged += RefreshExposureWeight;
			DebugLod.OnValueChanged += RefreshDebugLod;

			LodDistanceLod0.OnValueChanged += RefreshChunksToRender;
			LodDistanceLod1.OnValueChanged += RefreshChunksToRender;

			InitCollidersBuffer();
		}

		private void OnDestroy()
		{
			ExposureWeight.OnValueChanged -= RefreshExposureWeight;
			DebugLod.OnValueChanged -= RefreshDebugLod;
			LodDistanceLod0.OnValueChanged -= RefreshChunksToRender;
			LodDistanceLod1.OnValueChanged -= RefreshChunksToRender;

			Release();
		}

		private void Update()
		{
			if (!IsReady)
			{
				return;
			}

			mCurrentChunkWorldIndex = GetPlayerCurrentChunkIndex(PlayerPosition.position);
			float angle = Quaternion.Angle(mCamera.transform.rotation, mPreviousRotation);
			mPreviousCheckTimer += Time.unscaledDeltaTime;
			bool isAnotherChunk = mPreviousPlayerChunkIndex != mCurrentChunkWorldIndex;
			if (isAnotherChunk || angle > MIN_DIFF_ANGLE_CAMERA && mPreviousCheckTimer >= MIN_TIMER_CHECK_CAMERA)
			{
				mPreviousCheckTimer = 0;
				if (isAnotherChunk)
				{
					mPreviousPlayerChunkIndex = mCurrentChunkWorldIndex;
				}
				else
				{
					mPreviousRotation = mCamera.transform.rotation;
				}
				RefreshChunksToRender();
			}

			if (Vector3.Distance(PlayerPosition.position, mPreviousPosition) > 1.5f || isAnotherChunk)
			{
				mPreviousPosition = PlayerPosition.position;
				RefreshColliders();
			}
		}

		private void OnDrawGizmos()
		{
			if (!IsReady)
			{
				return;
			}

			Vector3 position = PlayerPosition.position;
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
			Gizmos.DrawWireSphere(position, LodDistanceLod0.Value);

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(position, LodDistanceLod1.Value);

			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(position, ColliderDistance.Value);

			//Gizmos.color = Color.red;
			//Gizmos.DrawWireSphere(position, LodDistance.w);
		}

		#endregion

		#region PublicMethods

		public void Release()
		{
			IsReady = false;
			//mGraphicsBuffer?.Release();
			//mPaletteBuffer?.Release();
			//mChunkBuffer?.Release();
			////mRotationBuffer?.Release();
			//mPaletteBuffer = null;
			//mGraphicsBuffer = null;
			//mChunkBuffer = null;
			////mRotationBuffer = null;
			//if (mVisualEffect != null)
			//{
			//	Destroy(mVisualEffect.gameObject);
			//}

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

			UnloadFinishedCallback?.Invoke();
		}

		public void SetMaterials(VoxelMaterialVFX[] materials)
		{
			//mPaletteBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, materials.Length, Marshal.SizeOf(typeof(VoxelMaterialVFX)));
			//mPaletteBuffer.SetData(materials);
		}

		public void SetVoxelChunk(int chunkIndex, UnsafeList<VoxelVFX> list)
		{
			if (!mChunksLoaded.IsCreated)
			{
				mChunksLoaded = new UnsafeHashMap<int, UnsafeList<VoxelVFX>>(Chunks.Length, Allocator.Persistent);
			}

			if (!mChunkPerMaterial.IsCreated)
			{
				mChunkPerMaterial = new UnsafeHashMap<int, UnsafeList<Matrix4x4>>(255, Allocator.Persistent);
			}

			mChunksLoaded[chunkIndex] = list;
		}

		public void OnChunkLoadedFinished()
		{
			//mVisualEffect = Instantiate(VisualEffectItemPrefab);
			
			//mVisualEffect.enabled = true;
			SetPlayerToWorldCenter();
			Debug.Log("[RuntimeVoxController] OnChunkLoadedFinished");
			IsReady = true;
			LoadFinishedCallback?.Invoke();
		}

		public void SetChunks(NativeArray<ChunkVFX> chunks)
		{
			Chunks = chunks;

			//mChunkBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, chunks.Length, Marshal.SizeOf(typeof(ChunkVFX)));
			//mChunkBuffer.SetData(chunks);
		}

		public void RefreshChunksToRender()
		{
			if (!Chunks.IsCreated)
				return;

			mPlanes = GeometryUtility.CalculateFrustumPlanes(mCamera);

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
				LodDistanceLod0 = LodDistanceLod0.Value,
				LodDistanceLod1 = LodDistanceLod1.Value,
				PlayerPosition = PlayerPosition.position,
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

		public void SetPlayerToWorldCenter()
		{
			float distance = 10;
			FirstPersonTransform.transform.position = new Vector3((MinMaxX.y + MinMaxX.x) / 2, 2000, (MinMaxZ.y + MinMaxZ.x) / 2);
			mCurrentChunkWorldIndex = GetPlayerCurrentChunkIndex(FirstPersonTransform.position);
			RefreshColliders();
			bool foundCollisions;
			do
			{
				foundCollisions = Physics.Raycast(FirstPersonTransform.transform.position, Vector3.down, out RaycastHit hit, distance);
				if (!foundCollisions)
				{
					//TODO: Add more verification if there is a hole in the center of the map
					FirstPersonTransform.transform.Translate(Vector3.down * distance);
					mCurrentChunkWorldIndex = GetPlayerCurrentChunkIndex(FirstPersonTransform.position);
					RefreshColliders();
				}
				else
				{
					FirstPersonTransform.transform.position = hit.point + Vector3.up * 0.1f;
				}

			} while (!foundCollisions);
		}
		#endregion

		#region PrivateMethods

		private void InitCollidersBuffer()
		{
			mBoxColliders = new List<GameObject>(BUFFER_COLLIDERS_SIZE);
			GameObject colliderParent = new GameObject("ColliderParent");

			for (int i = 0; i < mBoxColliders.Capacity; i++)
			{
				GameObject collider = new GameObject("Collider_" + i);
				collider.transform.parent = colliderParent.transform;
				BoxCollider boxCollider = collider.AddComponent<BoxCollider>();
				boxCollider.size = Vector3.one;
				mBoxColliders.Add(collider);
			}
		}

		private void RefreshColliders()
		{
			for (int index = 0; index < Chunks.Length; index++)
			{
				ChunkVFX chunkVFX = Chunks[index];
				if (chunkVFX.ChunkIndex == mCurrentChunkWorldIndex && chunkVFX.LodLevel == 1)
				{
					mCurrentChunkIndex = index;
				}
			}

			UnsafeList<VoxelVFX> data = mChunksLoaded[mCurrentChunkIndex];
			NativeList<int> buffer = new NativeList<int>(data.Length, Allocator.TempJob);
			Vector3 position = PlayerPosition.position;
			Vector3 worldPosition = Chunks[mCurrentChunkIndex].WorldPosition;

			JobHandle computeVoxelNearPlayer = new ComputeVoxelNearPlayer()
			{
				Data = data,
				Buffer = buffer.AsParallelWriter(),
				DistanceCheckVoxels = ColliderDistance.Value,
				ChunkWorldPosition = new float3(worldPosition.x, worldPosition.y, worldPosition.z),
				PlayerPosition = new float3(position.x, position.y, position.z)
			}.Schedule(data.Length, 64);

			computeVoxelNearPlayer.Complete();

			for (int i = 0; i < buffer.Length; i++)
			{
				if (i < BUFFER_COLLIDERS_SIZE)
				{
					VoxelVFX voxel = data[buffer[i]];
					float4 voxelRelativePosition = voxel.DecodePosition();

					mBoxColliders[i].transform.position = new Vector3(worldPosition.x + voxelRelativePosition.x, worldPosition.y + voxelRelativePosition.y, worldPosition.z + voxelRelativePosition.z);
				}
			}

			buffer.Dispose();
		}
		private void RefreshDebugLod()
		{
			if (!IsReady)
			{
				return;
			}
			//mVisualEffect.Reinit();
			//mVisualEffect.SetBool(DEBUG_LOD_KEY, DebugLod.Value);
			//mVisualEffect.Play();
		}

		private void RefreshExposureWeight()
		{
			if (!IsReady)
			{
				return;
			}
			//mVisualEffect.Reinit();
			//mVisualEffect.SetFloat(EXPOSURE_WEIGHT_KEY, ExposureWeight.Value);
			//mVisualEffect.Play();
		}

		private void RefreshRender(NativeList<VoxelVFX> voxels)
		{
			//mVisualEffect.visualEffectAsset = GetVisualEffectAsset(voxels.Length);
			//mVisualEffect.Reinit();

			//for (int index = 0; index < voxels.Length && index < 200; index++)
			//{
			//	VoxelVFX voxel = voxels[index];
			//	Debug.Log("pos: " + voxel.DecodePosition());
			//	Debug.Log("additional: " + voxel.DecodeAdditionalData());
			//}

			//string colorGreen = "green";
			//string colorRed = "red";
			//Debug.Log($"[RuntimeVoxManager] <color={(voxels.Length < MAX_CAPACITY_VFX ? colorGreen : colorRed)}> RefreshRender: {voxels.Length}</color>");

			//mGraphicsBuffer?.Release();
			//mGraphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, voxels.Length, Marshal.SizeOf(typeof(VoxelVFX)));
			//mGraphicsBuffer.SetData(voxels.AsArray());

			//mVisualEffect.SetInt(INITIAL_BURST_COUNT_KEY, voxels.Length);
			//mVisualEffect.SetGraphicsBuffer(VFX_BUFFER_KEY, mGraphicsBuffer);
			//mVisualEffect.SetGraphicsBuffer(MATERIAL_VFX_BUFFER_KEY, mPaletteBuffer);
			//mVisualEffect.SetGraphicsBuffer(CHUNK_VFX_BUFFER_KEY, mChunkBuffer);
			//mVisualEffect.SetFloat(EXPOSURE_WEIGHT_KEY, ExposureWeight.Value);
			//mVisualEffect.SetBool(DEBUG_LOD_KEY, DebugLod.Value);

			//mVisualEffect.Play();

			//mAdditionalLightData.RequestShadowMapRendering();

			Dictionary<int, List<Matrix4x4>> chunks = new Dictionary<int, List<Matrix4x4>>();
			foreach (VoxelVFX voxel in voxels)
			{
				float4 decodedPosition = voxel.DecodePosition();
				int colorIndex = (int)decodedPosition.w;
				if (!chunks.ContainsKey(colorIndex))
				{
					chunks.Add(colorIndex, new List<Matrix4x4>());
				}

				uint voxelChunkIndex = voxel.DecodeChunkIndex();
				ChunkVFX chunk = Chunks[(int)voxelChunkIndex];

				Vector3 worldPosition = chunk.WorldPosition + new Vector3(decodedPosition.x, decodedPosition.y, decodedPosition.z);
				Matrix4x4 matrix = new Matrix4x4();
				matrix.SetTRS(worldPosition, Quaternion.identity, Vector3.one * chunk.LodLevel);
				chunks[colorIndex].Add(matrix);
			}

			ManualRTASManager.Instance.Build(chunks);
		}

		private VisualEffectAsset GetVisualEffectAsset(int voxelCount)
		{
			int index = voxelCount / STEP_CAPACITY;
			if (index > VFXListAsset.VisualEffectAssets.Count)
			{
				index = VFXListAsset.VisualEffectAssets.Count - 1;
			}

			VisualEffectAsset asset = VFXListAsset.VisualEffectAssets[index];
			Debug.Log("[RuntimeVoxManager] Selected VisualEffectAsset: " + asset.name);
			return asset;
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

