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
		[ReadOnly] public NativeArray<VoxelMaterialVFX> Materials;
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
			bool left = Data.TryGetValue(VoxImporter.GetGridPos(v.PosX - Step, v.PosY, v.PosZ, VolumeSize), out VoxelData vLeft);
			bool right = Data.TryGetValue(VoxImporter.GetGridPos(v.PosX + Step, v.PosY, v.PosZ, VolumeSize), out VoxelData vRight);
			bool top = Data.TryGetValue(VoxImporter.GetGridPos(v.PosX, v.PosY + Step, v.PosZ, VolumeSize), out VoxelData vTop);
			bool bottom = Data.TryGetValue(VoxImporter.GetGridPos(v.PosX, v.PosY - Step, v.PosZ, VolumeSize), out VoxelData vBottom);
			bool front = Data.TryGetValue(VoxImporter.GetGridPos(v.PosX, v.PosY, v.PosZ + Step, VolumeSize), out VoxelData vFront);
			bool back = Data.TryGetValue(VoxImporter.GetGridPos(v.PosX, v.PosY, v.PosZ - Step, VolumeSize), out VoxelData vBack);

			bool isCurrentTransparent = IsTransparent(v.ColorIndex);

			if (!left || isCurrentTransparent && !IsTransparent(vLeft.ColorIndex) || !isCurrentTransparent && IsTransparent(vLeft.ColorIndex))
			{
				voxelFace = VoxelFace.Left;
			}

			if (!right || isCurrentTransparent && !IsTransparent(vRight.ColorIndex) || !isCurrentTransparent && IsTransparent(vRight.ColorIndex))
			{
				voxelFace |= VoxelFace.Right;
			}

			if (!top || isCurrentTransparent && !IsTransparent(vTop.ColorIndex) || !isCurrentTransparent && IsTransparent(vTop.ColorIndex))
			{
				voxelFace |= VoxelFace.Top;
			}

			if (!bottom || isCurrentTransparent && !IsTransparent(vBottom.ColorIndex) || !isCurrentTransparent && IsTransparent(vBottom.ColorIndex))
			{
				voxelFace |= VoxelFace.Bottom;
			}

			if (!front || isCurrentTransparent && !IsTransparent(vFront.ColorIndex) || !isCurrentTransparent && IsTransparent(vFront.ColorIndex))
			{
				voxelFace |= VoxelFace.Front;
			}

			if (!back || isCurrentTransparent && !IsTransparent(vBack.ColorIndex) || !isCurrentTransparent && IsTransparent(vBack.ColorIndex))
			{
				voxelFace |= VoxelFace.Back;
			}

			v.Face = voxelFace;

			Result.AddNoResize(v);
		}

		private bool IsTransparent(byte colorIndex)
		{
			return Materials[colorIndex].alpha < 1;
		}

	}
}
