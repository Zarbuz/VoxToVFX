using FileToVoxCore.Utils;
using System;
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
		#endregion

		#region PublicMethods

		public WorldData()
		{
			WorldDataPositions = new NativeMultiHashMap<int, int4>(256, Allocator.Persistent); //double capacity strategy
			WorldDataIndices = new NativeHashMap<int, int>(256, Allocator.Persistent);
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

		public void ComputeLodsChunks(Action<float, VoxelResult> onChunkLoadedCallback)
		{
			NativeArray<int> keys = WorldDataIndices.GetKeyArray(Allocator.Temp);
			for (int index = 0; index < keys.Length; index++)
			{
				int chunkIndex = keys[index];
				NativeMultiHashMap<int, int4>.Enumerator enumerator = WorldDataPositions.GetValuesForKey(chunkIndex).GetEnumerator();
				NativeHashMap<int, int4> data = new NativeHashMap<int, int4>(200, Allocator.TempJob);
				int3 worldChunkPosition = GetChunkWorldPosition(chunkIndex);
				while (enumerator.MoveNext())
				{
					int4 voxel = enumerator.Current;
					int xFinal = voxel.x % CHUNK_SIZE;
					int yFinal = voxel.y % CHUNK_SIZE;
					int zFinal = voxel.z % CHUNK_SIZE;

					data[VoxImporter.GetGridPos(xFinal, yFinal, zFinal, ChunkVolume)] = voxel;
				}
				enumerator.Dispose();

				NativeHashMap<int, int4> resultLod1 = new NativeHashMap<int, int4>(data.Count(), Allocator.TempJob);
				ComputeLodJob computeLodJob = new ComputeLodJob()
				{
					VolumeSize = ChunkVolume,
					WorldChunkPosition = worldChunkPosition,
					Result = resultLod1.AsParallelWriter(),
					Data = data,
					Step = 1,
					ModuloCheck = 2
				};
				JobHandle jobHandle = computeLodJob.Schedule(CHUNK_SIZE, 64);
				jobHandle.Complete();

				NativeHashMap<int, int4> resultLod2 = new NativeHashMap<int, int4>(resultLod1.Count(), Allocator.TempJob);
				ComputeLodJob computeLodJob2 = new ComputeLodJob()
				{
					VolumeSize = ChunkVolume,
					WorldChunkPosition = worldChunkPosition,
					Result = resultLod2.AsParallelWriter(),
					Data = resultLod1,
					Step = 2,
					ModuloCheck = 4
				};
				JobHandle jobHandle2 = computeLodJob2.Schedule(CHUNK_SIZE, 64);
				jobHandle2.Complete();

				NativeHashMap<int, int4> resultLod3 = new NativeHashMap<int, int4>(resultLod2.Count(), Allocator.TempJob);
				ComputeLodJob computeLodJob3 = new ComputeLodJob()
				{
					VolumeSize = ChunkVolume,
					WorldChunkPosition = worldChunkPosition,
					Result = resultLod3.AsParallelWriter(),
					Data = resultLod2,
					Step = 4,
					ModuloCheck = 8
				};
				JobHandle jobHandle3 = computeLodJob3.Schedule(CHUNK_SIZE, 64);
				jobHandle3.Complete();


				NativeArray<int4> dataValue0 = data.GetValueArray(Allocator.TempJob);
				NativeArray<Vector4> finalResultLod0 = new NativeArray<Vector4>(dataValue0.Length, Allocator.TempJob);
				ConvertInt4ToVector4Job convertInt4Job = new ConvertInt4ToVector4Job()
				{
					Data = dataValue0,
					Result = finalResultLod0
				};

				JobHandle jobHandle4 = convertInt4Job.Schedule(dataValue0.Length, 64);
				jobHandle4.Complete();
				dataValue0.Dispose();

				NativeArray<int4> dataValue1 = resultLod1.GetValueArray(Allocator.TempJob);
				NativeArray<Vector4> finalResultLod1 = new NativeArray<Vector4>(dataValue1.Length, Allocator.TempJob);
				ConvertInt4ToVector4Job convertInt4Job1 = new ConvertInt4ToVector4Job()
				{
					Data = dataValue1,
					Result = finalResultLod1
				};

				JobHandle jobHandle5 = convertInt4Job1.Schedule(dataValue1.Length, 64);
				jobHandle5.Complete();
				dataValue1.Dispose();

				NativeArray<int4> dataValue2 = resultLod2.GetValueArray(Allocator.TempJob);
				NativeArray<Vector4> finalResultLod2 = new NativeArray<Vector4>(dataValue2.Length, Allocator.TempJob);
				ConvertInt4ToVector4Job convertInt4Job2 = new ConvertInt4ToVector4Job()
				{
					Data = dataValue2,
					Result = finalResultLod2
				};

				JobHandle jobHandle6 = convertInt4Job2.Schedule(dataValue2.Length, 64);
				jobHandle6.Complete();
				dataValue2.Dispose();

				NativeArray<int4> dataValue3 = resultLod3.GetValueArray(Allocator.TempJob);
				NativeArray<Vector4> finalResultLod3 = new NativeArray<Vector4>(dataValue3.Length, Allocator.TempJob);
				ConvertInt4ToVector4Job convertInt4Job3 = new ConvertInt4ToVector4Job()
				{
					Data = dataValue3,
					Result = finalResultLod3
				};

				JobHandle jobHandle7 = convertInt4Job3.Schedule(dataValue3.Length, 64);
				jobHandle7.Complete();
				dataValue3.Dispose();

				VoxelResult voxelResult = new VoxelResult
				{
					DataLod0 = finalResultLod0,
					DataLod1 = finalResultLod1,
					DataLod2 = finalResultLod2,
					DataLod3 = finalResultLod3,
					FrameWorldPosition = GetCenterChunkWorldPosition(chunkIndex)
				};

				resultLod1.Dispose();
				resultLod2.Dispose();
				resultLod3.Dispose();
				data.Dispose();
				onChunkLoadedCallback?.Invoke(index / (float)keys.Length, voxelResult);

				voxelResult.DataLod0.Dispose();
				voxelResult.DataLod1.Dispose();
				voxelResult.DataLod2.Dispose();
				voxelResult.DataLod3.Dispose();
			}

			keys.Dispose();
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

		public void Dispose()
		{
			WorldDataPositions.Dispose();
			WorldDataIndices.Dispose();
		}

		#endregion

		#region PrivateMethods


		#endregion
	}
}
