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

	public struct ChunkData
	{
		public NativeArray<VoxelVFX> Data;
		public int LodLevel;
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

	public struct VoxelResult
	{
		public int ChunkIndex;
		public int LodLevel;
		public NativeArray<Vector4> Data;
		public Vector3 FrameWorldPosition;
	}

	public class ChunkParent
	{
		public List<ChunkData> ChunkData;

		public ChunkParent()
		{
			ChunkData = new List<ChunkData>();
		}

		public ChunkData GetChunkForLodLevel(int lodLevel)
		{
			return ChunkData.FirstOrDefault(chunk => chunk.LodLevel == lodLevel);
		}
	}
}
