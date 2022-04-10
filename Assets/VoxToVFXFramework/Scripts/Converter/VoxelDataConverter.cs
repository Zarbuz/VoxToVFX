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
		public static NativeArray<VoxelData> Decode(byte[] data)
		{
			int length = BitConverter.ToInt32(data, 0);
			NativeArray<byte> convertedBytes = new NativeArray<byte>(data, Allocator.TempJob);
			NativeArray<VoxelData> array = new NativeArray<VoxelData>(length, Allocator.Persistent);

			JobHandle job = new VoxelDataConverterJob()
			{
				Data = convertedBytes,
				Result = array
			}.Schedule(length, 64);
			job.Complete();

			convertedBytes.Dispose();
			//int offset = 4;
			//for (int i = 0; i < length; i++)
			//{
			//	byte posX = data[offset++];
			//	byte posY = data[offset++];
			//	byte posZ = data[offset++];
			//	byte colorIndex = data[offset++];
			//	VoxelFace face = (VoxelFace)Enum.Parse<VoxelFace>(BitConverter.ToInt16(data, offset).ToString());
			//	offset += 2;
			//	//array[i] = new VoxelData(posX, posY, posZ, colorIndex, face);
			//}

			return array;
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
