using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Managers;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct ComputeRenderingChunkJob : IJobParallelFor
	{
		[ReadOnly] public NativeList<int> ChunkIndex;
		[ReadOnly] public NativeArray<ChunkVFX> Chunks;
		[ReadOnly] public Vector3 LodDistance;
		[ReadOnly] public int ForcedLevelLod;
		[ReadOnly] public UnsafeParallelHashMap<int, UnsafeList<VoxelVFX>> Data;
		[ReadOnly] public Vector3 CameraPosition;
		public NativeList<VoxelVFX>.ParallelWriter Buffer;


		public void Execute(int index)
		{
			ChunkVFX chunkVFX = Chunks[index];
			int chunkIndex = ChunkIndex[index];
			//if (chunk.IsActive == 0)
			//	return;


			float distance = Vector3.Distance(CameraPosition, chunkVFX.CenterWorldPosition);
			if ((distance >= LodDistance.x && distance < LodDistance.y && ForcedLevelLod == -1 || ForcedLevelLod == 0) && chunkVFX.LodLevel == 1)
			{
				Buffer.AddRangeNoResize(Data[chunkIndex]);
			}
			else if ((distance >= LodDistance.y && distance < LodDistance.z && ForcedLevelLod == -1 || ForcedLevelLod == 1) && chunkVFX.LodLevel == 2)
			{
				Buffer.AddRangeNoResize(Data[chunkIndex]);
			}
			else if ((distance >= LodDistance.z && distance < int.MaxValue && ForcedLevelLod == -1 || ForcedLevelLod == 2) && chunkVFX.LodLevel == 4)
			{
				Buffer.AddRangeNoResize(Data[chunkIndex]);
			}

			//else if ((distance >= LodDistance.w && distance < int.MaxValue && ForcedLevelLod == -1 || ForcedLevelLod == 3) && chunk.LodLevel == 8)
			//{
			//	Buffer.AddRangeNoResize(Data[RuntimeVoxManager.GetUniqueChunkIndexWithLodLevel(chunk.ChunkIndex, chunk.LodLevel)]);
			//}
		}

	}
}
