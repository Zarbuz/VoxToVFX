using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct ComputeVoxelRotationJob : IJobParallelFor
	{
		[ReadOnly] public NativeArray<int> Keys;
		[ReadOnly] public UnsafeHashMap<int, VoxelData> Data;
		[ReadOnly] public int Step;

		[ReadOnly] public int3 VolumeSize;

		public NativeList<VoxelData>.ParallelWriter Result;

		public void Execute(int index)
		{
			int key = Keys[index];
			VoxelData v = Data[key];
			byte rotationIndex = 0;
			bool left = Data.ContainsKey(VoxImporter.GetGridPos(v.PosX - Step, v.PosY, v.PosZ, VolumeSize));
			bool right = Data.ContainsKey(VoxImporter.GetGridPos(v.PosX + Step, v.PosY, v.PosZ, VolumeSize));
			bool top = Data.ContainsKey(VoxImporter.GetGridPos(v.PosX, v.PosY + Step, v.PosZ, VolumeSize));
			bool bottom = Data.ContainsKey(VoxImporter.GetGridPos(v.PosX, v.PosY - Step, v.PosZ, VolumeSize));
			bool front = Data.ContainsKey(VoxImporter.GetGridPos(v.PosX, v.PosY, v.PosZ + Step, VolumeSize));
			bool back = Data.ContainsKey(VoxImporter.GetGridPos(v.PosX, v.PosY, v.PosZ - Step, VolumeSize));

			if (left && right && front && back)
			{
				rotationIndex = 1;
			}
			else if (left && right && top && bottom)
			{
				rotationIndex = 2;
			}
			else if (front && back && top && bottom)
			{
				rotationIndex = 3;
			}

			v.RotationIndex = rotationIndex;
			Result.AddNoResize(v);
		}

	}
}
