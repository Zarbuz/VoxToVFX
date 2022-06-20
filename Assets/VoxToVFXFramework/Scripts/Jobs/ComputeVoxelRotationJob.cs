using FileToVoxCore.Utils;
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
		[ReadOnly] public UnsafeParallelHashMap<int, VoxelData> Data;
		[ReadOnly] public NativeArray<VoxelMaterialVFX> Materials;
		[ReadOnly] public UnsafeParallelHashMap<int, UnsafeParallelHashMap<int, VoxelData>> WorldDataPositions;
		[ReadOnly] public int Step;

		[ReadOnly] public int3 VolumeSize;
		[ReadOnly] public int3 WorldChunkPosition;

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
			VoxelData vLeft, vBack, vRight, vTop, vBottom, vFront;
			bool left, right, top, bottom, front, back;

			if (Step == 1)
			{
				FastMath.FloorToInt(
					(v.PosX - Step + WorldChunkPosition.x) / (float)WorldData.CHUNK_SIZE,
					(v.PosY + WorldChunkPosition.y) / (float)WorldData.CHUNK_SIZE,
					(v.PosZ + WorldChunkPosition.z) / (float)WorldData.CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);

				int chunkIndexLeft = VoxImporter.GetGridPos(chunkX, chunkY, chunkZ, WorldData.RelativeWorldVolume);

				if (WorldDataPositions.ContainsKey(chunkIndexLeft))
				{
					int x = v.PosX == 0 ? WorldData.CHUNK_SIZE - 1 : v.PosX - Step;
					int voxelGridPos = VoxImporter.GetGridPos(x, v.PosY, v.PosZ, VolumeSize);
					left = WorldDataPositions[chunkIndexLeft].TryGetValue(voxelGridPos, out vLeft);
				}
				else
				{
					vLeft = default;
					left = false;
				}

				FastMath.FloorToInt(
					(v.PosX + Step + WorldChunkPosition.x) / (float)WorldData.CHUNK_SIZE,
					(v.PosY + WorldChunkPosition.y) / (float)WorldData.CHUNK_SIZE,
					(v.PosZ + WorldChunkPosition.z) / (float)WorldData.CHUNK_SIZE, out chunkX, out chunkY, out chunkZ);

				int chunkIndexRight = VoxImporter.GetGridPos(chunkX, chunkY, chunkZ, WorldData.RelativeWorldVolume);

				if (WorldDataPositions.ContainsKey(chunkIndexRight))
				{
					int x = v.PosX == WorldData.CHUNK_SIZE - 1 ? 0 : v.PosX + Step;
					int voxelGridPos = VoxImporter.GetGridPos(x, v.PosY, v.PosZ, VolumeSize);
					right = WorldDataPositions[chunkIndexRight].TryGetValue(voxelGridPos, out vRight);
				}
				else
				{
					vRight = default;
					right = false;
				}

				FastMath.FloorToInt(
					(v.PosX + WorldChunkPosition.x) / (float)WorldData.CHUNK_SIZE,
					(v.PosY + Step + WorldChunkPosition.y) / (float)WorldData.CHUNK_SIZE,
					(v.PosZ + WorldChunkPosition.z) / (float)WorldData.CHUNK_SIZE, out chunkX, out chunkY, out chunkZ);

				int chunkIndexTop = VoxImporter.GetGridPos(chunkX, chunkY, chunkZ, WorldData.RelativeWorldVolume);

				if (WorldDataPositions.ContainsKey(chunkIndexTop))
				{
					int y = v.PosY == WorldData.CHUNK_SIZE - 1 ? 0 : v.PosY + Step;
					int voxelGridPos = VoxImporter.GetGridPos(v.PosX, y, v.PosZ, VolumeSize);
					top = WorldDataPositions[chunkIndexTop].TryGetValue(voxelGridPos, out vTop);
				}
				else
				{
					vTop = default;
					top = false;
				}

				FastMath.FloorToInt(
					(v.PosX + WorldChunkPosition.x) / (float)WorldData.CHUNK_SIZE,
					(v.PosY - Step + WorldChunkPosition.y) / (float)WorldData.CHUNK_SIZE,
					(v.PosZ + WorldChunkPosition.z) / (float)WorldData.CHUNK_SIZE, out chunkX, out chunkY, out chunkZ);

				int chunkIndexBottom = VoxImporter.GetGridPos(chunkX, chunkY, chunkZ, WorldData.RelativeWorldVolume);

				if (WorldDataPositions.ContainsKey(chunkIndexBottom))
				{
					int y = v.PosY == 0 ? WorldData.CHUNK_SIZE - 1 : v.PosY - Step;
					int voxelGridPos = VoxImporter.GetGridPos(v.PosX, y, v.PosZ, VolumeSize);
					bottom = WorldDataPositions[chunkIndexBottom].TryGetValue(voxelGridPos, out vBottom);
				}
				else
				{
					vBottom = default;
					bottom = false;
				}

				FastMath.FloorToInt(
					(v.PosX + WorldChunkPosition.x) / (float)WorldData.CHUNK_SIZE,
					(v.PosY + WorldChunkPosition.y) / (float)WorldData.CHUNK_SIZE,
					(v.PosZ + Step + WorldChunkPosition.z) / (float)WorldData.CHUNK_SIZE, out chunkX, out chunkY, out chunkZ);

				int chunkIndexFront = VoxImporter.GetGridPos(chunkX, chunkY, chunkZ, WorldData.RelativeWorldVolume);

				if (WorldDataPositions.ContainsKey(chunkIndexFront))
				{
					int z = v.PosZ == WorldData.CHUNK_SIZE - 1 ? 0 : v.PosZ + Step;
					int voxelGridPos = VoxImporter.GetGridPos(v.PosX, v.PosY, z, VolumeSize);
					front = WorldDataPositions[chunkIndexFront].TryGetValue(voxelGridPos, out vFront);
				}
				else
				{
					vFront = default;
					front = false;
				}

				FastMath.FloorToInt(
					(v.PosX + WorldChunkPosition.x) / (float)WorldData.CHUNK_SIZE,
					(v.PosY + WorldChunkPosition.y) / (float)WorldData.CHUNK_SIZE,
					(v.PosZ - Step + WorldChunkPosition.z) / (float)WorldData.CHUNK_SIZE, out chunkX, out chunkY, out chunkZ);

				int chunkIndexBack = VoxImporter.GetGridPos(chunkX, chunkY, chunkZ, WorldData.RelativeWorldVolume);

				if (WorldDataPositions.ContainsKey(chunkIndexBack))
				{
					int z = v.PosZ == 0 ? WorldData.CHUNK_SIZE - 1 : v.PosZ - Step;
					int voxelGridPos = VoxImporter.GetGridPos(v.PosX, v.PosY, z, VolumeSize);
					back = WorldDataPositions[chunkIndexBack].TryGetValue(voxelGridPos, out vBack);
				}
				else
				{
					vBack = default;
					back = false;
				}
			}
			else
			{
				left = Data.TryGetValue(VoxImporter.GetGridPos(v.PosX - Step, v.PosY, v.PosZ, VolumeSize), out vLeft);
				right = Data.TryGetValue(VoxImporter.GetGridPos(v.PosX + Step, v.PosY, v.PosZ, VolumeSize), out vRight);
				top = Data.TryGetValue(VoxImporter.GetGridPos(v.PosX, v.PosY + Step, v.PosZ, VolumeSize), out vTop);
				bottom = Data.TryGetValue(VoxImporter.GetGridPos(v.PosX, v.PosY - Step, v.PosZ, VolumeSize), out vBottom);
				front = Data.TryGetValue(VoxImporter.GetGridPos(v.PosX, v.PosY, v.PosZ + Step, VolumeSize), out vFront);
				back = Data.TryGetValue(VoxImporter.GetGridPos(v.PosX, v.PosY, v.PosZ - Step, VolumeSize), out vBack);
			}


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
