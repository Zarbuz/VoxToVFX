using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;
using System;
using System.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxToVFXFramework.Scripts.Importer;
using VoxToVFXFramework.Scripts.Jobs;

namespace VoxToVFXFramework.Scripts.Data
{
	public class WorldData : IDisposable
	{
		#region Fields

		public UnsafeHashMap<int, UnsafeHashMap<int, Vector4>> WorldDataPositions;
		#endregion

		#region ConstStatic

		public const int CHUNK_SIZE = 100;
		public static readonly int3 ChunkVolume = new int3(CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE);
		public static readonly int3 RelativeWorldVolume = new int3(2000 / CHUNK_SIZE, 2000 / CHUNK_SIZE, 2000 / CHUNK_SIZE);
		#endregion

		#region PublicMethods

		public WorldData()
		{
			WorldDataPositions = new UnsafeHashMap<int, UnsafeHashMap<int, Vector4>>(256, Allocator.Persistent);
		}

		public void AddVoxels(NativeList<Vector4> voxels)
		{
			JobHandle addVoxelsJob = new AddVoxelsJob()
			{
				Voxels = voxels,
				WorldDataPositions = WorldDataPositions
			}.Schedule(voxels.Length, new JobHandle());
			addVoxelsJob.Complete();
		}

		public IEnumerator ComputeLodsChunks(Action<float, VoxelResult> onChunkLoadedCallback, Action onChunkLoadedFinished)
		{
			NativeArray<int> keys = WorldDataPositions.GetKeyArray(Allocator.Persistent);
			for (int index = 0; index < keys.Length; index++)
			{
				int chunkIndex = keys[index];
				int3 worldChunkPosition = GetChunkWorldPosition(chunkIndex);
				Vector3 centerChunkWorldPosition = GetCenterChunkWorldPosition(chunkIndex);
				UnsafeHashMap<int, Vector4> resultLod0 = WorldDataPositions[chunkIndex];

				UnsafeHashMap<int, Vector4> resultLod1 = ComputeLod(resultLod0, worldChunkPosition, 1, 2);
				NativeArray<Vector4> finalResultLod0 = resultLod0.GetValueArray(Allocator.Temp);
				resultLod0.Dispose();
				WorldDataPositions[chunkIndex] = resultLod0;

				VoxelResult voxelResult = new VoxelResult
				{
					Data = finalResultLod0,
					ChunkIndex = chunkIndex,
					FrameWorldPosition = centerChunkWorldPosition,
					LodLevel = 1
				};
				onChunkLoadedCallback?.Invoke(index / (float)keys.Length, voxelResult);
				voxelResult.Data.Dispose();
				yield return new WaitForEndOfFrame();

				UnsafeHashMap<int, Vector4> resultLod2 = ComputeLod(resultLod1, worldChunkPosition, 2, 4);
				NativeArray<Vector4> finalResultLod1 = resultLod1.GetValueArray(Allocator.Temp);
				resultLod1.Dispose();
				voxelResult.Data = finalResultLod1;
				voxelResult.LodLevel = 2;
				onChunkLoadedCallback?.Invoke(index / (float)keys.Length, voxelResult);
				voxelResult.Data.Dispose();
				yield return new WaitForEndOfFrame();

				//NativeHashMap<int, Vector4> resultLod3 = ComputeLod(resultLod2, worldChunkPosition, 4, 8);
				NativeArray<Vector4> finalResultLod2 = resultLod2.GetValueArray(Allocator.Temp);
				resultLod2.Dispose();
				voxelResult.Data = finalResultLod2;
				voxelResult.LodLevel = 4;
				onChunkLoadedCallback?.Invoke(index / (float)keys.Length, voxelResult);
				voxelResult.Data.Dispose();
				yield return new WaitForEndOfFrame();

				//NativeArray<Vector4> finalResultLod3 = ComputeFinalArrayResult(resultLod3);
				//voxelResult.Data = finalResultLod3;
				//voxelResult.LodLevel = 8;
				//onChunkLoadedCallback?.Invoke(index / (float)keys.Item2, voxelResult);
				//finalResultLod3.Dispose();

				//yield return new WaitForEndOfFrame();
			}

			keys.Dispose();
			onChunkLoadedFinished?.Invoke();
		}

		public void Dispose()
		{
			WorldDataPositions.Dispose();
		}

		#endregion

		#region PrivateMethods

		private static UnsafeHashMap<int, Vector4> ComputeLod(UnsafeHashMap<int, Vector4> data, int3 worldChunkPosition, int step, int moduloCheck)
		{
			UnsafeHashMap<int, Vector4> resultLod1 = new UnsafeHashMap<int, Vector4>(data.Count(), Allocator.TempJob);
			ComputeLodJob computeLodJob = new ComputeLodJob()
			{
				VolumeSize = ChunkVolume,
				WorldChunkPosition = worldChunkPosition,
				Result = resultLod1.AsParallelWriter(),
				Data = data,
				Step = step,
				ModuloCheck = moduloCheck
			};
			JobHandle jobHandle = computeLodJob.Schedule(CHUNK_SIZE, 64);
			jobHandle.Complete();

			return resultLod1;
		}

		private static int3 GetChunkWorldPosition(int chunkIndex)
		{
			int3 pos3d = VoxImporter.Get3DPos(chunkIndex, RelativeWorldVolume);
			return pos3d * CHUNK_SIZE;
		}

		private static Vector3 GetCenterChunkWorldPosition(int chunkIndex)
		{
			int3 chunkPosition = GetChunkWorldPosition(chunkIndex);
			int halfChunkSize = CHUNK_SIZE / 2;

			return new Vector3(chunkPosition.x + halfChunkSize, chunkPosition.y + halfChunkSize, chunkPosition.z + halfChunkSize);
		}

		#endregion
	}
}
