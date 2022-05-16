using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using VoxToVFXFramework.Scripts.Converter;
using VoxToVFXFramework.Scripts.Data;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct VoxelDataConverterJob : IJobParallelFor
	{
		[ReadOnly] public NativeArray<byte> Data;
		[ReadOnly] public int ChunkIndex;

		public UnsafeList<VoxelVFX>.ParallelWriter Result;
		public void Execute(int index)
		{
			int offset = index * 6 + 4;
			byte posX = Data[offset++];
			byte posY = Data[offset++];
			byte posZ = Data[offset++];
			byte colorIndex = Data[offset++];
			VoxelFace face = (VoxelFace)VoxelDataConverter.ToInt16(Data, offset);

			VoxelVFX voxelVFX = new VoxelVFX()
			{
				position = (uint)((posX << 24) | (posY << 16) | (posZ << 8) | colorIndex),
				additionalData = (uint)((ushort)face << 16 | (ushort)ChunkIndex)
			};

			Result.AddNoResize(voxelVFX);
		}
	}
}
