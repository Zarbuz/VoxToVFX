using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using VoxToVFXFramework.Scripts.Data;

namespace VoxToVFXFramework.Scripts.Extensions
{
	public static class VoxelVFXExtensions
	{
		public static string DecodePosition(this VoxelVFX voxel)
		{
			uint x = voxel.position >> 24;
			uint y = (voxel.position & 0xff0000) >> 16;
			uint z = (voxel.position & 0xff00) >> 8;
			uint colorIndex = (voxel.position & 0xff);

			return $"x:{x} y:{y} z:{z} colorIndex: {colorIndex}";
		}

		public static string DecodeAdditionalData(this VoxelVFX voxel)
		{
			uint rotationIndex = voxel.additionalData >> 16;
			uint chunkIndex = voxel.additionalData & 0x0000FFFF;

			return $"rotationIndex:{rotationIndex} chunkIndex: {chunkIndex}";
		}

		public static uint CountVoxelFaceFlags(this VoxelFace voxelFace)
		{
			uint v = (uint)voxelFace;
			v = v - ((v >> 1) & 0x55555555);
			v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
			uint count = ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveInlining)]
		private static unsafe Boolean HasFlags<T>(T* first, T* second) where T : unmanaged, Enum
		{
			byte* pf = (byte*)first;
			byte* ps = (byte*)second;

			for (int i = 0; i < sizeof(T); i++)
			{
				if ((pf[i] & ps[i]) != ps[i])
				{
					return false;
				}
			}

			return true;
		}

		/// <remarks>Faster analog of Enum.HasFlag</remarks>
		/// <inheritdoc cref="Enum.HasFlag"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe bool HasFlags<T>(this T first, T second) where T : unmanaged, Enum
		{
			return HasFlags(&first, &second);
		}
	}
}
