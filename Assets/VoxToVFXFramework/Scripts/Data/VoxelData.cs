using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Color = FileToVoxCore.Drawing.Color;

namespace VoxToVFXFramework.Scripts.Data
{
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    [Serializable]
    public struct VoxelVFX
    {
        public Vector3 position;
        public int paletteIndex;
        public int rotationIndex;

        public bool IsTransparent(VoxelMaterialVFX[] materials)
        {
	        return materials[paletteIndex].alpha < 1;
        }
    }

    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    [Serializable]
    public struct VoxelMaterialVFX
    {
        public Vector3 color;
        public float smoothness;
        public float metallic;
        public float emission;
        public float alpha;
        public float softParticleFadeDistance;
    }

    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct VoxelRotationVFX
    {
	    public Vector3 rotation;
	    public Vector3 pivot;
    }
}
