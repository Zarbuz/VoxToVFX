using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxToVFXFramework.Scripts.Importer;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct RemoveInvisibleVoxelJob : IJobParallelFor
	{
		[ReadOnly] public int3 VolumeSize;
		[ReadOnly] public NativeArray<byte> Data;
		[NativeDisableParallelForRestriction]
		[WriteOnly] public NativeArray<byte> Result;

		public void Execute(int z)
		{
			for (int y = 0; y < VolumeSize.y; y++)
			{
				for (int x = 0; x < VolumeSize.x; x++)
				{
					int index = VoxImporter.GetGridPos(x, y, z, VolumeSize);
					byte color = Data[index];
					if (color != 0)
					{
						byte right = ComputeLodJob.GetSafe(x - 1, y, z, Data, VolumeSize);
						byte left = ComputeLodJob.GetSafe(x + 1, y, z, Data, VolumeSize);
						byte bottom = ComputeLodJob.GetSafe(x, y - 1, z, Data, VolumeSize);
						byte top = ComputeLodJob.GetSafe(x, y + 1, z, Data, VolumeSize);
						byte front = ComputeLodJob.GetSafe(x, y, z - 1, Data, VolumeSize);
						byte back = ComputeLodJob.GetSafe(x, y, z + 1, Data, VolumeSize);

						if (right == 0 || left == 0 || bottom == 0 || top == 0 || front == 0 || back == 0)
						{
							Result[index] = color;
						}
						else
						{
							Result[index] = 0;
						}
					}
				}
			}
		}
	}
}
