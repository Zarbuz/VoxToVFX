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
	[BurstCompile]
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

			CreateEntity(index, posX, posY, posZ);
		}

		private void CreateEntity(int index, uint posX, uint posY, uint posZ)
		{
			Entity newEntity = ECB.Instantiate(index, PrefabEntity);

			ECB.SetComponent(index, newEntity, new Translation()
			{
				Value = new float3(Chunk.WorldPosition.x + posX, Chunk.WorldPosition.y + posY, Chunk.WorldPosition.z + posZ)
			});

			ECB.SetComponent(index, newEntity, new PhysicsCollider()
			{
				Value = Collider
			});
		}

	}
}
