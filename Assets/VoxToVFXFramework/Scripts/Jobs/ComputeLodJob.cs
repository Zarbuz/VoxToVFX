using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
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
		[ReadOnly] public Vector3 VolumeSize;
		[ReadOnly] public NativeHashMap<int, Vector4> Data;
		[NativeDisableParallelForRestriction]
		[WriteOnly] public NativeHashMap<int, Vector4>.ParallelWriter Result;
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
					//NativeArray<byte> work = new NativeArray<byte>(8, Allocator.Temp); 

					//work[0] = GetSafe(x, y, z, Data, VolumeSize);
					//work[1] = GetSafe(x1, y, z, Data, VolumeSize);
					//work[2] = GetSafe(x, y1, z, Data, VolumeSize);
					//work[3] = GetSafe(x1, y1, z, Data, VolumeSize);
					//work[4] = GetSafe(x, y, z1, Data, VolumeSize);
					//work[5] = GetSafe(x1, y, z1, Data, VolumeSize);
					//work[6] = GetSafe(x, y1, z1, Data, VolumeSize);
					//work[7] = GetSafe(x1, y1, z1, Data, VolumeSize);

					//NativeHashMap<byte, int> dictKeyValues = new NativeHashMap<byte, int>(8, Allocator.Temp);

					//byte keyMax = 0;
					//int max = 0;
					//for (int i = 0; i < work.Length; i++)
					//{
					//	byte val = work[i];
					//	if (val != 0)
					//	{
					//		if (dictKeyValues.ContainsKey(val))
					//		{
					//			dictKeyValues[val]++;
					//		}
					//		else
					//		{
					//			dictKeyValues.Add(val, 1);
					//		}

					//		if (dictKeyValues[val] > max)
					//		{
					//			max = dictKeyValues[val];
					//			keyMax = val;
					//		}
					//	}
					//}

					//if (keyMax != 0)
					//	Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = (byte)dictKeyValues[keyMax];
					//else
					//	Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = 0;
					//work.Dispose();
					//dictKeyValues.Dispose();

					int xFinal = x % WorldData.CHUNK_SIZE;
					int x1Final = x1 % WorldData.CHUNK_SIZE;

					int yFinal = y % WorldData.CHUNK_SIZE;
					int y1Final = y1 % WorldData.CHUNK_SIZE;

					int zFinal = z % WorldData.CHUNK_SIZE;
					int z1Final = z1 % WorldData.CHUNK_SIZE;

					int worldPositionKey = VoxImporter.GetGridPos(x, y, z, VolumeSize);
					int b0Key = VoxImporter.GetGridPos(xFinal, yFinal, zFinal, VolumeSize);
					int b1Key = VoxImporter.GetGridPos(x1Final, yFinal, zFinal, VolumeSize);
					int b2Key = VoxImporter.GetGridPos(xFinal, y1Final, zFinal, VolumeSize);
					int b3Key = VoxImporter.GetGridPos(x1Final, y1Final, zFinal, VolumeSize);
					int b4Key = VoxImporter.GetGridPos(xFinal, yFinal, z1Final, VolumeSize);
					int b5Key = VoxImporter.GetGridPos(x1Final, yFinal, z1Final, VolumeSize);
					int b6Key = VoxImporter.GetGridPos(xFinal, y1Final, z1Final, VolumeSize);
					int b7Key = VoxImporter.GetGridPos(x1Final, y1Final, z1Final, VolumeSize);
					if (Data.TryGetValue(b0Key, out Vector4 b0))
					{
						if (b0.w != 0) 
							Result.TryAdd(worldPositionKey, b0);
					}
					else if (Data.TryGetValue(b1Key, out Vector4 b1))
					{
						if (b1.w != 0) 
							Result.TryAdd(worldPositionKey, b1);
					}
					else if (Data.TryGetValue(b2Key, out Vector4 b2))
					{
						if (b2.w != 0)
							Result.TryAdd(worldPositionKey, b2);
					}
					else if (Data.TryGetValue(b3Key, out Vector4 b3))
					{
						if (b3.w != 0)
							Result.TryAdd(worldPositionKey, b3);
					}
					else if (Data.TryGetValue(b4Key, out Vector4 b4))
					{
						if (b4.w != 0)
							Result.TryAdd(worldPositionKey, b4);
					}
					else if (Data.TryGetValue(b5Key, out Vector4 b5))
					{
						if (b5.w != 0)
							Result.TryAdd(worldPositionKey, b5);
					}
					else if (Data.TryGetValue(b6Key, out Vector4 b6))
					{
						if (b6.w != 0)
							Result.TryAdd(worldPositionKey, b6);
					}
					else if (Data.TryGetValue(b7Key, out Vector4 b7))
					{
						if (b7.w != 0)
							Result.TryAdd(worldPositionKey, b7);
					}
					else
					{
						Result.TryAdd(worldPositionKey, Vector4.zero);
					}


					//if (work.Any(color => color != 0))
					//{
					//	IOrderedEnumerable<IGrouping<byte, byte>> groups = work.Where(color => color != 0)
					//		.GroupBy(v => v).OrderByDescending(v => v.Count());
					//	int count = groups.ElementAt(0).Count();
					//	IGrouping<byte, byte> group = groups.TakeWhile(v => v.Count() == count)
					//		.OrderByDescending(v => v.Key).First();

					//	Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = group.Key;
					//}
					//else
					//{
					//	Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = 0;
					//}
				}
			}
		}

		public static bool Contains(int x, int y, int z, Vector3 volumeSize)
			=> x >= 0 && y >= 0 && z >= 0 && x < volumeSize.x && y < volumeSize.y && z < volumeSize.z;
		public static byte GetSafe(int x, int y, int z, NativeArray<byte> data, Vector3 volumeSize)
			=> Contains(x, y, z, volumeSize) ? data[VoxImporter.GetGridPos(x, y, z, volumeSize)] : (byte)0;
	}
}
