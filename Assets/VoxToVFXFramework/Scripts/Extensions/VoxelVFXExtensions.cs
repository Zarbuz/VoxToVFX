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

			return $"x:{x} y:{y} z:{z}";
		}

		public static string DecodeAdditionalData(this VoxelVFX voxel)
		{
			uint colorIndex = voxel.additionalData >> 24;
			uint chunkIndex = (voxel.additionalData & 0xff0000) >> 16;
			uint rotationIndex = (voxel.additionalData & 0xff00) >> 8;

			return $"colorIndex: {colorIndex} chunkIndex:{chunkIndex} rotationIndex:{rotationIndex}";
		}
	}
}
