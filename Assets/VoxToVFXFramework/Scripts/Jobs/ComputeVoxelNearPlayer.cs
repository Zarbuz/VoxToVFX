using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Extensions;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct ComputeVoxelNearPlayer : IJobParallelFor
	{
		[ReadOnly] public UnsafeList<VoxelVFX> Data;
		[ReadOnly] public float3 PlayerPosition;
		[ReadOnly] public float3 ChunkWorldPosition;
		[ReadOnly] public float DistanceCheckVoxels;

		public NativeList<int>.ParallelWriter Buffer;
		public void Execute(int index)
		{
			VoxelVFX voxel = Data[index];
			float4 voxelPosition = voxel.DecodePosition();
			float3 worldVoxelPosition = new float3(ChunkWorldPosition.x + voxelPosition.x,
				ChunkWorldPosition.y + voxelPosition.y, ChunkWorldPosition.z + voxelPosition.z);
			if (math.distance(PlayerPosition, worldVoxelPosition) < DistanceCheckVoxels)
			{
				Buffer.AddNoResize(index);
			}
		}

	}
}
