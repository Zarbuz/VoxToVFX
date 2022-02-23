using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct ComputeLodJob : IJobParallelFor
	{
		[ReadOnly] public int Step;
		[ReadOnly] public int ModuloCheck;
		[ReadOnly] public int3 VolumeSize;
		[ReadOnly] public int3 WorldChunkPosition;
		[ReadOnly] public NativeHashMap<int, int4> Data;
		public NativeHashMap<int, int4>.ParallelWriter Result;
		public void Execute(int z)
		{
			if (z % ModuloCheck != 0)
			{
				return;
			}

			int z1 = z + Step;
			for (int y = 0; y < VolumeSize.y; y += Step * 2)
			{
				int y1 = y + Step;
				for (int x = 0; x < VolumeSize.x; x += Step * 2)
				{
					int x1 = x + Step;
					int worldPositionKey = VoxImporter.GetGridPos(x, y, z, VolumeSize);
					int b0Key = VoxImporter.GetGridPos(x, y, z, VolumeSize);
					int b1Key = VoxImporter.GetGridPos(x1, y, z, VolumeSize);
					int b2Key = VoxImporter.GetGridPos(x, y1, z, VolumeSize);
					int b3Key = VoxImporter.GetGridPos(x1, y1, z, VolumeSize);
					int b4Key = VoxImporter.GetGridPos(x, y, z1, VolumeSize);
					int b5Key = VoxImporter.GetGridPos(x1, y, z1, VolumeSize);
					int b6Key = VoxImporter.GetGridPos(x, y1, z1, VolumeSize);
					int b7Key = VoxImporter.GetGridPos(x1, y1, z1, VolumeSize);
					if (Data.TryGetValue(b0Key, out int4 b0))
					{
						if (b0.w != 0)
							Result.TryAdd(worldPositionKey, new int4(x + WorldChunkPosition.x, y + WorldChunkPosition.y, z + WorldChunkPosition.z, b0.w));
					}
					else if (Data.TryGetValue(b1Key, out int4 b1))
					{
						if (b1.w != 0) 
							Result.TryAdd(worldPositionKey, new int4(x + WorldChunkPosition.x, y + WorldChunkPosition.y, z + WorldChunkPosition.z, b1.w));
					}
					else if (Data.TryGetValue(b2Key, out int4 b2))
					{
						if (b2.w != 0)
							Result.TryAdd(worldPositionKey, new int4(x + WorldChunkPosition.x, y + WorldChunkPosition.y, z + WorldChunkPosition.z, b2.w));
					}
					else if (Data.TryGetValue(b3Key, out int4 b3))
					{
						if (b3.w != 0)
							Result.TryAdd(worldPositionKey, new int4(x + WorldChunkPosition.x, y + WorldChunkPosition.y, z + WorldChunkPosition.z, b3.w));
					}
					else if (Data.TryGetValue(b4Key, out int4 b4))
					{
						if (b4.w != 0)
							Result.TryAdd(worldPositionKey, new int4(x + WorldChunkPosition.x, y + WorldChunkPosition.y, z + WorldChunkPosition.z, b4.w));
					}
					else if (Data.TryGetValue(b5Key, out int4 b5))
					{
						if (b5.w != 0)
							Result.TryAdd(worldPositionKey, new int4(x + WorldChunkPosition.x, y + WorldChunkPosition.y, z + WorldChunkPosition.z, b5.w));
					}
					else if (Data.TryGetValue(b6Key, out int4 b6))
					{
						if (b6.w != 0)
							Result.TryAdd(worldPositionKey, new int4(x + WorldChunkPosition.x, y + WorldChunkPosition.y, z + WorldChunkPosition.z, b6.w));
					}
					else if (Data.TryGetValue(b7Key, out int4 b7))
					{
						if (b7.w != 0)
							Result.TryAdd(worldPositionKey, new int4(x + WorldChunkPosition.x, y + WorldChunkPosition.y, z + WorldChunkPosition.z, b7.w));
					}

				}
			}
		}

		public static bool Contains(int x, int y, int z, int3 volumeSize)
			=> x >= 0 && y >= 0 && z >= 0 && x < volumeSize.x && y < volumeSize.y && z < volumeSize.z;
		public static byte GetSafe(int x, int y, int z, NativeArray<byte> data, int3 volumeSize)
			=> Contains(x, y, z, volumeSize) ? data[VoxImporter.GetGridPos(x, y, z, volumeSize)] : (byte)0;
	}
}
