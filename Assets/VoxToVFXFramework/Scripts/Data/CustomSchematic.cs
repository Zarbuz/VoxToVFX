using FileToVoxCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace VoxToVFXFramework.Scripts.Data
{
    public class Region
	{
		public int X;
		public int Y;
		public int Z;
		public Dictionary<long, VoxelVFX> BlockDict { get; set; }

		public Region(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
			BlockDict = new Dictionary<long, VoxelVFX>();
		}

		public override string ToString()
		{
			return $"{X} {Y} {Z}";
		}
    }


	public class CustomSchematic
	{
		#region ConstStatic

		public const int MAX_WORLD_WIDTH = 2000;
		public const int MAX_WORLD_HEIGHT = 1000;
		public const int MAX_WORLD_LENGTH = 2000;

		public static int CHUNK_SIZE = 64;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long GetVoxelIndex(int x, int y, int z)
		{
			return (y * MAX_WORLD_LENGTH + z) * MAX_WORLD_WIDTH + x;
		}

		
		#endregion

		#region Fields

		
		public Dictionary<long, Region> RegionDict { get; private set; }

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

		public CustomSchematic()
        {
            CreateAllRegions();
        }

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
		
		public bool GetVoxel(int x, int y, int z, ref VoxelVFX voxel)
		{
			FastMath.FloorToInt(x / CHUNK_SIZE, y / CHUNK_SIZE, z / CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);

			long chunkIndex = GetVoxelIndex(chunkX, chunkY, chunkZ);
			long voxelIndex = GetVoxelIndex(x, y, z);
			if (RegionDict.ContainsKey(chunkIndex))
			{
				bool found = RegionDict[chunkIndex].BlockDict.TryGetValue(voxelIndex, out VoxelVFX foundVoxel);
				voxel = foundVoxel;
				return found;
			}

            return false;
        }

		public List<Region> GetAllRegions()
		{
			return RegionDict.Values.Where(region => region.BlockDict.Count > 0).ToList();
		}

		public List<VoxelVFX> GetAllVoxels()
		{
			List<VoxelVFX> voxels = new List<VoxelVFX>();
			foreach (KeyValuePair<long, Region> region in RegionDict)
			{
				voxels.AddRange(region.Value.BlockDict.Values);
			}

			return voxels;
		}

		#endregion

		#region PrivateMethods

		private void CreateAllRegions()
		{
			RegionDict = new Dictionary<long, Region>();

            //        for (int x = -HALF_MAX_WORLD_WIDTH; x <= HALF_MAX_WORLD_WIDTH; x+= CHUNK_SIZE)
            //        {
            //            for (int y = -HALF_MAX_WORLD_HEIGHT; y <= HALF_MAX_WORLD_HEIGHT; y += CHUNK_SIZE)
            //            {
            //	for (int z = -HALF_MAX_WORLD_LENGTH; z <= HALF_MAX_WORLD_LENGTH; z += CHUNK_SIZE)
            //                {
            //        RegionDict[GetVoxelIndex(x, y, z)] = new Region(x, y, z);
            //	}
            //}
            //        }

            int worldRegionX = (int)Math.Ceiling((decimal)MAX_WORLD_WIDTH / CHUNK_SIZE);
            int worldRegionY = (int)Math.Ceiling((decimal)MAX_WORLD_HEIGHT / CHUNK_SIZE);
            int worldRegionZ = (int)Math.Ceiling((decimal)MAX_WORLD_LENGTH / CHUNK_SIZE);

            int countSize = worldRegionX * worldRegionY * worldRegionZ;

            for (int i = 0; i < countSize; i++)
            {
                int x = i % worldRegionX;
                int y = (i / worldRegionX) % worldRegionY;
                int z = i / (worldRegionX * worldRegionY);

                //Debug.Log($"x: {x} y: {y} z: {z}");
                RegionDict[GetVoxelIndex(x, y, z)] = new Region(x * CHUNK_SIZE, y * CHUNK_SIZE, z * CHUNK_SIZE);
            }
        }

		private void AddUsageForRegion(int x, int y, int z, int paletteIndex)
		{
			FastMath.FloorToInt(x / CHUNK_SIZE, y / CHUNK_SIZE, z / CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);

			long chunkIndex = GetVoxelIndex(chunkX, chunkY, chunkZ);
			long voxelIndex = GetVoxelIndex(x, y, z);

            VoxelVFX voxelVfx = new VoxelVFX();
            voxelVfx.position = new Vector3(x, y, z);
            voxelVfx.paletteIndex = paletteIndex;

            RegionDict[chunkIndex].BlockDict[voxelIndex] = voxelVfx;
        }

        private void ComputeMinMax(int x, int y, int z)
        {
            MinX = Math.Min(x, MinX);
            MinY = Math.Min(y, MinY);
            MinZ = Math.Min(z, MinZ);


            MaxX = Math.Max(x, MaxX);
            MaxY = Math.Max(y, MaxY);
            MaxZ = Math.Max(z, MaxZ);
        }

		#endregion
	}
}
