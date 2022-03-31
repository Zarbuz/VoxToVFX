using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxToVFXFramework.Scripts.Data;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct ComputeRenderingChunkJob : IJobParallelFor
	{
		[ReadOnly] public NativeList<Chunk> Chunks;
		[ReadOnly] public Vector4 LodDistance;
		[ReadOnly] public int ForcedLevelLod;
		[ReadOnly] public NativeMultiHashMap<int, VoxelVFX> Data; 
		[ReadOnly] public Vector3 CameraPosition;
		public NativeList<VoxelVFX>.ParallelWriter Buffer;

		public void Execute(int index)
		{
			Chunk chunk = Chunks[index];
			
			if (chunk.IsActive == 0)
				return;

			float distance = Vector3.Distance(CameraPosition, chunk.Position);
			if ((distance >= LodDistance.x && distance < LodDistance.y && ForcedLevelLod == -1 || ForcedLevelLod == 0) && chunk.LodLevel == 1)
			{
				NativeMultiHashMap<int, VoxelVFX>.Enumerator enumerator = Data.GetValuesForKey(chunk.ChunkIndex).GetEnumerator();
				AddToBuffer(enumerator, 1);
				enumerator.Dispose();
			}
			else if ((distance >= LodDistance.y && distance < LodDistance.z && ForcedLevelLod == -1 || ForcedLevelLod == 1) && chunk.LodLevel == 2)
			{
				NativeMultiHashMap<int, VoxelVFX>.Enumerator enumerator = Data.GetValuesForKey(chunk.ChunkIndex).GetEnumerator();
				AddToBuffer(enumerator, 2);
				enumerator.Dispose();
			}
			else if ((distance >= LodDistance.z && distance < LodDistance.w && ForcedLevelLod == -1 || ForcedLevelLod == 2) && chunk.LodLevel == 4)
			{
				NativeMultiHashMap<int, VoxelVFX>.Enumerator enumerator = Data.GetValuesForKey(chunk.ChunkIndex).GetEnumerator();
				AddToBuffer(enumerator, 4);
				enumerator.Dispose();
			}
			else if ((distance >= LodDistance.w && distance < int.MaxValue && ForcedLevelLod == -1 || ForcedLevelLod == 3) && chunk.LodLevel == 8)
			{
				NativeMultiHashMap<int, VoxelVFX>.Enumerator enumerator = Data.GetValuesForKey(chunk.ChunkIndex).GetEnumerator();
				AddToBuffer(enumerator, 8);
				enumerator.Dispose();
			}
		}

		private void AddToBuffer(NativeMultiHashMap<int, VoxelVFX>.Enumerator enumerator, int lodLevel)
		{
			while (enumerator.MoveNext())
			{
				VoxelVFX voxel = enumerator.Current;
				if (voxel.lodLevel == lodLevel)
				{
					Buffer.AddNoResize(voxel);
				}
			}
		}
	}
}
