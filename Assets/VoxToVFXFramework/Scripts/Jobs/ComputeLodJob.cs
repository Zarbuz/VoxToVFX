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
		[ReadOnly] public UnsafeHashMap<int, VoxelData> Data;
		public UnsafeHashMap<int, VoxelData>.ParallelWriter Result;
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
					int b1Key = VoxImporter.GetGridPos(x1, y, z, VolumeSize);
					int b2Key = VoxImporter.GetGridPos(x, y1, z, VolumeSize);
					int b3Key = VoxImporter.GetGridPos(x1, y1, z, VolumeSize);
					int b4Key = VoxImporter.GetGridPos(x, y, z1, VolumeSize);
					int b5Key = VoxImporter.GetGridPos(x1, y, z1, VolumeSize);
					int b6Key = VoxImporter.GetGridPos(x, y1, z1, VolumeSize);
					int b7Key = VoxImporter.GetGridPos(x1, y1, z1, VolumeSize);

					if (Data.TryGetValue(worldPositionKey, out VoxelData b0) && b0.ColorIndex != 0)
					{
						Result.TryAdd(worldPositionKey, new VoxelData((byte)x, (byte)y, (byte)z, b0.ColorIndex));
					}
					else if (Data.TryGetValue(b1Key, out VoxelData b1) && b1.ColorIndex != 0)
					{
						Result.TryAdd(worldPositionKey, new VoxelData((byte)x, (byte)y, (byte)z, b1.ColorIndex));
					}
					else if (Data.TryGetValue(b2Key, out VoxelData b2) && b2.ColorIndex != 0)
					{
						Result.TryAdd(worldPositionKey, new VoxelData((byte)x, (byte)y, (byte)z, b2.ColorIndex));
					}
					else if (Data.TryGetValue(b3Key, out VoxelData b3) && b3.ColorIndex != 0)
					{
						Result.TryAdd(worldPositionKey, new VoxelData((byte)x, (byte)y, (byte)z, b3.ColorIndex));
					}
					else if (Data.TryGetValue(b4Key, out VoxelData b4) && b4.ColorIndex != 0)
					{
						Result.TryAdd(worldPositionKey, new VoxelData((byte)x, (byte)y, (byte)z, b4.ColorIndex));
					}
					else if (Data.TryGetValue(b5Key, out VoxelData b5) && b5.ColorIndex != 0)
					{
						Result.TryAdd(worldPositionKey, new VoxelData((byte)x, (byte)y, (byte)z, b5.ColorIndex));
					}
					else if (Data.TryGetValue(b6Key, out VoxelData b6) && b6.ColorIndex != 0)
					{
						Result.TryAdd(worldPositionKey, new VoxelData((byte)x, (byte)y, (byte)z, b6.ColorIndex));
					}
					else if (Data.TryGetValue(b7Key, out VoxelData b7) && b7.ColorIndex != 0)
					{
						Result.TryAdd(worldPositionKey, new VoxelData((byte)x, (byte)y, (byte)z, b7.ColorIndex));
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
