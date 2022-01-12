using System;
using Unity.Collections;
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

	public struct LodParameters
	{
		public Vector3 VolumeSize;
		public NativeHashMap<int, Vector4> Data;
		public NativeArray<byte> Work;
	}

	[VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
	public struct VoxelRotationVFX
	{
		public Vector3 rotation;
		public Vector3 pivot;
	}
}
