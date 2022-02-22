using FileToVoxCore.Utils;
using System;
using FileToVoxCore.Schematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections.NotBurstCompatible;
using Unity.Jobs;
using UnityEngine;
using VoxToVFXFramework.Scripts.Importer;
using VoxToVFXFramework.Scripts.Jobs;

namespace VoxToVFXFramework.Scripts.Data
{
	public class WorldData : IDisposable
	{
		#region Fields

		public NativeMultiHashMap<int, Vector4> WorldDataPositions;
		public NativeHashMap<int, int> WorldDataIndices;
		#endregion

		#region ConstStatic

		public const int CHUNK_SIZE = 500;
		public static Vector3 ChunkVolume = new Vector3(CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE);
		public static Vector3 WorldVolume = new Vector3(Schematic.MAX_WORLD_WIDTH, Schematic.MAX_WORLD_HEIGHT, Schematic.MAX_WORLD_LENGTH);
		#endregion

		#region PublicMethods

		public WorldData()
		{
			WorldDataPositions = new NativeMultiHashMap<int, Vector4>(256, Allocator.Persistent); //double capacity strategy
			WorldDataIndices = new NativeHashMap<int, int>(256, Allocator.Persistent);
		}

		public void AddVoxels(NativeList<Vector4> voxels)
		{
			foreach (Vector4 vector4 in voxels)
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
				NativeMultiHashMap<int, Vector4>.Enumerator enumerator = WorldDataPositions.GetValuesForKey(chunkIndex).GetEnumerator();
				NativeHashMap<int, Vector4> data = new NativeHashMap<int, Vector4>(200, Allocator.Persistent);
				Vector3 worldChunkPosition = GetChunkWorldPosition(chunkIndex);
				while (enumerator.MoveNext())
				{
					Vector4 voxel = enumerator.Current;
					int xFinal = (int)(voxel.x % CHUNK_SIZE);
					int yFinal = (int)(voxel.y % CHUNK_SIZE);
					int zFinal = (int)(voxel.z % CHUNK_SIZE);

					data[VoxImporter.GetGridPos(xFinal, yFinal, zFinal, ChunkVolume)] = voxel;
				}
				enumerator.Dispose();

				NativeHashMap<int, Vector4> resultLod1 = new NativeHashMap<int, Vector4>(data.Count(), Allocator.TempJob);
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

				NativeHashMap<int, Vector4> resultLod2 = new NativeHashMap<int, Vector4>(resultLod1.Count(), Allocator.TempJob);
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

				NativeHashMap<int, Vector4> resultLod3 = new NativeHashMap<int, Vector4>(resultLod2.Count(), Allocator.TempJob);
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

				VoxelResult voxelResult = new VoxelResult
				{
					DataLod0 = data.GetValueArray(Allocator.Persistent),
					DataLod1 = resultLod1.GetValueArray(Allocator.Persistent),
					DataLod2 = resultLod2.GetValueArray(Allocator.Persistent),
					DataLod3 = resultLod3.GetValueArray(Allocator.Persistent),
					FrameWorldPosition = worldChunkPosition
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

		private Vector3 GetChunkWorldPosition(int chunkIndex)
		{
			Vector3 pos3d = VoxImporter.Get3DPos(chunkIndex, WorldVolume);
			return pos3d * CHUNK_SIZE;
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
