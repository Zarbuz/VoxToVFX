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
		}

		public void AddVoxels(NativeList<Vector4> voxels)
		{
			foreach (Vector4 vector4 in voxels)
			{
				FastMath.FloorToInt(vector4.x / CHUNK_SIZE, vector4.y / CHUNK_SIZE, vector4.z / CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);
				int chunkIndex = VoxImporter.GetGridPos(chunkX, chunkY, chunkZ, WorldVolume);
				int voxelIndex = VoxImporter.GetGridPos((int)vector4.x, (int)vector4.y, (int)vector4.z, ChunkVolume);
				if (vector4.y > 0)
				{
					NativeMultiHashMap<int, Vector4>.Enumerator enumerator = WorldDataPositions.GetValuesForKey(chunkIndex).GetEnumerator();
					bool canInsert = true;
					while (enumerator.MoveNext())
					{
						if (enumerator.Current == vector4)
						{
							canInsert = false;
							break;
						}
					}
					enumerator.Dispose();

					if (canInsert)
					{
						WorldDataPositions.Add(chunkIndex, vector4);
					}
				}
			}
		}

		public void ComputeLodsChunks(Action<float, VoxelResult> onChunkLoadedCallback)
		{
			(NativeArray<int>, int) keys = WorldDataPositions.GetUniqueKeyArrayNBC(Allocator.Temp);
			for (int index = 0; index < keys.Item2; index++)
			{
				NativeMultiHashMap<int, Vector4>.Enumerator enumerator= WorldDataPositions.GetValuesForKey(keys.Item1[index]).GetEnumerator();
				NativeHashMap<int, Vector4> data = new NativeHashMap<int, Vector4>(200, Allocator.Persistent);
				while (enumerator.MoveNext())
				{
					Vector4 voxel = enumerator.Current;
					int xFinal = (int)(voxel.x % CHUNK_SIZE);
					int yFinal = (int)(voxel.y % CHUNK_SIZE);
					int zFinal = (int)(voxel.z % CHUNK_SIZE);

					data[VoxImporter.GetGridPos(xFinal, yFinal, zFinal, ChunkVolume)] = voxel;
				}
				enumerator.Dispose();

				NativeHashMap<int, Vector4> resultLod1 = new NativeHashMap<int, Vector4>(200, Allocator.TempJob);
				ComputeLodJob computeLodJob = new ComputeLodJob()
				{
					VolumeSize = ChunkVolume,
					Result = resultLod1.AsParallelWriter(),
					Data = data,
					Step = 1,
					ModuloCheck = 2
				};
				JobHandle jobHandle = computeLodJob.Schedule(CHUNK_SIZE, 64);
				jobHandle.Complete();

				NativeHashMap<int, Vector4> resultLod2 = new NativeHashMap<int, Vector4>(200, Allocator.TempJob);
				ComputeLodJob computeLodJob2 = new ComputeLodJob()
				{
					VolumeSize = ChunkVolume,
					Result = resultLod2.AsParallelWriter(),
					Data = resultLod1,
					Step = 2,
					ModuloCheck = 4
				};
				JobHandle jobHandle2 = computeLodJob2.Schedule(CHUNK_SIZE, 64);
				jobHandle2.Complete();

				NativeHashMap<int, Vector4> resultLod3 = new NativeHashMap<int, Vector4>(200, Allocator.TempJob);
				ComputeLodJob computeLodJob3 = new ComputeLodJob()
				{
					VolumeSize = ChunkVolume,
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
					FrameWorldPosition = GetChunkWorldPosition(keys.Item1[index])
				};

				resultLod1.Dispose();
				resultLod2.Dispose();
				resultLod3.Dispose();
				data.Dispose();
				onChunkLoadedCallback?.Invoke(index / (float)keys.Item2, voxelResult);

				voxelResult.DataLod0.Dispose();
				voxelResult.DataLod1.Dispose();
				voxelResult.DataLod2.Dispose();
				voxelResult.DataLod3.Dispose();
			}

			keys.Item1.Dispose();
		}

		private Vector3 GetChunkWorldPosition(int chunkIndex)
		{
			return VoxImporter.Get3DPos(chunkIndex, WorldVolume); 
		}

		public void Dispose()
		{
			WorldDataPositions.Dispose();
		}

		#endregion

		#region PrivateMethods


		#endregion
	}
}
