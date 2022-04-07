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
		public uint position;
		public uint additionalData; //paletteIndex, chunkIndex, rotationIndex
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

	[VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
	[Serializable]
	public struct VoxelRotationVFX
	{
		public Vector3 rotation;
		public Vector3 pivot;
	}

	[VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
	[Serializable]
	public struct ChunkVFX
	{
		public Vector3 CenterWorldPosition;
		public Vector3 WorldPosition;
		public int ChunkIndex;
		public int LodLevel;
		public int IsActive;
		public int Length;
	}

	public struct ChunkDataFile
	{
		public string Filename;
		public Vector3 WorldCenterPosition;
		public Vector3 WorldPosition;
		public int ChunkIndex;
		public int LodLevel;
		public int Length;
	}

	public struct VoxelData
	{
		public byte PosX;
		public byte PosY;
		public byte PosZ;
		public byte ColorIndex;
		public byte RotationIndex;

		public VoxelData(byte posX, byte posY, byte posZ, byte colorIndex, byte rotationIndex = 0)
		{
			PosX = posX;
			PosY = posY;
			PosZ = posZ;
			ColorIndex = colorIndex;
			RotationIndex = rotationIndex;
		}
	}

	public struct VoxelResult
	{
		public int ChunkIndex;
		public int LodLevel;
		public NativeList<VoxelData> Data;
		public Vector3 ChunkCenterWorldPosition;
		public Vector3 ChunkWorldPosition;
	}
}
