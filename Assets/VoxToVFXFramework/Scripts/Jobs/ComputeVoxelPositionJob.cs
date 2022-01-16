using FileToVoxCore.Vox;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxToVFXFramework.Scripts.Common;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct ComputeVoxelPositionJob : IJobParallelFor
	{
		[ReadOnly] public Vector3 Pivot;
		[ReadOnly] public Vector3 FPivot;
		[ReadOnly] public Matrix4x4 Matrix4X4;
		[ReadOnly] public Vector3 VolumeSize;
		[ReadOnly] public Vector3 InitialVolumeSize;
		[ReadOnly] public NativeArray<byte> Data;
		[WriteOnly] public NativeList<Vector4>.ParallelWriter Result;

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
						IntVector3 worldPosition = GetVoxPosition(InitialVolumeSize, x, y, z, Pivot, FPivot, Matrix4X4);
						Result.AddNoResize(new Vector4()
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

		private static IntVector3 GetVoxPosition(Vector3 size, int x, int y, int z, Vector3 pivot, Vector3 fpivot, Matrix4x4 matrix4X4)
		{
			IntVector3 tmpVoxel = new IntVector3(x, y, z);
			IntVector3 origPos;

			origPos.x = (int)(size.x - 1 - tmpVoxel.x); //invert
			origPos.y = (int)(size.z - 1 - tmpVoxel.z); //swapYZ //invert
			origPos.z = tmpVoxel.y;

			Vector3 pos = new(origPos.x + 0.5f, origPos.y + 0.5f, origPos.z + 0.5f);
			pos -= pivot;
			pos = matrix4X4.MultiplyPoint(pos);
			pos += pivot;

			pos.x += fpivot.x;
			pos.y += fpivot.y;
			pos.z -= fpivot.z;

			origPos.x = Mathf.FloorToInt(pos.x);
			origPos.y = Mathf.FloorToInt(pos.y);
			origPos.z = Mathf.FloorToInt(pos.z);

			tmpVoxel.x = (int)(size.x - 1 - origPos.x); //invert
			tmpVoxel.z = (int)(size.z - 1 - origPos.y); //swapYZ  //invert
			tmpVoxel.y = origPos.z;

			return tmpVoxel;
		}
	}
}
