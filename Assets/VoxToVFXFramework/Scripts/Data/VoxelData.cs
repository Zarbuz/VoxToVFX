using System;
using System.Collections.Generic;
using System.Linq;
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
		public int lodLevel;
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

	public struct Chunk
	{
		public Vector3 Position;
		public int ChunkIndex;
		public int LodLevel;
		public int IsActive;
		public int Length;
	}

	public struct ChunkDataFile
	{
		public string Filename;
		public Vector3 Position;
		public int ChunkIndex;
		public int LodLevel;
		public int Length;
	}

	public struct VoxelData
	{
		public byte PosX;
		public byte PosY;
		public byte PosZ;
		public byte Color;

		public VoxelData(byte posX, byte posY, byte posZ, byte color)
		{
			PosX = posX;
			PosY = posY;
			PosZ = posZ;
			Color = color;
		}
	}

	public struct VoxelResult
	{
		public int ChunkIndex;
		public int LodLevel;
		public NativeArray<VoxelData> Data;
		public Vector3 FrameWorldPosition;
	}
}
