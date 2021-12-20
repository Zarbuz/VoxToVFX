using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Color = FileToVoxCore.Drawing.Color;

namespace VoxToVFXFramework.Scripts.Data
{
    public class VoxelDataVFX
    {
        public CustomSchematic CustomSchematic;
        public VoxelMaterialVFX[] Materials;
    }


    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct VoxelVFX
    {
        public Vector3 position;
        public int paletteIndex;
    }

    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct VoxelMaterialVFX
    {
        public Vector3 color;
        public float smoothness;
        public float metallic;
        public float emission;
        public float alpha;
    }
}
