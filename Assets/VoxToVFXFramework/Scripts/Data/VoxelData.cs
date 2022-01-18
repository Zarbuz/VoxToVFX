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

	public struct VoxelResult
	{
		public NativeList<Vector4> DataLod0;
		public NativeList<Vector4> DataLod1;
		public NativeList<Vector4> DataLod2;
		public NativeList<Vector4> DataLod3;
		public Vector3 FrameWorldPosition; //0 0 0
	}
}
