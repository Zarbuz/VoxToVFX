using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct ComputeVoxelPositionJob : IJobParallelFor
	{
		[ReadOnly] public float3 Pivot;
		[ReadOnly] public float3 FPivot;
		[ReadOnly] public Matrix4x4 Matrix4X4;
		[ReadOnly] public int3 VolumeSize;
		[ReadOnly] public NativeArray<byte> Data;
		[WriteOnly] public NativeList<int4>.ParallelWriter Result;

		public void Execute(int z)
		{
			for (int y = 0; y < VolumeSize.y; y++)
			{
				for (int x = 0; x < VolumeSize.x; x++)
				{
					byte color = ComputeLodJob.GetSafe(x, y, z, Data, VolumeSize);
					if (color != 0)
					{
						int3 worldPosition = GetVoxPosition(VolumeSize, x, y, z, Pivot, FPivot, Matrix4X4);
						Result.AddNoResize(new int4()
						{
							x = worldPosition.x + 1000,
							y = worldPosition.y + 1000,
							z = worldPosition.z + 1000,
							w = color - 1
						});
						//Result[index] = new Vector4(tmpVoxel.x + 1000, tmpVoxel.y + 1000, tmpVoxel.z + 1000, v.w - 1);
					}
				}
			}
			
		}

		public static int3 GetVoxPosition(int3 size, int x, int y, int z, float3 pivot, float3 fpivot, Matrix4x4 matrix4X4)
		{
			int3 tmpVoxel = new int3(x, y, z);
			int3 origPos;

			origPos.x = size.x - 1 - tmpVoxel.x; //invert
			origPos.y = size.z - 1 - tmpVoxel.z; //swapYZ //invert
			origPos.z = tmpVoxel.y;

			float3 pos = new(origPos.x + 0.5f, origPos.y + 0.5f, origPos.z + 0.5f);
			pos -= pivot;
			pos = matrix4X4.MultiplyPoint(pos);
			pos += pivot;

			pos.x += fpivot.x;
			pos.y += fpivot.y;
			pos.z -= fpivot.z;

			origPos.x = Mathf.FloorToInt(pos.x);
			origPos.y = Mathf.FloorToInt(pos.y);
			origPos.z = Mathf.FloorToInt(pos.z);

			tmpVoxel.x = size.x - 1 - origPos.x; //invert
			tmpVoxel.z = size.z - 1 - origPos.y; //swapYZ  //invert
			tmpVoxel.y = origPos.z;

			return tmpVoxel;
		}
	}
}
