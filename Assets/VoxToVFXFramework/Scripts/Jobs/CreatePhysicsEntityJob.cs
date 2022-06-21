using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using VoxToVFXFramework.Scripts.Data;

namespace VoxToVFXFramework.Scripts.Jobs
{
	//[BurstCompile]
	public struct CreatePhysicsEntityJob: IJobParallelFor
	{
		[ReadOnly] public UnsafeList<VoxelVFX> Data;
		[ReadOnly] public ChunkVFX Chunk;
		[ReadOnly] public BlobAssetReference<Collider> Collider;
		[ReadOnly] public Entity PrefabEntity;
		public EntityCommandBuffer.ParallelWriter ECB;

		public void Execute(int index)
		{
			VoxelVFX voxel = Data[index];

			uint posX = voxel.position >> 24;
			uint posY = (voxel.position & 0xff0000) >> 16;
			uint posZ = (voxel.position & 0xff00) >> 8;

			uint rotationIndex = voxel.additionalData >> 16;

			if ((rotationIndex & 1) != 0)
			{
				CreateEntity(index, posX, posY, posZ, new quaternion()
				{
					value = new float4(0.707f, 0, 0, 0.707f)
				});
			}

			if ((rotationIndex & 2) != 0)
			{
				CreateEntity(index, posX, posY, posZ, new quaternion()
				{
					value = new float4(0, -0.707f, 0, 0.707f)
				});
			}

			if ((rotationIndex & 4) != 0)
			{
				CreateEntity(index, posX, posY, posZ, new quaternion()
				{
					value = new float4(0.707f, 0, 0, -0.707f)
				});
			}

			if ((rotationIndex & 8) != 0)
			{
				CreateEntity(index, posX, posY, posZ, new quaternion()
				{
					value = new float4(0, .707f, 0, 0.707f)
				});
			}

			if ((rotationIndex & 16) != 0)
			{
				CreateEntity(index, posX, posY, posZ, new quaternion()
				{
					value = new float4(0, 1, 0, 0)
				});
			}

			if ((rotationIndex & 32) != 0)
			{
				CreateEntity(index, posX, posY, posZ, new quaternion()
				{
					value = new float4(0, 0, 0, 1)
				});
			}
		}

		private void CreateEntity(int index, uint posX, uint posY, uint posZ, quaternion rotation)
		{
			Entity newEntity = ECB.Instantiate(index, PrefabEntity);

			ECB.AddComponent<LocalToWorld>(index, newEntity);
			ECB.AddComponent<Translation>(index, newEntity);
			ECB.SetComponent(index, newEntity, new Translation()
			{
				Value = new float3(Chunk.WorldPosition.x + posX, Chunk.WorldPosition.y + posY, Chunk.WorldPosition.z + posZ)
			});

			ECB.AddComponent<Rotation>(index, newEntity);
			ECB.SetComponent(index, newEntity, new Rotation()
			{
				Value = rotation
			});

			ECB.AddComponent<PhysicsCollider>(index, newEntity);
			ECB.SetComponent(index, newEntity, new PhysicsCollider()
			{
				Value = Collider
			});

		}

	}
}
