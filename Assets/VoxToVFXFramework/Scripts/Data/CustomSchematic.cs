using FileToVoxCore.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace VoxToVFXFramework.Scripts.Data
{
	public class Region
	{
		public NativeArray<Vector4> BlockDict { get; set; } = new NativeArray<Vector4>();
	}


	public class CustomSchematic : IDisposable
	{
		#region ConstStatic

		public const int MAX_WORLD_WIDTH = 2000;
		public const int MAX_WORLD_HEIGHT = 2000;
		public const int MAX_WORLD_LENGTH = 2000;

		public static int CHUNK_SIZE = 500;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetVoxelIndex(int x, int y, int z)
		{
			return (y * MAX_WORLD_LENGTH + z) * MAX_WORLD_WIDTH + x;
		}


		#endregion

		#region Fields


		public ConcurrentDictionary<long, Region> RegionDict { get; private set; } = new ConcurrentDictionary<long, Region>();

		private ushort mWidth;

		public ushort Width
		{
			get
			{
				mWidth = (ushort)(MaxX - MinX + 1);
				return mWidth;
			}
		}

		private ushort mHeight;

		public ushort Height
		{
			get
			{

				mHeight = (ushort)(MaxY - MinY + 1);
				return mHeight;
			}
		}

		private ushort mLength;

		public ushort Length
		{
			get
			{
				mLength = (ushort)(MaxZ - MinZ + 1);
				return mLength;
			}
		}

		public int MinX { get; private set; }
		public int MinY { get; private set; }
		public int MinZ { get; private set; }

		public int MaxX { get; private set; }
		public int MaxY { get; private set; }
		public int MaxZ { get; private set; }


		#endregion

		#region PublicMethods


		public void AddVoxel(int x, int y, int z, int palettePosition)
		{
			if (x < MAX_WORLD_WIDTH && y < MAX_WORLD_HEIGHT && z < MAX_WORLD_LENGTH)
			{
				AddUsageForRegion(x, y, z, palettePosition);
				ComputeMinMax(x, y, z);
			}
			else
			{
				Debug.LogError($"[CustomSchematic] Invalid coordinate when AddVoxel: x:{x} y:{y} z:{z}");
			}
		}

		

		public void Dispose()
		{
			foreach (Region region in RegionDict.Values)
			{
				region.BlockDict.Dispose();
			}
			RegionDict.Clear();
			RegionDict = null;
		}

		#endregion

		#region PrivateMethods

		private void AddUsageForRegion(int x, int y, int z, int paletteIndex)
		{
			//FastMath.FloorToInt(x / CHUNK_SIZE, y / CHUNK_SIZE, z / CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);

			//int chunkIndex = GetVoxelIndex(chunkX, chunkY, chunkZ);
			//int voxelIndex = GetVoxelIndex(x, y, z);

			//VoxelVFX voxelVfx = new VoxelVFX();
			//voxelVfx.position = ;
			//voxelVfx.paletteIndex = paletteIndex;

			//if (!RegionDict.ContainsKey(chunkIndex))
			//{
			//	RegionDict[chunkIndex] = new Region();
			//}
			//NativeArray<Vector4> test= new NativeArray<Vector4>();
			//test[0] = Vector3.back;

			//RegionDict[chunkIndex].BlockDict[voxelIndex] = new Vector3(x, y, z);
		}

		private void ComputeMinMax(int x, int y, int z)
		{
			MinX = Mathf.Min(x, MinX);
			MinY = Mathf.Min(y, MinY);
			MinZ = Mathf.Min(z, MinZ);


			MaxX = Mathf.Max(x, MaxX);
			MaxY = Mathf.Max(y, MaxY);
			MaxZ = Mathf.Max(z, MaxZ);
		}

		

		#endregion
	}
}
