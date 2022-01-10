using System;
using UnityEngine;
using UnityEngine.VFX;

namespace VoxToVFXFramework.Scripts.Data
{
	[VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    [Serializable]
    public struct VoxelVFX
    {
        public Vector3 position;
        public int paletteIndex;
    }

    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    [Serializable]
    public struct VoxelMaterialVFX
    {
        public Vector3 color;
        public float smoothness;
        public float metallic;
        public float emission;
    }
}
