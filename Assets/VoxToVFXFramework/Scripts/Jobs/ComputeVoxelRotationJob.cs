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

		/*
		 * +--------+
		  /        /|
		 /    1   / |
		+--------+  |
		|        | 2|
		|        |  +
		|        | /
		|        |/
		+--------+
		 *
		 * Top: 1
		 * Right: 2
		 * Bottom: 3
		 * Left: 4
		 * Front: 5
		 * Back: 6
		 */
		public void Execute(int index)
		{
			int key = Keys[index];
			VoxelData v = Data[key];
			VoxelFace voxelFace = VoxelFace.None;
			bool left = Data.ContainsKey(VoxImporter.GetGridPos(v.PosX - Step, v.PosY, v.PosZ, VolumeSize));
			bool right = Data.ContainsKey(VoxImporter.GetGridPos(v.PosX + Step, v.PosY, v.PosZ, VolumeSize));
			bool top = Data.ContainsKey(VoxImporter.GetGridPos(v.PosX, v.PosY + Step, v.PosZ, VolumeSize));
			bool bottom = Data.ContainsKey(VoxImporter.GetGridPos(v.PosX, v.PosY - Step, v.PosZ, VolumeSize));
			bool front = Data.ContainsKey(VoxImporter.GetGridPos(v.PosX, v.PosY, v.PosZ + Step, VolumeSize));
			bool back = Data.ContainsKey(VoxImporter.GetGridPos(v.PosX, v.PosY, v.PosZ - Step, VolumeSize));

			if (!left)
			{
				voxelFace = VoxelFace.Left;
			}

			if (!right)
			{
				voxelFace |= VoxelFace.Right;
			}

			if (!top)
			{
				voxelFace |= VoxelFace.Top;
			}

			if (!bottom)
			{
				voxelFace |= VoxelFace.Bottom;
			}

			if (!front)
			{
				voxelFace |= VoxelFace.Front;
			}

			if (!back)
			{
				voxelFace |= VoxelFace.Back;
			}

			v.Face = voxelFace;

			Result.AddNoResize(v);
		}

	}
}
