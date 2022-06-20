using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEditor.UI;
using VoxToVFXFramework.Scripts.Converter;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.System;

namespace VoxToVFXFramework.Scripts.Jobs
{
	[BurstCompile]
	public struct ImportVoxelDataJob : IJobParallelFor
	{
		[ReadOnly] public NativeArray<byte> Data;
		[ReadOnly] public int ChunkIndex;
		[ReadOnly] public ChunkVFX Chunk;

		public UnsafeList<VoxelVFX>.ParallelWriter Result;
		[BurstDiscard]
		public void Execute(int index)
		{
			int offset = index * 6 + 4;
			byte posX = Data[offset++];
			byte posY = Data[offset++];
			byte posZ = Data[offset++];
			byte colorIndex = Data[offset++];
			VoxelFace face = (VoxelFace)VoxelDataConverter.ToInt16(Data, offset);

			VoxelVFX voxelVFX = new VoxelVFX()
			{
				position = (uint)((posX << 24) | (posY << 16) | (posZ << 8) | colorIndex),
				additionalData = (uint)((ushort)face << 16 | (ushort)ChunkIndex)
			};

			Result.AddNoResize(voxelVFX);

			uint faceUint = (uint)face;

			if (Chunk.LodLevel == 1)
			{
				PhysicsShapeQuerySystem physicsShapeQuerySystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<PhysicsShapeQuerySystem>();

				if ((faceUint & 1) != 0)
				{
					CreateEntity(posX, posY, posZ, physicsShapeQuerySystem.TopBlobAssetReference, new quaternion()
					{
						value = new float4(0.707f,0,0, 0.707f)
					});
				}

				if ((faceUint & 2) != 0)
				{
					CreateEntity(posX, posY, posZ, physicsShapeQuerySystem.RightBlobAssetReference, new quaternion()
					{
						value = new float4(0, -0.707f, 0, 0.707f)
					});
				}

				if ((faceUint & 4) != 0)
				{
					CreateEntity(posX, posY, posZ, physicsShapeQuerySystem.BottomBlobAssetReference, new quaternion()
					{
						value = new float4(0.707f, 0, 0, -0.707f)
					});
				}

				if ((faceUint & 8) != 0)
				{
					CreateEntity(posX, posY, posZ, physicsShapeQuerySystem.LeftBlobAssetReference, new quaternion()
					{
						value = new float4(0, .707f, 0, 0.707f)
					});
				}

				if ((faceUint & 16) != 0)
				{
					CreateEntity(posX, posY, posZ, physicsShapeQuerySystem.FrontBlobAssetReference, new quaternion()
					{
						value = new float4(0, 1, 0, 0)
					});
				}

				if ((faceUint & 32) != 0)
				{
					CreateEntity(posX, posY, posZ, physicsShapeQuerySystem.BackBlobAssetReference, new quaternion()
					{
						value = new float4(0, 0, 0, 1)
					});
				}
			}
		}

		private void CreateEntity(byte posX, byte posY, byte posZ, BlobAssetReference<Collider> colliderBlobAssetReference, quaternion rotation)
		{
			EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			EntityArchetype entityArchetype = entityManager.CreateArchetype(
				typeof(Translation),
				typeof(Rotation),
				typeof(RotationPivot),
				typeof(LocalToWorld),
				typeof(PhysicsCollider));

			Entity e = entityManager.CreateEntity(entityArchetype);
			entityManager.SetComponentData(e, new Translation()
			{
				Value = new float3(Chunk.WorldPosition.x + posX, Chunk.WorldPosition.y + posY, Chunk.WorldPosition.z + posZ)
			});

			entityManager.SetComponentData(e, new Rotation()
			{
				Value = rotation
			});

			entityManager.SetComponentData(e, new RotationPivot()
			{
				Value = new float3(0, 0, 0.5f)
			});

			entityManager.SetComponentData(e, new PhysicsCollider()
			{
				Value = colliderBlobAssetReference
			});
		}
	}
}
