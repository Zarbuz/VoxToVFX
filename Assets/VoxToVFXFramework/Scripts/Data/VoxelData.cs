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
		public uint position; // x (8) y (8) z (8) colorIndex (8)
		public uint additionalData; // rotationIndex (16 bits), chunkIndex (16 bits)
		//public uint chunkIndex;
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

	[Flags]
	public enum VoxelFace : short
	{
		None = 0,
		Top = 1,
		Right = 2,
		Bottom = 4,
		Left = 8,
		Front = 16,
		Back = 32
	}

	public struct VoxelData
	{
		public byte PosX;
		public byte PosY;
		public byte PosZ;
		public byte ColorIndex;
		public VoxelFace Face;

		public VoxelData(byte posX, byte posY, byte posZ, byte colorIndex)
		{
			PosX = posX;
			PosY = posY;
			PosZ = posZ;
			ColorIndex = colorIndex;
			Face = VoxelFace.None;
		}

		public VoxelData(byte posX, byte posY, byte posZ, byte colorIndex, VoxelFace face)
		{
			PosX = posX;
			PosY = posY;
			PosZ = posZ;
			ColorIndex = colorIndex;
			Face = face;
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
