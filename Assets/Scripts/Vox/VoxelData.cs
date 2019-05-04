namespace VoxSlicer.Vox
{
    public class VoxelData
    {
        /// <summary>
        /// Creates a voxeldata with provided dimensions
        /// </summary>
        /// <param name="voxelWide"></param>
        /// <param name="voxelsTall"></param>
        /// <param name="voxelsDeep"></param>
        public VoxelData(int voxelWide, int voxelsTall, int voxelsDeep)
        {
            Resize(voxelWide, voxelsTall, voxelsDeep);
        }

        public VoxelData()
        {
        }

        public void Resize(int voxelsWide, int voxelsTall, int voxelsDeep)
        {
            VoxelsWide = voxelsWide;
            VoxelsTall = voxelsTall;
            VoxelsDeep = voxelsDeep;
            Colors = new byte[VoxelsWide * VoxelsTall * VoxelsDeep];
        }

        /// <summary>
        /// Gets a grid position from voxel coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public int GetGridPos(int x, int y, int z)
            => (VoxelsWide * VoxelsTall) * z + (VoxelsWide * y) + x;

        /// <summary>
        /// Set a color index from voxel coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Set(int x, int y, int z, byte value)
            => Colors[GetGridPos(x, y, z)] = value;

        /// <summary>
        /// Sets a colors index from grid position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Set(int x, byte value)
            => Colors[x] = value;

        /// <summary>
        /// Gets a palette index from voxel coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public int Get(int x, int y, int z)
            => Colors[GetGridPos(x, y, z)];

        /// <summary>
        /// Gets a palette index from a grid position
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public byte Get(int x) => Colors[x];

        /// <summary>
        /// Width of the data in voxels
        /// </summary>
        public int VoxelsWide { get; private set; }

        /// <summary>
        /// Height of the data in voxels
        /// </summary>
        public int VoxelsTall { get; private set; }

        /// <summary>
        /// Depth of the data in voxels
        /// </summary>
        public int VoxelsDeep { get; private set; }

        public byte[] Colors { get; private set; }

        public bool Contains(int x, int y, int z)
            => x >= 0 && y >= 0 && z >= 0 && x < VoxelsWide && y < VoxelsTall && z < VoxelsDeep;

        public byte GetSafe(int x, int y, int z)
            => Contains(x, y, z) ? Colors[GetGridPos(x, y, z)] : (byte)0;
    }
}
