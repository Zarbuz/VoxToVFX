using FileToVoxCore.Utils;
using System;
using System.Collections;
using FileToVoxCore.Schematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections.NotBurstCompatible;
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

		public NativeMultiHashMap<int, int4> WorldDataPositions;
		public NativeHashMap<int, int> WorldDataIndices;
		#endregion

		#region ConstStatic

		public const int CHUNK_SIZE = 500;
		public static int3 ChunkVolume = new int3(CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE);
		public static int3 WorldVolume = new int3(Schematic.MAX_WORLD_WIDTH, Schematic.MAX_WORLD_HEIGHT, Schematic.MAX_WORLD_LENGTH);
		private const int INITIAL_CAPACITY = 1000000;

		#endregion

		#region PublicMethods

		public WorldData()
		{
			WorldDataPositions = new NativeMultiHashMap<int, int4>(INITIAL_CAPACITY, Allocator.Persistent); //double capacity strategy
			WorldDataIndices = new NativeHashMap<int, int>(INITIAL_CAPACITY, Allocator.Persistent);
		}

		public void AddVoxels(NativeList<int4> voxels)
		{
			foreach (int4 vector4 in voxels)
			{
				FastMath.FloorToInt(vector4.x / CHUNK_SIZE, vector4.y / CHUNK_SIZE, vector4.z / CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);
				int chunkIndex = VoxImporter.GetGridPos(chunkX, chunkY, chunkZ, WorldVolume);
				if (vector4.y > 0)
				{
					WorldDataPositions.Add(chunkIndex, vector4);
					WorldDataIndices[chunkIndex] = chunkIndex;
				}
			}
		}

		public IEnumerator ComputeLodsChunks(Action<float, VoxelResult> onChunkLoadedCallback, Action onChunkLoadedFinished)
		{
			NativeArray<int> keys = WorldDataIndices.GetKeyArray(Allocator.Persistent);
			for (int index = 0; index < keys.Length; index++)
			{
				int chunkIndex = keys[index];
				int3 worldChunkPosition = GetChunkWorldPosition(chunkIndex);
				NativeHashMap<int, int4> resultLod0 = ComputeInitialChunkData(WorldDataPositions, chunkIndex);
				WorldDataPositions.Remove(chunkIndex);

				NativeHashMap<int, int4> resultLod1 = ComputeLod(resultLod0, worldChunkPosition, 1, 2);
				NativeArray<Vector4> finalResultLod0 = ComputeFinalArrayResult(resultLod0); //will dispose resultLod0

				NativeHashMap<int, int4> resultLod2 = ComputeLod(resultLod1, worldChunkPosition, 2, 4);
				NativeArray<Vector4> finalResultLod1 = ComputeFinalArrayResult(resultLod1);

				NativeHashMap<int, int4> resultLod3 = ComputeLod(resultLod2, worldChunkPosition, 4, 8);
				NativeArray<Vector4> finalResultLod2 = ComputeFinalArrayResult(resultLod2);
				NativeArray<Vector4> finalResultLod3 = ComputeFinalArrayResult(resultLod3);

				VoxelResult voxelResult = new VoxelResult
				{
					DataLod0 = finalResultLod0,
					DataLod1 = finalResultLod1,
					DataLod2 = finalResultLod2,
					DataLod3 = finalResultLod3,
					FrameWorldPosition = GetCenterChunkWorldPosition(chunkIndex)
				};

				onChunkLoadedCallback?.Invoke(index / (float)keys.Length, voxelResult);
				voxelResult.DataLod0.Dispose();
				voxelResult.DataLod1.Dispose();
				voxelResult.DataLod2.Dispose();
				voxelResult.DataLod3.Dispose();
				yield return new WaitForEndOfFrame();
			}

			keys.Dispose();
			onChunkLoadedFinished?.Invoke();
		}

		public void Dispose()
		{
			WorldDataPositions.Dispose();
			WorldDataIndices.Dispose();
		}

		#endregion

		#region PrivateMethods

		private static NativeHashMap<int, int4> ComputeInitialChunkData(NativeMultiHashMap<int, int4> worldDataPositions, int chunkIndex)
		{
			NativeMultiHashMap<int, int4>.Enumerator enumerator = worldDataPositions.GetValuesForKey(chunkIndex).GetEnumerator();
			NativeHashMap<int, int4> data = new NativeHashMap<int, int4>(INITIAL_CAPACITY, Allocator.TempJob);
			while (enumerator.MoveNext())
			{
				int4 voxel = enumerator.Current;
				int xFinal = voxel.x % CHUNK_SIZE;
				int yFinal = voxel.y % CHUNK_SIZE;
				int zFinal = voxel.z % CHUNK_SIZE;

				data[VoxImporter.GetGridPos(xFinal, yFinal, zFinal, ChunkVolume)] = voxel;
			}
			enumerator.Dispose();

			return data;
		}

		private static NativeHashMap<int, int4> ComputeLod(NativeHashMap<int, int4> data, int3 worldChunkPosition, int step, int moduloCheck)
		{
			NativeHashMap<int, int4> resultLod1 = new NativeHashMap<int, int4>(data.Count(), Allocator.TempJob);
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

		private static NativeArray<Vector4> ComputeFinalArrayResult(NativeHashMap<int, int4> resultLod0)
		{
			NativeArray<int4> dataValue0 = resultLod0.GetValueArray(Allocator.TempJob);
			NativeArray<Vector4> finalResultLod0 = new NativeArray<Vector4>(dataValue0.Length, Allocator.TempJob);
			ConvertInt4ToVector4Job convertInt4Job = new ConvertInt4ToVector4Job()
			{
				Data = dataValue0,
				Result = finalResultLod0
			};

			JobHandle jobHandle4 = convertInt4Job.Schedule(dataValue0.Length, 64);
			jobHandle4.Complete();
			dataValue0.Dispose();
			resultLod0.Dispose();
			return finalResultLod0;
		}

		private static int3 GetChunkWorldPosition(int chunkIndex)
		{
			int3 pos3d = VoxImporter.Get3DPos(chunkIndex, WorldVolume);
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
