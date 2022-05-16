using System;
using System.Runtime.InteropServices;
using System.Security;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Jobs;

namespace VoxToVFXFramework.Scripts.Converter
{
	public static class VoxelDataConverter
	{
		public static UnsafeList<VoxelVFX> Decode(int chunkIndex, byte[] data)
		{
			int length = BitConverter.ToInt32(data, 0);
			NativeArray<byte> convertedBytes = new NativeArray<byte>(data, Allocator.TempJob);
			UnsafeList<VoxelVFX> list = new UnsafeList<VoxelVFX>(length, Allocator.Persistent);

			JobHandle job = new VoxelDataConverterJob()
			{
				Data = convertedBytes,
				Result = list.AsParallelWriter(),
				ChunkIndex = chunkIndex
			}.Schedule(length, 64);
			job.Complete();

			convertedBytes.Dispose();
			return list;
		}

		public static int ToInt32(NativeArray<byte> data, int startIndex)
		{
			return data[startIndex] | data[startIndex+1] << 8 | data[startIndex+2] << 16 | data[startIndex+3] << 24;
		}

		public static short ToInt16(NativeArray<byte> data, int startIndex)
		{
			return (short)(data[startIndex] | data[startIndex + 1] << 8);
		}
	}
}
