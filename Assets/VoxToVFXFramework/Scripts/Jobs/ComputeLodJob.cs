using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxToVFXFramework.Scripts.Importer;

namespace VoxToVFXFramework.Scripts.Jobs
{
	//[BurstCompile]
	public struct ComputeLodJob : IJobParallelFor
	{
		[ReadOnly] public int Step;
		[ReadOnly] public Vector3 VolumeSize;
		[ReadOnly] public NativeArray<byte> Data;
		[NativeDisableParallelForRestriction]
		[WriteOnly] public NativeArray<byte> Result;
		public void Execute(int z) 
		{
			if (z % 2 != 0)
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
					NativeArray<byte> work = new NativeArray<byte>(8, Allocator.Temp); 

					work[0] = GetSafe(x, y, z, Data, VolumeSize);
					work[1] = GetSafe(x1, y, z, Data, VolumeSize);
					work[2] = GetSafe(x, y1, z, Data, VolumeSize);
					work[3] = GetSafe(x1, y1, z, Data, VolumeSize);
					work[4] = GetSafe(x, y, z1, Data, VolumeSize);
					work[5] = GetSafe(x1, y, z1, Data, VolumeSize);
					work[6] = GetSafe(x, y1, z1, Data, VolumeSize);
					work[7] = GetSafe(x1, y1, z1, Data, VolumeSize);

					if (work.Any(color => color != 0))
					{
						IOrderedEnumerable<IGrouping<byte, byte>> groups = work.Where(color => color != 0)
							.GroupBy(v => v).OrderByDescending(v => v.Count());
						int count = groups.ElementAt(0).Count();
						IGrouping<byte, byte> group = groups.TakeWhile(v => v.Count() == count)
							.OrderByDescending(v => v.Key).First();

						Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = group.Key;
					}
					else
					{
						Result[VoxImporter.GetGridPos(x, y, z, VolumeSize)] = 0;
					}

					work.Dispose();
				}
			}
		}

		public static bool Contains(int x, int y, int z, Vector3 volumeSize)
			=> x >= 0 && y >= 0 && z >= 0 && x < volumeSize.x && y < volumeSize.y && z < volumeSize.z;
		public static byte GetSafe(int x, int y, int z, NativeArray<byte> data, Vector3 volumeSize)
			=> Contains(x, y, z, volumeSize) ? data[VoxImporter.GetGridPos(x, y, z, volumeSize)] : (byte)0;
	}
}
