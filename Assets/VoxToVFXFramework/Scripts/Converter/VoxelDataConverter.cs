using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine.SocialPlatforms;
using VoxToVFXFramework.Scripts.Data;
using VoxToVFXFramework.Scripts.Jobs;
using VoxToVFXFramework.Scripts.System;

namespace VoxToVFXFramework.Scripts.Converter
{
	public static class VoxelDataConverter
	{
		public static UnsafeList<VoxelVFX> Decode(int chunkIndex, ChunkVFX chunk, byte[] data)
		{
			int length = BitConverter.ToInt32(data, 0);
			NativeArray<byte> convertedBytes = new NativeArray<byte>(data, Allocator.TempJob);
			UnsafeList<VoxelVFX> list = new UnsafeList<VoxelVFX>(length, Allocator.Persistent);
			PhysicsShapeQuerySystem physicsShapeQuerySystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<PhysicsShapeQuerySystem>();
			EndSimulationEntityCommandBufferSystem ecbSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
			JobHandle job = new ImportVoxelDataJob()
			{
				Data = convertedBytes,
				Result = list.AsParallelWriter(),
				ChunkIndex = chunkIndex,
			}.Schedule(length, 64);
			job.Complete();

			if (chunk.LodLevel == 2)
			{
				EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				EntityArchetype entityArchetype = entityManager.CreateArchetype(
					typeof(Translation),
					typeof(PhysicsCollider),
					typeof(LocalToWorld),
					typeof(PhysicsWorldIndex));

				Entity prefab = entityManager.CreateEntity(entityArchetype);

				JobHandle createPhysicsEntityJob = new CreatePhysicsEntityJob()
				{
					ECB = ecbSystem.CreateCommandBuffer().AsParallelWriter(),
					Chunk = chunk,
					Collider = physicsShapeQuerySystem.BlobAssetReference,
					Data = list,
					PrefabEntity = prefab
				}.Schedule(list.Length, 64);
				createPhysicsEntityJob.Complete();
			}

			convertedBytes.Dispose();
			return list;
		}

		public static int ToInt32(NativeArray<byte> data, int startIndex)
		{
			return data[startIndex] | data[startIndex + 1] << 8 | data[startIndex + 2] << 16 | data[startIndex + 3] << 24;
		}

		public static short ToInt16(NativeArray<byte> data, int startIndex)
		{
			return (short)(data[startIndex] | data[startIndex + 1] << 8);
		}
	}
}
