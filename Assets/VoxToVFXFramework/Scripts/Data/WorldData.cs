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
		public UnsafeHashMap<int, UnsafeHashMap<int, VoxelData>> WorldDataPositions;
		#endregion

		#region ConstStatic

		public const int CHUNK_SIZE = 100;
		public static readonly int3 ChunkVolume = new int3(CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE);
		public static readonly int3 RelativeWorldVolume = new int3(2000 / CHUNK_SIZE, 2000 / CHUNK_SIZE, 2000 / CHUNK_SIZE);
		#endregion

		#region PublicMethods

		public WorldData()
		{
			WorldDataPositions = new UnsafeHashMap<int, UnsafeHashMap<int, VoxelData>>(256, Allocator.Persistent);
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
				int3 chunkWorldPosition = GetChunkWorldPosition(chunkIndex);
				Vector3 chunkCenterWorldPosition = GetCenterChunkWorldPosition(chunkIndex);
				UnsafeHashMap<int, VoxelData> resultLod0 = WorldDataPositions[chunkIndex];

				UnsafeHashMap<int, VoxelData> resultLod1 = ComputeLod(resultLod0, chunkWorldPosition, 1, 2);
				NativeList<VoxelData> finalResultLod0 = ComputeRotation(WorldDataPositions[chunkIndex], 1);
				resultLod0.Dispose();
				WorldDataPositions[chunkIndex] = resultLod0;

				VoxelResult voxelResult = new VoxelResult
				{
					Data = finalResultLod0,
					ChunkIndex = chunkIndex,
					ChunkCenterWorldPosition = chunkCenterWorldPosition,
					ChunkWorldPosition = new Vector3(chunkWorldPosition.x, chunkWorldPosition.y, chunkWorldPosition.z),
					LodLevel = 1
				};
				onChunkLoadedCallback?.Invoke(index / (float)keys.Length, voxelResult);
				voxelResult.Data.Dispose();
				yield return new WaitForEndOfFrame();

				UnsafeHashMap<int, VoxelData> resultLod2 = ComputeLod(resultLod1, chunkWorldPosition, 2, 4);
				NativeList<VoxelData> finalResultLod1 = ComputeRotation(resultLod1, 2);
				resultLod1.Dispose();
				voxelResult.Data = finalResultLod1;
				voxelResult.LodLevel = 2;
				onChunkLoadedCallback?.Invoke(index / (float)keys.Length, voxelResult);
				voxelResult.Data.Dispose();
				yield return new WaitForEndOfFrame();

				NativeList<VoxelData> finalResultLod2 = ComputeRotation(resultLod2, 4);
				resultLod2.Dispose();
				voxelResult.Data = finalResultLod2;
				voxelResult.LodLevel = 4;
				onChunkLoadedCallback?.Invoke(index / (float)keys.Length, voxelResult);
				voxelResult.Data.Dispose();
				yield return new WaitForEndOfFrame();

				
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

		private static UnsafeHashMap<int, VoxelData> ComputeLod(UnsafeHashMap<int, VoxelData> data, int3 worldChunkPosition, int step, int moduloCheck)
		{
			UnsafeHashMap<int, VoxelData> resultLod1 = new UnsafeHashMap<int, VoxelData>(data.Count(), Allocator.TempJob);
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

		private static NativeList<VoxelData> ComputeRotation(UnsafeHashMap<int, VoxelData> data, int step)
		{
			NativeArray<int> keys = data.GetKeyArray(Allocator.TempJob);
			NativeList<VoxelData> result = new NativeList<VoxelData>(data.Count(), Allocator.TempJob);

			JobHandle computeRotationJob = new ComputeVoxelRotationJob()
			{
				Data = data,
				Result = result.AsParallelWriter(),
				Step = step,
				VolumeSize = ChunkVolume,
				Keys = keys,
				Materials = VoxImporter.Materials
			}.Schedule(data.Count(), 64);
			computeRotationJob.Complete();

			keys.Dispose();
			return result;
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
