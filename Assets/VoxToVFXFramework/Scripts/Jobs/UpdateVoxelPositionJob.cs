using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxToVFXFramework.Scripts.Common;
using VoxToVFXFramework.Scripts.Importer;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct UpdateVoxelPositionJob : IJobParallelFor
	{
		[ReadOnly] public Vector3 Pivot;
		[ReadOnly] public Vector3 FPivot;
		[ReadOnly] public Matrix4x4 Matrix4X4;
		[ReadOnly] public Vector3 VolumeSize;
		[ReadOnly] public NativeArray<int> Keys;
		[ReadOnly] public NativeHashMap<int, Vector4> HashMap;
		[WriteOnly] public NativeArray<int> RotationArray;
		public NativeArray<Vector4> Result;

		public void Execute(int index)
		{
			Vector4 v = HashMap[Keys[index]];

			bool left = HashMap.ContainsKey(VoxImporter.GetGridPos((int)v.x - 1, (int)v.y, (int)v.z, VolumeSize));
			bool right = HashMap.ContainsKey(VoxImporter.GetGridPos((int)v.x + 1, (int)v.y, (int)v.z, VolumeSize));
			bool top = HashMap.ContainsKey(VoxImporter.GetGridPos((int)v.x, (int)v.y + 1, (int)v.z, VolumeSize));
			bool bottom = HashMap.ContainsKey(VoxImporter.GetGridPos((int)v.x, (int)v.y - 1, (int)v.z, VolumeSize));
			bool front = HashMap.ContainsKey(VoxImporter.GetGridPos((int)v.x, (int)v.y, (int)v.z + 1, VolumeSize));
			bool back = HashMap.ContainsKey(VoxImporter.GetGridPos((int)v.x, (int)v.y, (int)v.z - 1, VolumeSize));

			if (left && right && front && back)
			{
				RotationArray[index] = 1;
			}
			else if (left && right && top && bottom)
			{
				RotationArray[index] = 2;
			}
			else if (front && back && top && bottom)
			{
				RotationArray[index] = 3;
			}

			IntVector3 tmpVoxel = GetVoxPosition(VolumeSize, (int)v.x, (int)v.y, (int)v.z, Pivot, FPivot, Matrix4X4);
			Result[index] = new Vector4(tmpVoxel.x + 1000, tmpVoxel.y + 1000, tmpVoxel.z + 1000, v.w - 1);
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
