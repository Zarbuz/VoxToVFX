using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using VoxToVFXFramework.Scripts.Importer;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct ComputeLodJob : IJobParallelFor
	{
		[ReadOnly] public int Step;
		[ReadOnly] public int ModuloCheck;
		[ReadOnly] public Vector3 VolumeSize;
		[ReadOnly] public NativeArray<byte> Data;
		[NativeDisableParallelForRestriction]
		[WriteOnly] public NativeArray<byte> Result;
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

					byte b0 = GetSafe(x, y, z, Data, VolumeSize);
					byte b1 = GetSafe(x1, y, z, Data, VolumeSize);
					byte b2 = GetSafe(x, y1, z, Data, VolumeSize);
					byte b3 = GetSafe(x1, y1, z, Data, VolumeSize);
					byte b4 = GetSafe(x, y, z1, Data, VolumeSize);
					byte b5 = GetSafe(x1, y, z1, Data, VolumeSize);
					byte b6 = GetSafe(x, y1, z1, Data, VolumeSize);
					byte b7 = GetSafe(x1, y1, z1, Data, VolumeSize);

					if (b0 != 0)
						Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = b0;
					else if (b1 != 0)
						Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = b1;
					else if (b2 != 0)
						Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = b2;
					else if (b3 != 0)
						Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = b3;
					else if (b4 != 0)
						Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = b4;
					else if (b5 != 0)
						Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = b5;
					else if (b6 != 0)
						Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = b6;
					else if (b7 != 0)
						Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = b7;
					else
						Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = 0;
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
