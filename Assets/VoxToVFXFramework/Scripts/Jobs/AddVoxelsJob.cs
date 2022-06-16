using FileToVoxCore.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Importer;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct AddVoxelsJob : IJobFor
	{
		[ReadOnly] public NativeList<Vector4> Voxels;
		public UnsafeHashMap<int, UnsafeHashMap<int, VoxelData>> WorldDataPositions;

		public void Execute(int index)
		{
			Vector4 voxel = Voxels[index];
			FastMath.FloorToInt(voxel.x / WorldData.CHUNK_SIZE, voxel.y / WorldData.CHUNK_SIZE, voxel.z / WorldData.CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);
			int chunkIndex = VoxImporter.GetGridPos(chunkX, chunkY, chunkZ, WorldData.RelativeWorldVolume);
			int xFinal = (int)(voxel.x % WorldData.CHUNK_SIZE);
			int yFinal = (int)(voxel.y % WorldData.CHUNK_SIZE);
			int zFinal = (int)(voxel.z % WorldData.CHUNK_SIZE);
			int voxelGridPos = VoxImporter.GetGridPos(xFinal, yFinal, zFinal, WorldData.ChunkVolume);
			UnsafeHashMap<int, VoxelData> chunkHashMap = WorldDataPositions.ContainsKey(chunkIndex) ? WorldDataPositions[chunkIndex] : new UnsafeHashMap<int, VoxelData>(256, Allocator.Persistent);
			chunkHashMap[voxelGridPos] = new VoxelData((byte)xFinal, (byte)yFinal, (byte)zFinal, (byte)voxel.w);
			WorldDataPositions[chunkIndex] = chunkHashMap;
		}
	}
}
